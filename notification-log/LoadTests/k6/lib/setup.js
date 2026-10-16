import { marker, probeEventKey, profile, urls } from './config.js';
import { adminSession, email, login } from './auth.js';
import { duringSetup, get, send, json } from './http.js';
import { Channel } from './data.js';
import { ensureAccounts } from './accounts.js';
import { approve, draftListing, ensureOwner, prepareForReview } from './rentals.js';

export function seed() {
  return duringSetup(seedData);
}

function seedData() {
  ensureAccounts();

  const moderator = login(email('moderator', 1));
  const owners = [];
  const listings = [];

  for (let index = 1; index <= profile.owners; index++) {
    const ownerEmail = email('owner', index);
    const session = login(ownerEmail);
    const ownerId = ensureOwner(session, index, ownerEmail);

    owners.push({ index, ownerId, userId: session.userId });

    if (index === profile.owners) continue;

    for (const id of publishedListings(session, ownerId, moderator)) listings.push({ id, owner: index });
  }

  const probeOwner = owners[owners.length - 1];

  return {
    owners: owners.slice(0, -1),
    listings,
    probe: prepareProbe(probeOwner),
  };
}

function publishedListings(session, ownerId, moderator) {
  const query = `status=Published&participantId=${session.userId}&search=${marker}&pageSize=100`;
  const existing = json(get(`${urls.rental}/api/listings?${query}`, session, 'GET /api/listings')) || { items: [] };
  const ids = existing.items.filter((listing) => listing.ownerId === ownerId).map((listing) => listing.id);

  while (ids.length < profile.listingsPerOwner) {
    const id = draftListing(session, ownerId);

    if (!id || !prepareForReview(session, id) || approve(moderator, id).status !== 204) {
      throw new Error('No se pudo preparar una publicación para el catálogo.');
    }

    ids.push(id);
  }

  return ids.slice(0, profile.listingsPerOwner);
}

function prepareProbe(owner) {
  const admin = adminSession();

  if (!ensureProbeTrigger(admin)) return null;

  const session = login(email('owner', owner.index));
  const listingId = draftListing(session, owner.ownerId);

  return listingId ? { ownerIndex: owner.index, recipientId: owner.userId, listingId } : null;
}

function findTrigger(admin, eventKey) {
  for (let page = 1; ; page++) {
    const result = json(get(`${urls.notification}/api/triggers?page=${page}&pageSize=100`, admin, 'GET /api/triggers'));

    if (!result) return null;

    const found = result.items.find((trigger) => trigger.eventKey === eventKey);

    if (found || page * result.pageSize >= result.totalCount) return found || null;
  }
}

function ensureProbeTrigger(admin) {
  const existing = findTrigger(admin, probeEventKey);

  if (existing) {
    if (existing.isEnabled && existing.activeConfigurations > 0) return true;

    console.warn(`El trigger '${probeEventKey}' existe pero está deshabilitado o sin configuraciones: no se mide el pipeline de notificaciones.`);
    return false;
  }

  const template = json(send('POST', `${urls.notification}/api/templates`, {
    name: 'pruebas_carga_precio_cambiado',
    channel: Channel.Email,
    subject: 'Cambió el precio de tu publicación',
    body: '<p>Hola {{ recipient.name }}, el precio de tu publicación cambió.</p>',
  }, admin, 'POST /api/templates'));

  const trigger = json(send('POST', `${urls.notification}/api/triggers`, {
    eventKey: probeEventKey,
    description: 'Creado por las pruebas de carga para medir el pipeline de notificaciones.',
  }, admin, 'POST /api/triggers'));

  if (!template || !trigger) return false;

  return send('POST', `${urls.notification}/api/triggers/${trigger.id}/configurations`, {
    templateId: template.id,
    channel: Channel.Email,
  }, admin, 'POST /api/triggers/{id}/configurations').status === 201;
}
