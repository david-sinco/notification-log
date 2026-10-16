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

export const urls = {
  identity: required('IDENTITY_URL'),
  rental: required('RENTAL_URL'),
  notification: required('NOTIFICATION_URL'),
  mailbox: required('MAILBOX_URL'),
};

export const insecureTls = __ENV.INSECURE_TLS === 'true';
export const summaryPath = __ENV.SUMMARY_PATH || `/results/${profileName}-summary.json`;

export const accounts = {
  clientId: __ENV.LOADTEST_CLIENT_ID || 'loadtest',
  ownerClientId: __ENV.OWNER_CLIENT_ID || 'web',
  password: __ENV.LOADTEST_PASSWORD || 'LoadTest12345',
  domain: __ENV.LOADTEST_EMAIL_DOMAIN || 'loadtest.local',
  adminEmail: __ENV.ADMIN_EMAIL || 'admin@notificationlog.local',
  adminPassword: __ENV.ADMIN_PASSWORD || 'Admin12345',
  visitors: profile.vus.visitor + profile.vus.negotiation,
  owners: profile.owners,
  moderators: Math.min(profile.vus.moderator, 5),
};

export const marker = 'LoadTest';
export const probeEventKey = 'publicacion.precio.cambiado';
