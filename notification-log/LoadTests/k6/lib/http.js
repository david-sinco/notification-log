import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

export const conflicts = new Counter('concurrency_conflicts');

let seeding = false;

export function duringSetup(action) {
  seeding = true;

  try {
    return action();
  } finally {
    seeding = false;
  }
}

export function kindOf(kind) {
  return seeding ? 'setup' : kind;
}

function params(session, name, kind, expected) {
  const headers = { 'Content-Type': 'application/json' };

  if (session) headers.Authorization = `Bearer ${session.accessToken}`;

  const result = { headers, tags: { name, kind: kindOf(kind) } };

  if (expected) result.responseCallback = http.expectedStatuses(...expected);

  return result;
}

function verify(response, name, expected) {
  const ok = expected
    ? expected.some((e) => (typeof e === 'number' ? response.status === e : response.status >= e.min && response.status <= e.max))
    : response.status >= 200 && response.status < 300;

  check(response, { [`${name} responde como se espera`]: () => ok });

  if (!ok) console.warn(`${name} respondió ${response.status}: ${String(response.body).slice(0, 300)}`);

  return response;
}

export function get(url, session, name, expected) {
  return verify(http.get(url, params(session, name, 'read', expected)), name, expected);
}

export function send(method, url, body, session, name, expected) {
  const payload = body === null ? null : JSON.stringify(body);
  return verify(http.request(method, url, payload, params(session, name, 'write', expected)), name, expected);
}

export function upload(url, file, session, name) {
  const p = params(session, name, 'write');
  delete p.headers['Content-Type'];
  return verify(http.post(url, { file }, p), name);
}

export function json(response) {
  return response.status >= 200 && response.status < 300 && response.body ? response.json() : null;
}
