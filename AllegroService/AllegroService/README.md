# AllegroService - EF Core Code First (Glamping Multi-tenant)

Modelo de datos para administracion de glampings usando .NET 10 + EF Core 10 con PostgreSQL (Npgsql).

## Estructura

- `Domain/Entities`: POCOs del dominio.
- `Domain/Enums`: enums de estado y tipos operativos.
- `Infrastructure/Persistence/AppDbContext.cs`: DbContext + stamping automatico de auditoria.
- `Infrastructure/Persistence/Configurations`: Fluent API por entidad.
- `Infrastructure/Persistence/Migrations`: migracion inicial.
- `Infrastructure/Persistence/Seed/SeedData.cs`: seed minimo.

## Decisiones de modelado

- Multi-tenant por `GlampingId` en todas las tablas operativas.
- Indice por `GlampingId` en todas las tablas tenant-scoped.
- Auditoria en todas las entidades:
  - `CreatedAt`, `UpdatedAt`, `CreatedByUserId`, `UpdatedByUserId`.
- `DeleteBehavior.Restrict` por defecto para proteger historico (sin cascadas destructivas).
- Sin soft-delete global en esta version:
  - Recomendado para dominios financieros/operativos con historial fuerte.
  - Si se requiere, se puede agregar `IsDeleted` + query filter global en una migracion incremental.

## Integridad y reglas relevantes

- `Reservation`: unico `(GlampingId, Code)` + check `CheckOutDate > CheckInDate`.
- `Stay`: indice unico parcial por unidad activa (`Status = CheckedIn`).
- `Folio`: indice unico parcial para 1 folio abierto por `Stay` (`Status = Open`).
- `ProductCategory`: unico `(GlampingId, Name)`.
- `Product`: unico `(GlampingId, Sku)`.
- `Location`: unico `(GlampingId, Name)`.
- `StockMovement`: indices en `(GlampingId, ProductId)`, `(GlampingId, LocationId)`, `(ReferenceType, ReferenceId)`.

## Seed incluido

- 1 glamping demo.
- 1 usuario admin demo (`admin@demo-glamping.local`, password hash placeholder `CHANGE_ME`).
- Categorias por defecto: `Alimentos`, `Bebidas`, `Extras`.
- 1 ubicacion inicial: `Main Warehouse`.

## Configuracion de EF en Program.cs

`AppDbContext` esta registrado con `UseNpgsql` y `ConnectionStrings:DefaultConnection`.

Para SQL Server:
1. Cambiar provider a `Microsoft.EntityFrameworkCore.SqlServer`.
2. Reemplazar `UseNpgsql(...)` por `UseSqlServer(...)`.
3. Regenerar migraciones para ese proveedor.

## Comandos de migraciones

```bash
dotnet restore
dotnet ef migrations add InitialCreate -o Infrastructure/Persistence/Migrations
dotnet ef database update
```

Si no tienes SDK .NET 10 local, puedes usar Docker:

```bash
docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 bash -lc \
"dotnet tool install --tool-path /tmp/tools dotnet-ef --version 10.* && \
 /tmp/tools/dotnet-ef database update"
```

## Flujo operativo (ejemplo)

1. Crear reserva
- Insertar `Reservation` con `GuestId`, `CheckInDate`, `CheckOutDate`, `Status=Confirmed` y `Code` unico por glamping.

2. Check-in
- Crear `Stay` (`Status=CheckedIn`, `UnitId`, opcional `ReservationId`).
- Crear `Folio` asociado al `Stay` (`Status=Open`, `OpenedAt=UTC`).

3. Registrar consumo
- Crear `Charge` en el `Folio` (contenedor de cargo).
- Crear `ChargeItem`(s) opcionales por producto.
- Registrar `StockMovement` tipo `Out` para cada producto con control de inventario.

Notas:
- `Total` de `Charge`/`ChargeItem` se calcula en la aplicacion (`Qty * UnitPrice`).
- `StockBalance` se actualiza transaccionalmente junto con `StockMovement`.
