import { marker } from './config.js';

const bogotaOffsetHours = 5;

export const Operation = { Sale: 0, Rent: 1 };
export const PropertyType = { Apartment: 0, Studio: 1 };
export const Channel = { Email: 1 };

export function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function pick(items) {
  return items[Math.floor(Math.random() * items.length)];
}

export function slot(dayOffset, hour) {
  const bogota = new Date(Date.now() - bogotaOffsetHours * 3600 * 1000);

  return new Date(Date.UTC(
    bogota.getUTCFullYear(),
    bogota.getUTCMonth(),
    bogota.getUTCDate() + dayOffset,
    hour + bogotaOffsetHours,
  )).toISOString();
}

export function slots() {
  const day = randomInt(2, 13);
  const hour = randomInt(8, 12);
  return [slot(day, hour), slot(day, hour + 2), slot(day, hour + 4)];
}

const neighborhoods = ['Chapinero', 'Usaquén', 'Teusaquillo', 'Cedritos', 'Salitre', 'Suba', 'Kennedy', 'Fontibón'];

export function details() {
  const neighborhood = pick(neighborhoods);

  return {
    type: pick([PropertyType.Apartment, PropertyType.Studio]),
    area: randomInt(35, 180),
    bedrooms: randomInt(1, 4),
    bathrooms: randomInt(1, 3),
    parkingSpots: randomInt(0, 2),
    stratum: randomInt(2, 6),
    floor: randomInt(1, 20),
    hasElevator: Math.random() < 0.6,
    administrationFee: randomInt(100, 600) * 1000,
    city: 'Bogotá',
    neighborhood: `${neighborhood} ${marker}`,
    address: `Calle ${randomInt(1, 200)} # ${randomInt(1, 99)}-${randomInt(1, 99)}`,
    description: `Inmueble creado por las pruebas de carga (${marker}). Apartamento iluminado en ${neighborhood}, cerca de transporte público, parques y comercio, con cocina abierta y zona de ropas.`,
  };
}

export function rentPrice() {
  return randomInt(300, 9000) * 10000;
}

const photoBytes = (() => {
  const bytes = new Uint8Array(50 * 1024);
  bytes.set([0xff, 0xd8, 0xff, 0xe0]);
  for (let i = 4; i < bytes.length; i++) bytes[i] = (i * 31) % 256;
  return bytes.buffer;
})();

export function photo() {
  return photoBytes;
}
