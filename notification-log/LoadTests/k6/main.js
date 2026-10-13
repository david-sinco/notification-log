import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { insecureTls, profile, profileName, summaryPath, target } from './lib/config.js';
import { seed } from './lib/setup.js';

export { catalog } from './scenarios/catalog.js';
export { visitor } from './scenarios/visitor.js';
export { owner } from './scenarios/owner.js';
export { negotiation } from './scenarios/negotiation.js';
export { moderator } from './scenarios/moderator.js';
export { notifications } from './scenarios/notifications.js';
export { token } from './scenarios/token.js';

function seconds(duration) {
  const [, value, unit] = duration.match(/^(\d+)(s|m)$/);
  return Number(value) * (unit === 'm' ? 60 : 1);
}

const measured = `${seconds(profile.ramp) + seconds(profile.hold)}s`;

function users(exec) {
  const vus = profile.vus[exec];

  return {
    executor: 'ramping-vus',
    exec,
    startVUs: 0,
    stages: [
      { duration: profile.ramp, target: vus },
      { duration: profile.hold, target: vus },
      { duration: profile.rampDown, target: 0 },
    ],
    gracefulRampDown: '30s',
  };
}

export const options = {
  insecureSkipTLSVerify: insecureTls,
  setupTimeout: '20m',
  summaryTrendStats: ['avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  scenarios: {
    catalog: users('catalog'),
    visitor: users('visitor'),
    owner: users('owner'),
    negotiation: users('negotiation'),
    moderator: users('moderator'),
    token: {
      executor: 'constant-arrival-rate',
      exec: 'token',
      rate: profile.tokenRate,
      timeUnit: '1s',
      duration: measured,
      preAllocatedVUs: Math.max(2, profile.tokenRate * 2),
      maxVUs: Math.max(10, profile.tokenRate * 10),
    },
    notifications: {
      executor: 'constant-arrival-rate',
      exec: 'notifications',
      rate: 1,
      timeUnit: profile.probeEvery,
      duration: measured,
      preAllocatedVUs: 2,
      maxVUs: 10,
    },
  },
  thresholds: {
    'http_req_duration{kind:read}': ['p(95)<500'],
    'http_req_duration{kind:write}': ['p(95)<1000'],
    'http_req_duration{kind:token}': ['p(95)<1000'],
    'http_req_failed{kind:read}': ['rate<0.01'],
    'http_req_failed{kind:write}': ['rate<0.01'],
    'http_req_failed{kind:token}': ['rate<0.01'],
    notification_lag: ['p(95)<5000'],
    notification_timeouts: ['count==0'],
  },
};

export function setup() {
  console.log(`Perfil '${profileName}' contra '${target}'. Preparando datos...`);
  return seed();
}

export function handleSummary(data) {
  return {
    stdout: textSummary(data, { indent: ' ', enableColors: false }),
    [summaryPath]: JSON.stringify(data, null, 2),
  };
}
