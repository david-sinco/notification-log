import { sleep } from 'k6';
import { urls } from '../lib/config.js';
import { get, json } from '../lib/http.js';
import { pick, randomInt } from '../lib/data.js';

const searches = ['', '', 'Bogotá', 'Chapinero', 'Usaquén', 'LoadTest', 'Calle'];

export function catalog() {
  const search = pick(searches);
  const page = randomInt(1, 5);
  const operation = Math.random() < 0.5 ? '&operation=Rent' : '';
  const url = `${urls.rental}/api/catalog?search=${encodeURIComponent(search)}&page=${page}&pageSize=20${operation}`;
  const result = json(get(url, null, 'GET /api/catalog'));

  sleep(randomInt(1, 3));

  const items = (result && result.items) || [];

  for (let i = 0; i < Math.min(items.length, randomInt(1, 3)); i++) {
    get(`${urls.rental}/api/catalog/${pick(items).id}`, null, 'GET /api/catalog/{id}');
    sleep(randomInt(2, 5));
  }
}
