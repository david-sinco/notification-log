import http from 'k6/http';
import encoding from 'k6/encoding';
import { accounts, urls } from './config.js';
import { kindOf } from './http.js';

const scopes = 'rentals notifications roles email phone';
const renewBeforeSeconds = 60;
const cache = {};

export function email(prefix, index) {
  return `${prefix}-${String(index).padStart(4, '0')}@${accounts.domain}`;
}

export function login(user) {
  const response = http.post(
    `${urls.identity}/connect/token`,
    {
      grant_type: 'password',
      client_id: accounts.clientId,
      username: user,
      password: accounts.password,
      scope: scopes,
    },
    { tags: { name: 'POST /connect/token', kind: kindOf('token') } },
  );

  if (response.status !== 200) {
    throw new Error(`No se pudo obtener el token de ${user}: ${response.status} ${response.body}`);
  }

  const body = response.json();

  return {
    accessToken: body.access_token,
    expiresAt: Date.now() + (body.expires_in - renewBeforeSeconds) * 1000,
    userId: subject(body.access_token),
  };
}

export function session(user) {
  const cached = cache[user];

  if (cached && cached.expiresAt > Date.now()) return cached;

  cache[user] = login(user);

  return cache[user];
}

function subject(token) {
  const payload = token.split('.')[1];
  return JSON.parse(encoding.b64decode(payload, 'rawurl', 's')).sub;
}
