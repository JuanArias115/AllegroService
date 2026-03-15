# Manual de Uso

Ver tambien: [Contexto](./CONTEXT.md), [Arquitectura](./ARCHITECTURE.md), [Onboarding](./ONBOARDING.md), [bootstrap_admin.sql](../scripts/sql/bootstrap_admin.sql).

## A. Arquitectura
El proyecto corre con cuatro contenedores: `db` (Postgres), `api` (.NET), `web` (Angular compilado y servido por Nginx interno) y `nginx` (reverse proxy publico). En local y en VPS la URL principal es `http://<host>:4400/`; desde ahi el frontend consume la API por `/api`. La API tambien queda expuesta en `http://<host>:<API_PORT>/`, util para pruebas directas.

## B. Requisitos
- Docker y Docker Compose instalados
- Proyecto Firebase activo
- Firebase Authentication habilitado
- Google Sign-In habilitado en Firebase si quieres login con Google
- Dominios autorizados en Firebase:
  - `localhost`
  - IP o dominio del VPS
  - dominio final de produccion si aplica

## C. Configuracion (`.env`)
En la raiz del repo existe `.env` con las variables usadas por `docker-compose.yml`. Si necesitas crear uno nuevo, usa estas claves:

```env
POSTGRES_DB=allegro
POSTGRES_USER=allegro_user
POSTGRES_PASSWORD=tu_password_seguro
API_PORT=40123
API_BASE_URL=/api
FIREBASE_API_KEY=tu_firebase_api_key
FIREBASE_AUTH_DOMAIN=tu-proyecto.firebaseapp.com
FIREBASE_PROJECT_ID=tu-proyecto
FIREBASE_APP_ID=tu_firebase_app_id
```

Variables importantes:
- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`: base y credenciales de Postgres.
- `API_PORT`: puerto del backend expuesto directamente.
- `FIREBASE_PROJECT_ID`: se inyecta al backend como `Firebase__ProjectId`.
- `FIREBASE_API_KEY`, `FIREBASE_AUTH_DOMAIN`, `FIREBASE_APP_ID`: configuracion runtime del frontend.
- `API_BASE_URL`: normalmente `/api`.

Referencia util:
- [frontend-angular/.env.example](/Users/juanarias/Documents/Fuentes/AllegroService/frontend-angular/.env.example)

## D. Levantar el proyecto

### Local
```bash
cd /Users/juanarias/Documents/Fuentes/AllegroService
docker compose up -d --build
docker compose ps
docker compose logs -f api --tail 200
```

URLs:
- Panel: `http://localhost:4400/`
- API directa: `http://localhost:${API_PORT}/`

### VPS
```bash
cd /ruta/del/proyecto
docker compose pull
docker compose up -d --build
docker compose ps
docker compose logs -f nginx --tail 100
docker compose logs -f api --tail 200
```

URLs:
- Panel: `http://<IP_O_DOMINIO>:4400/`
- API directa: `http://<IP_O_DOMINIO>:<API_PORT>/`

Swagger:
- En este repo Swagger se publica solo si la API corre en `Development`.
- Si no lo ves en Compose, revisa o define `ASPNETCORE_ENVIRONMENT=Development` temporalmente.
- Ruta esperada si esta activo: `/swagger` en la API directa o `/api/swagger` detras de Nginx.

## E. Bootstrap del primer Admin
La autorizacion no depende de `glamping_id` en el JWT. El backend toma `sub` del token Firebase y busca ese UID en `UserTenants`.

SQL directo en Postgres:

```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;

WITH params AS (
  SELECT
    'REEMPLAZAR_UID_FIREBASE'::text AS firebase_uid,
    'admin@tu-dominio.com'::text AS email,
    '00000000-0000-0000-0000-000000000000'::uuid AS glamping_id
)
INSERT INTO "UserTenants"
(
  "Id",
  "FirebaseUid",
  "Email",
  "GlampingId",
  "Role",
  "Status",
  "CreatedAt",
  "UpdatedAt"
)
SELECT
  gen_random_uuid(),
  p.firebase_uid,
  p.email,
  p.glamping_id,
  1,
  2,
  now(),
  now()
FROM params p
ON CONFLICT ("FirebaseUid") DO UPDATE
SET
  "Email" = EXCLUDED."Email",
  "GlampingId" = EXCLUDED."GlampingId",
  "Role" = 1,
  "Status" = 2,
  "UpdatedAt" = now();
```

Verificacion:

```sql
SELECT
  "Id",
  "FirebaseUid",
  "Email",
  "GlampingId",
  "Role",
  "Status",
  "UpdatedAt"
FROM "UserTenants"
WHERE "FirebaseUid" = 'REEMPLAZAR_UID_FIREBASE';
```

Script equivalente ya incluido:
- [scripts/sql/bootstrap_admin.sql](/Users/juanarias/Documents/Fuentes/AllegroService/scripts/sql/bootstrap_admin.sql)

## F. Flujo de login y acceso
1. El usuario entra al panel en `http://localhost:4400/` o `http://<host>:4400/`.
2. Hace login con Google o email/password usando Firebase.
3. El frontend envia `Authorization: Bearer <idToken>` a la API.
4. La API valida issuer/audience y toma `sub`.
5. La API consulta `UserTenants`.
6. Si `Status = 2 (Active)`, el usuario entra.
7. Si responde `403 USER_NOT_ONBOARDED` o `403 USER_DISABLED`, el frontend muestra `No Access`.

