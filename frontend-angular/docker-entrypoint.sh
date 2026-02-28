#!/bin/sh
set -eu

CONFIG_FILE="/usr/share/nginx/html/assets/config.json"

if [ ! -f "$CONFIG_FILE" ]; then
  exit 0
fi

escape_sed() {
  printf '%s' "$1" | sed -e 's/[\/&]/\\&/g'
}

API_BASE_URL_VALUE="${API_BASE_URL:-/api}"
FIREBASE_API_KEY_VALUE="${FIREBASE_API_KEY:-}"
FIREBASE_AUTH_DOMAIN_VALUE="${FIREBASE_AUTH_DOMAIN:-}"
FIREBASE_PROJECT_ID_VALUE="${FIREBASE_PROJECT_ID:-}"
FIREBASE_APP_ID_VALUE="${FIREBASE_APP_ID:-}"

sed -i "s|\${API_BASE_URL}|$(escape_sed "$API_BASE_URL_VALUE")|g" "$CONFIG_FILE"
sed -i "s|\${FIREBASE_API_KEY}|$(escape_sed "$FIREBASE_API_KEY_VALUE")|g" "$CONFIG_FILE"
sed -i "s|\${FIREBASE_AUTH_DOMAIN}|$(escape_sed "$FIREBASE_AUTH_DOMAIN_VALUE")|g" "$CONFIG_FILE"
sed -i "s|\${FIREBASE_PROJECT_ID}|$(escape_sed "$FIREBASE_PROJECT_ID_VALUE")|g" "$CONFIG_FILE"
sed -i "s|\${FIREBASE_APP_ID}|$(escape_sed "$FIREBASE_APP_ID_VALUE")|g" "$CONFIG_FILE"
