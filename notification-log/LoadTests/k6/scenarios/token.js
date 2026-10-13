import { accounts } from '../lib/config.js';
import { email, login } from '../lib/auth.js';
import { randomInt } from '../lib/data.js';

export function token() {
  login(email('visitor', randomInt(1, accounts.visitors)));
}
