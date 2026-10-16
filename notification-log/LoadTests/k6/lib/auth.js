import http from 'k6/http';
import encoding from 'k6/encoding';
import { accounts, urls } from './config.js';
import { kindOf } from './http.js';

const scopes = 'identity rentals notifications roles email phone';
const renewBeforeSeconds = 60;
const cache = {};

export function email(prefix, index) {
  return `${prefix}-${String(index).padStart(4, '0')}@${accounts.domain}`;
}

export function login(user, password = accounts.password) {
  const response = requestToken(user, password);

  if (response.status !== 200) {
    throw new Error(`No se pudo obtener el token de ${user}: ${response.status} ${response.body}`);
  }

  return toSession(response);
}

export function tryLogin(user, password = accounts.password) {
  const response = requestToken(user, password, http.expectedStatuses(200, 400));
  return response.status === 200 ? toSession(response) : null;
}

export function adminSession() {
  return session(accounts.adminEmail, accounts.adminPassword);
}

export function session(user, password = accounts.password) {
  const cached = cache[user];

  if (cached && cached.expiresAt > Date.now()) return cached;

  cache[user] = login(user, password);

  return cache[user];
}

function requestToken(user, password, responseCallback) {
  const params = { tags: { name: 'POST /connect/token', kind: kindOf('token') } };

  if (responseCallback) params.responseCallback = responseCallback;

  return http.post(
    `${urls.identity}/connect/token`,
    {
      grant_type: 'password',
      client_id: accounts.clientId,
      username: user,
      password,
      scope: scopes,
    },
    params,
  );
}

function toSession(response) {
  const body = response.json();

  return {
    accessToken: body.access_token,
    expiresAt: Date.now() + (body.expires_in - renewBeforeSeconds) * 1000,
    userId: subject(body.access_token),
  };
}

function subject(token) {
  const payload = token.split('.')[1];
  return JSON.parse(encoding.b64decode(payload, 'rawurl', 's')).sub;
}
