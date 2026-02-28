# AllegroService API

Sistema multi-tenant para administracion de glampings con EF Core Code First + API REST.

## Stack

- .NET 10 (minimal hosting style, compatible con .NET 8 pattern)
- EF Core 10 + Npgsql
- JWT Bearer (Firebase token validation)
- FluentValidation
- Swagger (Bearer auth)

## Arquitectura

- `Domain/*`: entidades y enums del dominio
- `Infrastructure/Persistence/*`: `AppDbContext`, Fluent configs, migrations, seed
- `Application/*`
  - `DTOs`: contratos request/response
  - `Interfaces`: contratos de servicios
  - `Services`: logica de negocio + tenant filtering
  - `Validators`: reglas FluentValidation
- `Api/*`
  - `Controllers`: endpoints `/api/v1/*`
  - `Auth`: claim helpers + contexto de tenant/usuario actual
  - `Middlewares`: errores globales + resolucion de tenant por Firebase UID (`sub`)
  - `Common`: respuesta estandar `{ data, errors }`

## Multi-tenant y claims

Todas las operaciones usan `GlampingId` resuelto en backend desde BD (tabla `UserTenants`), buscando por claim `sub` (Firebase UID). Nunca se toma tenant desde el body.

Claims esperados:

- `sub`: Firebase UID (requerido)
- `email`: opcional
- `role`: opcional en token (se sobreescribe/inyecta desde BD para policies)

Si el usuario autenticado no esta asociado en `UserTenants` (o no esta `Active`), la API responde `403` con `USER_NOT_ONBOARDED`.

### Modelo UserTenants

`UserTenant`:
- `Id` (Guid)
- `FirebaseUid` (string, UNIQUE)
- `Email` (nullable)
- `GlampingId` (FK)
- `Role` (`Admin`, `Reception`, `Restaurant`, `Inventory`)
- `Status` (`Pending`, `Active`, `Disabled`)

## Firebase JWT config

En `appsettings*.json`:

```json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id"
  }
}
```

La API valida:

- Issuer/Authority: `https://securetoken.google.com/{projectId}`
- Audience: `{projectId}`

## Auditoria

`AppDbContext` setea automaticamente:

- `CreatedAt`, `UpdatedAt`
- `CreatedByUserId`, `UpdatedByUserId` (si existe UserTenant.Id)
- `CreatedByFirebaseUid`, `UpdatedByFirebaseUid` desde Firebase `sub`

## Endpoints principales

CRUD:

- `GET/POST/PUT/DELETE /api/v1/units`
- `GET/POST/PUT/DELETE /api/v1/guests`
- `GET/POST/PUT/DELETE /api/v1/products`
- `GET/POST/PUT/DELETE /api/v1/categories`
- `GET/POST/PUT/DELETE /api/v1/locations`
- `GET/POST/PUT/DELETE /api/v1/reservations`
- `GET/POST/PUT/DELETE /api/v1/user-tenants` (solo `Admin`)

Lectura:

- `GET /api/v1/stays`
- `GET /api/v1/stays/{id}`
- `GET /api/v1/folios/{folioId}`

Flujos de negocio:

- `POST /api/v1/reservations/{id}/check-in`
- `POST /api/v1/folios/{folioId}/charges`
- `POST /api/v1/folios/{folioId}/payments`
- `POST /api/v1/stays/{stayId}/check-out`

List endpoints soportan:

- `page`
- `pageSize`
- `search`
- `sort`

## Reglas de negocio implementadas

- Reserva:
  - `Code` unico por glamping
  - validacion de solapamiento por unidad/rango
- Check-in:
  - solo si `ReservationStatus == Confirmed`
  - crea `Stay` + `Folio` open
  - opcional cargo ROOM
  - marca unidad `Occupied`
- Consumo:
  - no agrega cargos a folio cerrado
  - calcula precios por producto (`SalePrice`) salvo override permitido por config
  - para `TrackStock=true`: valida stock, crea `StockMovement OUT`, descuenta `StockBalance`
- Pago:
  - no permite montos <= 0
  - no permite pagos en folio cerrado
- Check-out:
  - saldo = cargos - pagos pagados
  - si saldo != 0 y `force != true` -> `409`
  - cierra folio y stay, marca unidad `Dirty`

## Config adicional

`BusinessRules`:

```json
{
  "BusinessRules": {
    "AllowOverridePrice": false
  }
}
```

## Ejecutar

```bash
dotnet restore
dotnet build
dotnet run
```

Swagger UI (dev):

- `https://localhost:<port>/swagger`

## Migraciones

```bash
dotnet ef migrations add <Name> -o Infrastructure/Persistence/Migrations
dotnet ef database update
```

Si no tienes SDK local compatible, puedes usar Docker:

```bash
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 bash -lc \
"dotnet tool install --tool-path /tmp/tools dotnet-ef --version 10.* && \
/tmp/tools/dotnet-ef database update"
```

## Onboarding inicial (sin custom claims)

1. El usuario inicia sesion en Firebase y obtiene JWT valido.
2. Backend identifica al usuario por `sub`.
3. Debe existir fila activa en `UserTenants` para ese `FirebaseUid`.

Seed incluye un registro admin con `FirebaseUid = CHANGE_ME_FIREBASE_UID`. Antes del primer uso, actualiza ese valor por el UID real del admin (y deja `Status = Active`), o crea un registro equivalente en BD.

## Ejemplos de uso (Bearer)

Header:

```http
Authorization: Bearer <firebase-jwt>
```

### 1) Crear reserva

`POST /api/v1/reservations`

```json
{
  "code": "RES-2026-0001",
  "guestId": "11111111-1111-1111-1111-111111111111",
  "unitId": "22222222-2222-2222-2222-222222222222",
  "checkInDate": "2026-03-10",
  "checkOutDate": "2026-03-12",
  "totalEstimated": 450000,
  "status": 2
}
```

### 2) Check-in

`POST /api/v1/reservations/{reservationId}/check-in`

```json
{
  "checkInAt": "2026-03-10T15:00:00Z",
  "roomUnitPrice": 225000,
  "roomNights": 2,
  "roomDescription": "Room package"
}
```

### 3) Registrar consumo

`POST /api/v1/folios/{folioId}/charges`

```json
{
  "source": 2,
  "description": "Minibar consumption",
  "locationId": "33333333-3333-3333-3333-333333333333",
  "allowOverridePrice": false,
  "items": [
    {
      "productId": "44444444-4444-4444-4444-444444444444",
      "qty": 2
    }
  ]
}
```

### 4) Registrar pago

`POST /api/v1/folios/{folioId}/payments`

```json
{
  "amount": 300000,
  "method": 2,
  "reference": "POS-991122",
  "paidAt": "2026-03-12T10:15:00Z"
}
```

### 5) Check-out

`POST /api/v1/stays/{stayId}/check-out`

```json
{
  "force": false,
  "checkOutAt": "2026-03-12T11:00:00Z"
}
```
