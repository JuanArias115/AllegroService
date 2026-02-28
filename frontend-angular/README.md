# Frontend Angular - Glamping Admin

Frontend Angular (standalone components) para operar el sistema multi-tenant de glampings.

## Stack

- Angular 17+
- Firebase Web SDK v9 (modular)
- Reactive Forms + validators
- HttpClient + interceptors
- TailwindCSS

## Runtime config (obligatorio)

La app lee configuracion en runtime desde `src/assets/config.json`.

Campos esperados:

```json
{
  "apiBaseUrl": "/api",
  "firebase": {
    "apiKey": "...",
    "authDomain": "...",
    "projectId": "...",
    "appId": "..."
  }
}
```

Puedes usar `src/assets/config.example.json` como referencia.

## Firebase auth

- Login con email/password contra Firebase.
- En cada request API se envia `Authorization: Bearer <idToken>`.
- Si el JWT no trae `glamping_id` valido (GUID), la app bloquea acceso y muestra pantalla `No Access`.

## Ejecutar local (sin Docker)

```bash
npm install
npm start
```

App en `http://localhost:4200`.

## Ejecutar con Docker Compose (recomendado)

Desde la raiz del repo:

```bash
docker compose up --build
```

Servicios:

- `api`: backend .NET
- `web`: build + static hosting de Angular
- `nginx`: reverse proxy unico dominio

URL local unificada:

- `http://localhost:8080`

Proxy:

- `/` -> `web`
- `/api` -> `api`

## Variables en Docker Compose

El servicio `web` consume:

- `FIREBASE_API_KEY`
- `FIREBASE_AUTH_DOMAIN`
- `FIREBASE_PROJECT_ID`
- `FIREBASE_APP_ID`
- `API_BASE_URL` (default recomendado: `/api`)

Durante startup del contenedor web, `docker-entrypoint.sh` reemplaza placeholders en `assets/config.json`.