Activar usuario pendiente:

```sql
UPDATE "UserTenants"
SET "Status" = 2, "UpdatedAt" = now()
WHERE "FirebaseUid" = 'UID_DEL_USUARIO';
```

Endpoint de sesion actual:
- `GET /api/v1/user-tenants/me`

## G. Operacion del panel
- `Units`: cabanas/unidades y su estado operativo.
- `Guests`: huespedes y datos de contacto.
- `Categories`: categorias de catalogo.
- `Products`: productos vendibles y configuracion de stock.
- `Locations`: ubicaciones de inventario.
- `Reservations`: reservas, check-in y acceso a estadias/folios.

Flujo de negocio habitual:
1. Crear `Guest` si no existe.
2. Crear `Reservation`.
3. Hacer `Check-in` desde la reserva.
4. Desde la reserva en estado `CheckedIn`, registrar consumos en la seccion `Consumos` o abrir detalle de `Stay`.
5. En `Stay`, revisar `Folio`, agregar consumos y registrar pagos.
6. Agregar pagos.
7. Ejecutar `Check-out`.
8. Si el huesped tiene telefono, puedes generar y abrir el resumen por WhatsApp.

UX disponible en el panel:
- selector `ES / EN` en topbar, persistido en `localStorage`
- toggle de modo oscuro en topbar
- tooltips de ayuda en stock, pagos, roles y cierre
- iconos ligeros en menu y acciones principales

## H. Crear usuarios nuevos
Despues del primer admin, el flujo recomendado es por API. Si el panel aun no tiene pantalla de usuarios, usa `curl`.

Ejemplo:

```bash
curl -X POST "http://localhost:4400/api/v1/user-tenants" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -H "Content-Type: application/json" \
  -d '{
    "firebaseUid": "UID_FIREBASE_USUARIO",
    "email": "recepcion@empresa.com",
    "role": 2,
    "status": 2
  }'
```

Roles:
- `1=Admin`
- `2=Reception`
- `3=Restaurant`
- `4=Inventory`

Status:
- `1=Pending`
- `2=Active`
- `3=Disabled`

Como obtener el UID del usuario:
- Opcion mas simple: Firebase Console -> Authentication -> Users.
- Alternativa: el usuario hace login una vez y luego revisas el UID desde Firebase/Logs.
- Si tienes duda sobre el endpoint disponible, ver Swagger.

Consumo desde reserva o estadia:
- Reserva: usa la seccion `Consumos` cuando la reserva ya este en `CheckedIn`.
- Estadia: el detalle de `Stay` muestra consumos, pagos, saldo y check-out.
- Si `TrackStock = true`, debes indicar `Location`; el backend valida stock y descuenta inventario.
- Si `TrackStock = false`, el consumo se registra sin mover inventario.

## I. Troubleshooting

### 401 `Firebase UID claim (sub) is required.` o `Invalid/unauthenticated token`
Revisar:
- `FIREBASE_PROJECT_ID` en `.env`
- que Compose lo este pasando como `Firebase__ProjectId`
- que el token sea un Firebase ID token real, no un access token cualquiera
- issuer/audience correctos
- logs:

```bash
docker compose logs -f api --tail 200
```

### 403 `USER_NOT_ONBOARDED`
Significa token valido, pero no hay fila en `UserTenants` para ese `FirebaseUid`, o el usuario no esta activo.

Verifica:

```sql
SELECT "FirebaseUid", "Email", "GlampingId", "Role", "Status"
FROM "UserTenants"
WHERE "FirebaseUid" = 'UID_DEL_USUARIO';
```

### 403 `USER_DISABLED`
El usuario existe pero esta bloqueado.

Solucion:

```sql
UPDATE "UserTenants"
SET "Status" = 2, "UpdatedAt" = now()
WHERE "FirebaseUid" = 'UID_DEL_USUARIO';
```

### 409 en check-out o reglas de negocio
Caso tipico: el folio aun tiene saldo pendiente. Debes registrar el pago faltante o usar la opcion de cierre forzado si el flujo lo permite.

### WhatsApp no abre o el boton queda deshabilitado
Revisar:
- que el telefono tenga prefijo de pais (`+57`, `+34`, etc.)
- que el resumen de checkout se haya generado
- que el navegador permita abrir una pestana nueva

### Consumo rechazada por stock
Revisar:
- `Product.TrackStock = true`
- `Location` correcta
- existencia de `StockBalance` para ese producto y ubicacion
- stock suficiente antes de registrar el consumo

### Google Sign-In: `domain not authorized`
En Firebase Console, agrega el dominio/IP en:
- Authentication
- Settings / Authorized domains

## Checklist de deploy
- `.env` correcto en el servidor
- `FIREBASE_PROJECT_ID` coincide con el proyecto Firebase real
- dominio/IP autorizado en Firebase
- `docker compose up -d --build` ejecutado sin errores
- primer admin creado en `UserTenants`
- login Firebase validado
- `GET /api/v1/user-tenants/me` responde `200` para un usuario activo
- panel carga en `http://<host>:4400/`
- selector de idioma y dark mode funcionando
- flujo check-in -> consumo -> pago -> check-out validado
