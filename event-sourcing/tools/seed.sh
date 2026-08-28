#!/usr/bin/env bash
# tools/seed.sh <count> [usersApi] [concurrency]
#
# Drives the real Users HTTP API to populate data for the experiments in SPEC.md §11: register,
# request a phone change, fail verification ~20% of the time (so the funnel has real numbers),
# verify, enable SMS, deactivate ~10%.
set -euo pipefail

COUNT="${1:?usage: seed.sh <count> [usersApi] [concurrency]}"
USERS_API="${2:-http://localhost:5100}"
CONCURRENCY="${3:-20}"

seed_one() {
  local i="$1"
  local name="seed-user-$i"

  local id
  id=$(curl -sf -X POST "$USERS_API/api/users" \
    -H 'Content-Type: application/json' \
    -d "{\"name\":\"$name\"}" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4) || return 0
  [ -n "$id" ] || return 0

  local phone="+1555$(printf '%07d' "$i")"
  local token
  token=$(curl -sf -X POST "$USERS_API/api/users/$id/phone" \
    -H 'Content-Type: application/json' \
    -d "{\"phone\":\"$phone\"}" | grep -oE '"token":"[^"]+"' | cut -d'"' -f4) || return 0
  [ -n "$token" ] || return 0

  # ~20% of users fail verification once (wrong token) before succeeding, so the funnel has
  # real failure numbers.
  if [ $((i % 5)) -eq 0 ]; then
    curl -sf -X POST "$USERS_API/api/users/$id/phone/verify" \
      -H 'Content-Type: application/json' \
      -d '{"token":"wrong-token"}' >/dev/null || true
  fi

  curl -sf -X POST "$USERS_API/api/users/$id/phone/verify" \
    -H 'Content-Type: application/json' \
    -d "{\"token\":\"$token\"}" >/dev/null || return 0

  # Only legal once the phone is verified (SPEC.md §4, invariant 5).
  curl -sf -X PUT "$USERS_API/api/users/$id/preferences" \
    -H 'Content-Type: application/json' \
    -d '{"email":true,"sms":true,"quietHoursStart":null,"quietHoursEnd":null}' >/dev/null || true

  # ~10% deactivated.
  if [ $((i % 10)) -eq 0 ]; then
    curl -sf -X POST "$USERS_API/api/users/$id/deactivate" >/dev/null || true
  fi
}
export -f seed_one
export USERS_API

echo "Seeding $COUNT users against $USERS_API (concurrency $CONCURRENCY)..."
start=$(date +%s)

seq 1 "$COUNT" | xargs -P "$CONCURRENCY" -I{} bash -c 'seed_one "$@"' _ {}

end=$(date +%s)
echo "Done in $((end - start))s."
echo
echo "Compare:"
echo "  curl -s $USERS_API/api/users/funnel"
echo "  curl -s http://localhost:5200/api/notifications/replica/status   # should converge on $COUNT"
