import { sleep } from 'k6';
import { profile, urls } from '../lib/config.js';
import { email, session } from '../lib/auth.js';
import { get, send } from '../lib/http.js';
import { pick, randomInt, slots } from '../lib/data.js';

export function visitor(data) {
  const index = ((__VU - 1) % profile.vus.visitor) + 1;
  const user = session(email('visitor', index));
  const listing = pick(data.listings);

  get(`${urls.rental}/api/catalog?page=1&pageSize=20`, null, 'GET /api/catalog');
  sleep(randomInt(1, 3));

  get(`${urls.rental}/api/catalog/${listing.id}`, null, 'GET /api/catalog/{id}');
  sleep(randomInt(2, 4));

  send('POST', `${urls.rental}/api/visits`, { listingId: listing.id, slots: slots() }, user, 'POST /api/visits');
  sleep(randomInt(1, 3));

  get(`${urls.rental}/api/visits?page=1&pageSize=20`, user, 'GET /api/visits');
  sleep(randomInt(2, 5));
}
