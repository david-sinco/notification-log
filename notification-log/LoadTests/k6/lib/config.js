import smoke from '../profiles/smoke.js';
import carga100 from '../profiles/carga-100.js';
import carga1000 from '../profiles/carga-1000.js';

const profiles = { smoke, 'carga-100': carga100, 'carga-1000': carga1000 };

function required(name) {
  const value = __ENV[name];
  if (!value) throw new Error(`Falta la variable de entorno ${name}.`);
  return value.replace(/\/$/, '');
}

export const profileName = __ENV.PROFILE || 'smoke';
export const profile = profiles[profileName];

if (!profile) throw new Error(`El perfil '${profileName}' no existe. Usa: ${Object.keys(profiles).join(', ')}.`);

export const target = __ENV.TARGET || 'local';
export const urls = {
  identity: required('IDENTITY_URL'),
  rental: required('RENTAL_URL'),
  notification: required('NOTIFICATION_URL'),
};

export const insecureTls = __ENV.INSECURE_TLS === 'true';
export const summaryPath = __ENV.SUMMARY_PATH || `/results/${target}-${profileName}-summary.json`;

export const accounts = {
  clientId: __ENV.LOADTEST_CLIENT_ID || 'loadtest',
  password: __ENV.LOADTEST_PASSWORD || 'LoadTest12345',
  domain: __ENV.LOADTEST_EMAIL_DOMAIN || 'loadtest.local',
  visitors: 1000,
  moderators: 5,
};

export const marker = 'LoadTest';
export const probeEventKey = 'publicacion.precio.cambiado';
