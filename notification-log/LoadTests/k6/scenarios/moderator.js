import { sleep } from 'k6';
import { accounts, urls } from '../lib/config.js';
import { email, session } from '../lib/auth.js';
import { get, json } from '../lib/http.js';
import { randomInt } from '../lib/data.js';
import { approve } from '../lib/rentals.js';

export function moderator() {
  const user = session(email('moderator', ((__VU - 1) % accounts.moderators) + 1));

  const pending = json(get(`${urls.rental}/api/listings?status=InReview&page=1&pageSize=20`, user, 'GET /api/listings?status=InReview'));
  sleep(randomInt(1, 3));

  for (const listing of ((pending && pending.items) || []).slice(0, 3)) {
    approve(user, listing.id, [204, 422, 500]);
    sleep(randomInt(1, 2));
  }

  get(`${urls.rental}/api/owners?page=1&pageSize=20`, user, 'GET /api/owners');
  get(`${urls.rental}/api/visitors/summary`, user, 'GET /api/visitors/summary');
  get(`${urls.rental}/api/visitors?sort=MostVisits&page=1&pageSize=20`, user, 'GET /api/visitors');
  sleep(randomInt(3, 6));
}
