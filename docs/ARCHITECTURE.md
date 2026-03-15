# Arquitectura

Ver tambien: [Contexto](./CONTEXT.md), [Onboarding](./ONBOARDING.md).

## Vista general
Monorepo con backend API, frontend Angular y despliegue via Docker Compose.

- `AllegroService/AllegroService`: API .NET
- `frontend-angular`: Web app
- `docker-compose.yml`: `db` + `api` + `web` + `nginx`

Nginx publica un dominio local unico:
- `/` -> frontend
- `/api` -> backend

## Backend por capas
- `Domain`: entidades y enums
- `Infrastructure/Persistence`: `AppDbContext`, configuraciones EF, migraciones, seed
- `Application`: DTOs, interfaces, servicios, validadores
- `Api`: controllers, middlewares, auth/contexto

Patron de ejecucion:
`Controller -> Service -> DbContext`

## Seguridad
Autenticacion:
- JWT Bearer de Firebase
- Validaciones clave:
  - `issuer = https://securetoken.google.com/{projectId}`
  - `audience = {projectId}`

Configuracion:
- `Firebase:ProjectId` (appsettings)
- `Firebase__ProjectId` (env var, Docker)

Autorizacion y tenancy (Opcion 3):
- No se usa `glamping_id` en claims
- Se toma `sub` (Firebase UID) del token
- Middleware busca `UserTenants` por `FirebaseUid`
- Si existe y `Status=Active`, inyecta contexto actual:
  - `GlampingId`
  - `Role`
- Si no existe: `403 USER_NOT_ONBOARDED`
- Si disabled: `403 USER_DISABLED`

## Multi-tenant
Todas las operaciones de negocio filtran por `GlampingId` resuelto desde `UserTenants`.

Tabla clave:
`UserTenants(FirebaseUid UNIQUE, Email, GlampingId, Role, Status, ...)`

Enums numericos:
- Role:
  - `1=Admin`
  - `2=Reception`
  - `3=Restaurant`
  - `4=Inventory`
- Status:
  - `1=Pending`
  - `2=Active`
  - `3=Disabled`

## Policies y permisos
Policies declaradas: `Admin`, `Reception`, `Restaurant`, `Inventory`.

Estado actual de uso:
- Gestion de user-tenants (`/api/v1/user-tenants` CRUD): policy `Admin`
- `/api/v1/user-tenants/me`: autenticado
- Resto de endpoints: autenticado + tenant resuelto

## Transacciones
Se usan transacciones en flujos criticos:
- Check-in
- Consumo/cargos
- Check-out

Objetivo: mantener consistencia entre estadia, folio, pagos/cargos y stock.

## Frontend (Angular)
- Login Firebase (Google + email/password)
- Interceptor agrega `Authorization: Bearer <idToken>`
- En 401: refresh token + retry 1 vez
- Onboarding check por API al iniciar/login:
  - intenta `/api/v1/user-tenants/me`
  - fallback `/api/v1/me`
  - fallback `/api/v1/user-tenants/current`
- Si no habilitado, muestra `/no-access`

