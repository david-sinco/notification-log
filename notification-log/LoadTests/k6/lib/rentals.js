import http from 'k6/http';
import { urls } from './config.js';
import { get, send, upload, json } from './http.js';
import { Operation, details, photo, rentPrice } from './data.js';

const minPhotos = 5;

export function ensureOwner(session, index, email) {
  const mine = json(get(`${urls.rental}/api/owners/mine`, session, 'GET /api/owners/mine')) || [];
  const related = mine.find((owner) => owner.relatedUserId === session.userId);

  if (related) return related.id;

  const body = {
    name: `Propietario de carga ${String(index).padStart(4, '0')}`,
    email,
    phone: `300${String(index).padStart(7, '0')}`,
  };

  return json(send('POST', `${urls.rental}/api/owners`, body, session, 'POST /api/owners')).id;
}

export function draftListing(session, ownerId) {
  const created = json(send('POST', `${urls.rental}/api/listings`, { ownerId, operation: Operation.Rent }, session, 'POST /api/listings'));
  return created && created.id;
}

export function prepareForReview(session, listingId) {
  const base = `${urls.rental}/api/listings/${listingId}`;

  send('PUT', `${base}/details`, details(), session, 'PUT /api/listings/{id}/details');
  send('PATCH', `${base}/price`, { price: rentPrice() }, session, 'PATCH /api/listings/{id}/price');

  for (let i = 0; i < minPhotos; i++) {
    upload(`${base}/photos`, http.file(photo(), `foto-${i + 1}.jpg`, 'image/jpeg'), session, 'POST /api/listings/{id}/photos');
  }

  return send('POST', `${base}/submit`, null, session, 'POST /api/listings/{id}/submit').status === 204;
}

export function approve(session, listingId, expected) {
  return send('POST', `${urls.rental}/api/moderation/listings/${listingId}/approve`, null, session,
    'POST /api/moderation/listings/{id}/approve', expected);
}
