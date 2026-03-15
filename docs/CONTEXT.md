# Contexto del Proyecto

Ver tambien: [Arquitectura](./ARCHITECTURE.md), [Onboarding](./ONBOARDING.md), [Prompts canon](./PROMPTS.md).

## Resumen
Sistema de administracion de glampings multi-tenant.

- Backend: .NET + EF Core + PostgreSQL
- Frontend: Angular + Firebase Auth (Google/email-password)
- Orquestacion: Docker Compose + Nginx reverse proxy

## Modulos funcionales
- Hospedaje: unidades, huespedes, reservas, estadias
- Caja/Folio: cargos, items, pagos, cierre
- Productos e inventario: categorias, productos, ubicaciones, stock
- Multi-tenant y usuarios internos por glamping

## Entidades principales
- `Glamping`
- `UserTenant` (autorizacion y tenancy por UID Firebase)
- `Guest`, `Unit`, `Reservation`, `Stay`
- `Folio`, `Charge`, `ChargeItem`, `Payment`
- `ProductCategory`, `Product`, `Location`, `StockBalance`, `StockMovement`

## Endpoints base
Todos bajo `/api/v1` y requieren Bearer token valido (excepto endpoints anonimos si existieran).

CRUD principales:
- `/units`
- `/guests`
- `/products`
- `/categories`
- `/locations`
- `/reservations`

Lectura/operacion:
- `/stays`
- `/folios/{folioId}`

Flujos de negocio:
- `POST /reservations/{id}/check-in`
- `POST /folios/{folioId}/charges`
- `POST /folios/{folioId}/payments`
- `POST /stays/{stayId}/check-out`

Usuarios/tenancy:
- `GET /user-tenants/me` (usuario autenticado actual)
- `GET/POST/PUT/DELETE /user-tenants` (solo Admin)

Nota: para endpoints exactos y contratos vigentes, revisar Swagger en runtime.

## Reglas de negocio relevantes
- Reserva:
  - Codigo unico por glamping
  - Validacion de solapamiento de unidad/rango
- Check-in:
  - Solo reserva `Confirmed`
  - Crea `Stay` y `Folio` abierto
  - Ocupa unidad
- Consumo:
  - No permitir en folio cerrado
  - Si producto con stock, valida saldo y descuenta stock
- Pago:
  - Monto > 0
  - No permitir en folio cerrado
- Check-out:
  - Calcula saldo (cargos - pagos)
  - Si hay saldo y no `force`, responde conflicto
  - Cierra folio/estadia y marca unidad `Dirty`

## Flujo operativo resumido
1. Crear reserva
2. Check-in de reserva -> crea `Stay` + `Folio`
3. Registrar consumos/cargos durante la estadia
4. Registrar pagos
5. Check-out y cierre final

