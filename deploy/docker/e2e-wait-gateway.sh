#!/usr/bin/env bash
set -euo pipefail

GATEWAY_URL="${GATEWAY_URL:-http://127.0.0.1:5100}"
MAX_ATTEMPTS="${MAX_ATTEMPTS:-72}"

for ((i = 1; i <= MAX_ATTEMPTS; i++)); do
  if curl -fsS "${GATEWAY_URL}/health" >/dev/null 2>&1; then
    if curl -fsS -X POST "${GATEWAY_URL}/api/v1/auth/dev-token" \
      -H "Content-Type: application/json" \
      -d '{"username":"guest-player","password":"guest-player"}' >/dev/null 2>&1; then
      echo "Gateway and dev-token ready at ${GATEWAY_URL}"
      exit 0
    fi
  fi
  echo "Waiting for gateway (${i}/${MAX_ATTEMPTS})..."
  sleep 5
done

echo "Gateway did not become ready in time." >&2
exit 1
