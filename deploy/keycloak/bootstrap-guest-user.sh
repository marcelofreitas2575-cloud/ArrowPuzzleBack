#!/bin/sh
set -eu

KC_URL="${KEYCLOAK_URL:-http://keycloak:8080}"
ADMIN="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PW="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
REALM="${KEYCLOAK_REALM:-arrow-puzzle}"
USERNAME="${GUEST_USERNAME:-guest-player}"
PASSWORD="${GUEST_PASSWORD:-guest-player}"
KCADM="/opt/keycloak/bin/kcadm.sh"

echo "[guest-user] Aguardando Keycloak..."
attempt=0
until $KCADM config credentials --server "$KC_URL" --realm master --user "$ADMIN" --password "$ADMIN_PW" >/dev/null 2>&1; do
  attempt=$((attempt + 1))
  if [ "$attempt" -ge 40 ]; then
    echo "[guest-user] Keycloak não respondeu a tempo."
    exit 1
  fi
  sleep 3
done

if $KCADM get users -r "$REALM" -q "username=$USERNAME" 2>/dev/null | grep -q '"username"'; then
  echo "[guest-user] Usuário $USERNAME já existe."
  exit 0
fi

echo "[guest-user] Criando usuário $USERNAME no realm $REALM..."
$KCADM create users -r "$REALM" \
  -s username="$USERNAME" \
  -s enabled=true \
  -s emailVerified=true \
  -s firstName=Guest \
  -s lastName=Player \
  -s email="${USERNAME}@local.arrowpuzzle"

USER_ID=$($KCADM get users -r "$REALM" -q "username=$USERNAME" --fields id --format csv --noquotes | tail -n 1)
$KCADM set-password -r "$REALM" --username "$USERNAME" --new-password "$PASSWORD" --temporary=false
$KCADM add-roles -r "$REALM" --uid "$USER_ID" --rolename player

echo "[guest-user] Usuário $USERNAME criado."
