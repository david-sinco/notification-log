import http from 'k6/http';
import { sleep } from 'k6';
import { profile, urls } from '../lib/config.js';
import { email, session } from '../lib/auth.js';
import { conflicts, send, json } from '../lib/http.js';
import { pick, randomInt, slots } from '../lib/data.js';

export function negotiation(data) {
  const index = profile.vus.visitor + ((__VU - 1) % profile.vus.negotiation) + 1;
  const visitor = session(email('visitor', index));
  const listing = pick(data.listings);
  const host = session(email('owner', listing.owner));
  const base = `${urls.rental}/api/visits`;

  const created = json(send('POST', base, { listingId: listing.id, slots: slots() }, visitor, 'POST /api/visits'));

  if (!created) {
    sleep(randomInt(1, 2));
    return;
  }

  sleep(randomInt(1, 2));

  const proposal = slots();
  send('POST', `${base}/${created.id}/counter-proposal`, { slots: proposal }, host, 'POST /api/visits/{id}/counter-proposal');
  sleep(randomInt(1, 2));

  send('POST', `${base}/${created.id}/schedule`, { startsAt: pick(proposal) }, visitor, 'POST /api/visits/{id}/schedule');
  sleep(randomInt(1, 2));

  cancelFromBothSides(`${base}/${created.id}/cancel`, visitor, host);
  sleep(randomInt(2, 4));
}

function cancelFromBothSides(url, visitor, host) {
  const request = (user, reason) => ({
    method: 'POST',
    url,
    body: JSON.stringify({ reason }),
    params: {
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${user.accessToken}` },
      tags: { name: 'POST /api/visits/{id}/cancel (simultáneo)', kind: 'write' },
      responseCallback: http.expectedStatuses(204, 500),
    },
  });

  const responses = http.batch([
    request(visitor, 'Cancelada por el visitante durante la prueba de carga.'),
    request(host, 'Cancelada por el anfitrión durante la prueba de carga.'),
  ]);

  responses.filter((response) => response.status === 500).forEach(() => conflicts.add(1));
}
