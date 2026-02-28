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

### Autorizacion y onboarding (nuevo flujo)

El frontend ya **no** valida `glamping_id` en el JWT.

Despues del login Firebase, la app consulta el backend para resolver tenant/rol/status del usuario.  
Hace fallback automatico en este orden y cachea el endpoint valido:

- `GET /api/v1/user-tenants/me` (preferido)
- `GET /api/v1/me`
- `GET /api/v1/user-tenants/current`

Con esa respuesta (directa o envuelta en `{ data }`) guarda sesion local con:

- `firebaseUid`
- `email`
- `glampingId` (desde backend)
- `role` (1=Admin,2=Reception,3=Restaurant,4=Inventory)
- `status` (1=Pending,2=Active,3=Disabled)

Solo `status = Active` permite entrar a la app.

### Configuracion requerida en Firebase Console

1. `Authentication -> Sign-in method -> Google -> Enable`
2. `Authentication -> Settings -> Authorized domains`
   - `localhost`
   - tu dominio productivo

## No access / estados

Si el backend responde `403`:

- `USER_NOT_ONBOARDED` -> pantalla `/no-access` con mensaje de habilitacion pendiente
- `USER_DISABLED` -> pantalla `/no-access` con mensaje de cuenta deshabilitada
- `403` generico -> pantalla `/no-access` con mensaje de sin acceso

Si responde `401`:

- intenta 1 refresh de token + retry
- si falla: logout y redireccion a `/login`

## Primer admin y alta de usuarios

El onboarding ahora es por PostgreSQL tabla `UserTenants` (backend), no por custom claims.

Si aun no tienes un admin activo, crea uno directo en BD:

```sql
INSERT INTO \"UserTenants\"
(\"Id\",\"FirebaseUid\",\"Email\",\"GlampingId\",\"Role\",\"Status\",\"CreatedAt\",\"UpdatedAt\")
VALUES
(gen_random_uuid(),'UID_FIREBASE_ADMIN','admin@tu-dominio.com','<GLAMPING_GUID>',1,2,now(),now());
```

Luego, si el endpoint existe y el usuario tiene rol Admin, puedes gestionar usuarios por API:

- `GET/POST/PUT/DELETE /api/v1/user-tenants`

Ejemplo create:

```json
{
  "firebaseUid": "UID_FIREBASE_USUARIO",
  "email": "recepcion@tu-dominio.com",
  "role": 2,
  "status": 2
}
```

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

## Menu por rol

El sidebar usa el rol resuelto por backend:

- `Admin`: ve todos los modulos
- `Inventory`: ve modulos de inventario (categories/products/locations)
- otros roles: no ven modulos de inventario
