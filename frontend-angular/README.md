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

### Flujos implementados

- Login con email/password.
- Login con Google (`signInWithPopup`) y fallback a redirect (`signInWithRedirect`) cuando popup no es posible.
- Persistence local (`browserLocalPersistence`).
- En cada request API se envia `Authorization: Bearer <idToken>`.

### Requisito de claims (multi-tenant)

El backend exige claim `glamping_id` (GUID) en el JWT.

La app:

- decodifica el token para leer claims
- valida formato GUID de `glamping_id`
- bloquea acceso y redirige a `/no-access` si falta o es invalido

### Configuracion requerida en Firebase Console

1. `Authentication -> Sign-in method -> Google -> Enable`
2. `Authentication -> Settings -> Authorized domains`
   - `localhost`
   - tu dominio productivo

## Nota importante sobre usuarios Google y claims

Cuando un usuario se crea por Google, normalmente no trae `glamping_id` por defecto.
Debes asignarlo con Admin SDK / Cloud Function.

Despues de asignar claims:

- cerrar sesion e iniciar sesion de nuevo, o
- forzar refresh de token (la app ya intenta refresh)

## Manejo de 401 en API

El interceptor de errores:

- ante `401`: intenta 1 refresh + retry de request
- si sigue `401`: cierra sesion y redirige a `/login`

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
- `web`: build + static hosting Angular (nginx interno)
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

Durante startup del contenedor `web`, `docker-entrypoint.sh` reemplaza placeholders en `assets/config.json`.

## Asignar claims (opcional recomendado)

Hay script auxiliar en `tools/firebase-admin` para asignar custom claims a usuarios.

Ejemplo:

```bash
cd tools/firebase-admin
npm install
node set-claims.js --serviceAccount=./serviceAccountKey.json --uid=USER_UID --glamping_id=11111111-1111-1111-1111-111111111111 --role=Admin
```
