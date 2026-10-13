import { sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { probeEventKey, urls } from '../lib/config.js';
import { email, session } from '../lib/auth.js';
import { get, send, json } from '../lib/http.js';
import { rentPrice } from '../lib/data.js';

const lag = new Trend('notification_lag', true);
const timeouts = new Counter('notification_timeouts');
const timeoutMs = 60000;
const pollMs = 500;

export function notifications(data) {
  if (!data.probe) return;

  const admin = session(email('admin', 1));
  const owner = session(email('owner', data.probe.ownerIndex));
  const query = `${urls.notification}/api/notifications?recipientId=${data.probe.recipientId}&eventKey=${probeEventKey}&page=1&pageSize=1`;

  const before = latestId(query, admin);
  const started = Date.now();

  send('PATCH', `${urls.rental}/api/listings/${data.probe.listingId}/price`, { price: rentPrice() }, owner,
    'PATCH /api/listings/{id}/price (sonda)');

  while (Date.now() - started < timeoutMs) {
    if (latestId(query, admin) !== before) {
      lag.add(Date.now() - started);
      return;
    }

    sleep(pollMs / 1000);
  }

  timeouts.add(1);
}

function latestId(url, admin) {
  const page = json(get(url, admin, 'GET /api/notifications'));
  return page && page.items.length > 0 ? page.items[0].id : null;
}
