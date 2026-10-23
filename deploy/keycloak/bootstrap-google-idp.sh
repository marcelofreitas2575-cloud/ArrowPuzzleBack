#!/bin/sh
set -eu

if [ -z "${GOOGLE_IDP_CLIENT_ID:-}" ] || [ -z "${GOOGLE_IDP_CLIENT_SECRET:-}" ]; then
  echo "[google-idp] GOOGLE_IDP_CLIENT_ID ou GOOGLE_IDP_CLIENT_SECRET vazio — configure deploy/docker/.env e rode: docker compose up keycloak-google-bootstrap"
  exit 0
fi

KC_URL="${KEYCLOAK_URL:-http://keycloak:8080}"
ADMIN="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PW="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
REALM="${KEYCLOAK_REALM:-arrow-puzzle}"
KCADM="/opt/keycloak/bin/kcadm.sh"

echo "[google-idp] Aguardando Keycloak em $KC_URL ..."
attempt=0
until $KCADM config credentials --server "$KC_URL" --realm master --user "$ADMIN" --password "$ADMIN_PW" >/dev/null 2>&1; do
  attempt=$((attempt + 1))
  if [ "$attempt" -ge 40 ]; then
    echo "[google-idp] Keycloak não respondeu a tempo."
    exit 1
  fi
  sleep 3
done

echo "[google-idp] Atualizando Identity Provider Google no realm $REALM ..."
if $KCADM get identity-provider/instances/google -r "$REALM" >/dev/null 2>&1; then
  $KCADM update identity-provider/instances/google -r "$REALM" \
    -s enabled=true \
    -s "config.clientId=${GOOGLE_IDP_CLIENT_ID}" \
    -s "config.clientSecret=${GOOGLE_IDP_CLIENT_SECRET}" \
    -s 'config.defaultScope=openid profile email' \
    -s 'config.syncMode=IMPORT'
else
  $KCADM create identity-provider/instances -r "$REALM" \
    -s alias=google \
    -s providerId=google \
    -s enabled=true \
    -s trustEmail=true \
    -s 'config.clientId='"${GOOGLE_IDP_CLIENT_ID}" \
    -s 'config.clientSecret='"${GOOGLE_IDP_CLIENT_SECRET}" \
    -s 'config.defaultScope=openid profile email' \
    -s 'config.syncMode=IMPORT'
fi

if [ -n "${KEYCLOAK_FRONTEND_URL:-}" ]; then
  echo "[google-idp] Frontend URL do realm: $KEYCLOAK_FRONTEND_URL"
  $KCADM update realms/"$REALM" -s "frontendUrl=${KEYCLOAK_FRONTEND_URL}" || true
fi

echo "[google-idp] Google habilitado. Redirect URI no Google Cloud:"
echo "  ${KEYCLOAK_PUBLIC_REDIRECT_URI:-<defina KEYCLOAK_PUBLIC_REDIRECT_URI no .env>}"
