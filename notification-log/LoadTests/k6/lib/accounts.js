import http from 'k6/http';
import { sleep } from 'k6';
import { accounts, urls } from './config.js';
import { adminSession, email, login, tryLogin } from './auth.js';
import { kindOf } from './http.js';

const batchSize = 25;
const timeoutMs = 120000;
const pollMs = 500;
const clockSkewMs = 5000;

export function ensureAccounts() {
  const all = [
    ...range('visitor', 'Visitante', accounts.visitors),
    ...range('owner', 'Propietario', accounts.owners, `/connect/authorize?client_id=${accounts.ownerClientId}`),
    ...range('moderator', 'Moderador', accounts.moderators),
  ];

  const missing = all.filter((account) => !tryLogin(account.email));

  for (let i = 0; i < missing.length; i += batchSize) {
    registerBatch(missing.slice(i, i + batchSize));
  }

  promoteModerators();
  waitForReplicas();

  console.log(`Cuentas listas: ${all.length - missing.length} existentes y ${missing.length} registradas.`);
}

function range(prefix, label, count, returnUrl) {
  return Array.from({ length: count }, (_, i) => ({
    email: email(prefix, i + 1),
    name: `${label} de carga ${String(i + 1).padStart(4, '0')}`,
    returnUrl,
  }));
}

function registerBatch(batch) {
  const token = antiforgeryToken(`${urls.identity}/Account/Register`);
  const since = Date.now() - clockSkewMs;

  const registered = http.batch(batch.map((account) => form('/Account/Register', {
    __RequestVerificationToken: token,
    Name: account.name,
    Identifier: account.email,
    Password: accounts.password,
    ConfirmPassword: accounts.password,
    AcceptsNotifications: 'true',
    ReturnUrl: account.returnUrl || '',
  }, 'POST /Account/Register')));

  registered.forEach((response, i) => expectPage(response, '/Account/Verify', `registrar a ${batch[i].email}`));

  const codes = waitForCodes(batch.map((account) => account.email), since);

  const verified = http.batch(batch.map((account) => form('/Account/Verify', {
    __RequestVerificationToken: token,
    Identifier: account.email,
    Code: codes[account.email],
    ReturnUrl: account.returnUrl || '',
  }, 'POST /Account/Verify')));

  verified.forEach((response, i) => expectPage(response, '/Account/Login', `verificar a ${batch[i].email}`));
}

function promoteModerators() {
  const admin = adminSession();

  for (let i = 1; i <= accounts.moderators; i++) {
    const user = login(email('moderator', i));
    const response = http.put(
      `${urls.identity}/api/users/${user.userId}/role`,
      JSON.stringify({ role: 'Moderador' }),
      {
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${admin.accessToken}` },
        tags: { name: 'PUT /api/users/{id}/role', kind: kindOf('write') },
      },
    );

    if (response.status !== 204) {
      throw new Error(`No se pudo dar el rol de moderador a ${email('moderator', i)}: ${response.status} ${response.body}`);
    }
  }
}

function waitForReplicas() {
  const visitor = login(email('visitor', accounts.visitors));
  const owner = login(email('owner', accounts.owners));
  const admin = adminSession();

  waitFor(`Rentals no conoce a ${email('visitor', accounts.visitors)}`, () =>
    authorizedGet(`${urls.rental}/api/visitors/${visitor.userId}`, visitor, 'GET /api/visitors/{id}').status === 200);

  waitFor(`Notification no conoce a ${email('owner', accounts.owners)}`, () =>
    authorizedGet(`${urls.notification}/api/recipients/${owner.userId}`, admin, 'GET /api/recipients/{id}').status === 200);
}

function waitForCodes(addresses, since) {
  const found = {};

  waitFor(`No llegó el código de verificación a ${addresses.join(', ')}`, () => {
    for (const event of mailboxEvents()) {
      const to = JSON.stringify((event.payload && event.payload.to) || '').toLowerCase();
      const address = addresses.find((candidate) => to.includes(candidate.toLowerCase()));
      const at = timestampMs(event.timestamp);

      if (!address || at < since || (found[address] && found[address].at >= at)) continue;

      const code = codeIn(event);

      if (code) found[address] = { code, at };
    }

    return addresses.every((address) => found[address]);
  });

  return Object.fromEntries(addresses.map((address) => [address, found[address].code]));
}

function mailboxEvents() {
  const response = http.get(`${urls.mailbox}/api/events?type=smtp`, {
    tags: { name: 'GET /api/events (buzón)', kind: kindOf('read') },
    responseCallback: http.expectedStatuses(200),
  });

  return response.status === 200 ? response.json('data') || [] : [];
}

function codeIn(event) {
  let payload = event.payload || {};

  if (!payload.html && !payload.text) {
    const response = http.get(`${urls.mailbox}/api/event/${event.uuid}`, {
      tags: { name: 'GET /api/event/{uuid} (buzón)', kind: kindOf('read') },
    });

    if (response.status === 200) payload = response.json('payload') || response.json('data.payload') || {};
  }

  const html = String(payload.html || '').match(/>\s*(\d{6})\s*</);
  const text = String(payload.text || '').match(/\b(\d{6})\b/);

  return (html && html[1]) || (text && text[1]) || null;
}

function timestampMs(value) {
  const number = Number(value);

  if (!number) return Date.parse(value) || Number.MAX_SAFE_INTEGER;

  return number < 1e12 ? number * 1000 : number;
}

function antiforgeryToken(url) {
  const page = http.get(url, { tags: { name: 'GET /Account/Register', kind: kindOf('read') } });
  const token = page.html().find('input[name="__RequestVerificationToken"]').attr('value');

  if (!token) throw new Error(`La página ${url} no trae el token antiforgery: ${page.status}`);

  return token;
}

function form(path, fields, name) {
  return {
    method: 'POST',
    url: `${urls.identity}${path}`,
    body: fields,
    params: { tags: { name, kind: kindOf('write') } },
  };
}

function expectPage(response, path, action) {
  if (response.status === 200 && response.url.includes(path)) return;

  const error = response.html().find('.alert-error').text().trim();

  throw new Error(`No se pudo ${action}: ${response.status} ${error || response.url}`);
}

function authorizedGet(url, session, name) {
  return http.get(url, {
    headers: { Authorization: `Bearer ${session.accessToken}` },
    tags: { name, kind: kindOf('read') },
    responseCallback: http.expectedStatuses(200, 404),
  });
}

function waitFor(error, condition) {
  const deadline = Date.now() + timeoutMs;

  while (Date.now() < deadline) {
    if (condition()) return;
    sleep(pollMs / 1000);
  }

  throw new Error(`${error} después de ${timeoutMs / 1000} s.`);
}
