import { sleep } from 'k6';
import { urls } from '../lib/config.js';
import { email, session } from '../lib/auth.js';
import { get } from '../lib/http.js';
import { randomInt } from '../lib/data.js';
import { draftListing, prepareForReview } from '../lib/rentals.js';

export function owner(data) {
  const owner = data.owners[(__VU - 1) % data.owners.length];
  const user = session(email('owner', owner.index));

  get(`${urls.rental}/api/dashboard`, user, 'GET /api/dashboard');
  sleep(randomInt(1, 3));

  get(`${urls.rental}/api/owners/mine`, user, 'GET /api/owners/mine');
  get(`${urls.rental}/api/listings?participantId=${user.userId}&page=1&pageSize=20`, user, 'GET /api/listings');
  sleep(randomInt(1, 3));

  const listingId = draftListing(user, owner.ownerId);

  if (listingId) prepareForReview(user, listingId);

  sleep(randomInt(3, 6));
}
