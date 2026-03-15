# Prompts Canon (Reutilizables)

Ver tambien: [Contexto](./CONTEXT.md), [Arquitectura](./ARCHITECTURE.md), [Onboarding](./ONBOARDING.md).

## 1) Prompt base para cambios backend
```text
Actua como backend engineer senior en .NET para este repo.
Objetivo: <describe objetivo concreto>.

Reglas:
- Mantener arquitectura por capas actual (Api/Application/Infrastructure/Domain).
- No romper multi-tenant: resolver contexto por UserTenants (Firebase sub -> GlampingId/Role).
- No usar glamping_id en claims.
- Mantener endpoints bajo /api/v1.
- Controllers delgados, logica en services.
- Validar con FluentValidation si aplica.
- Mantener integridad EF Core (FK, indices, restricciones).
- Compilar al final y reportar archivos modificados.

Entrega:
- Patch completo en archivos.
- Lista de archivos creados/modificados.
- Comandos de prueba (curl o dotnet).
```

## 2) Prompt base para generar/ajustar frontend Angular
```text
Actua como frontend engineer senior en Angular standalone.
Objetivo: <describe objetivo UX/funcional>.

Reglas:
- Mantener estilo minimalista actual.
- Auth con Firebase Web SDK.
- Enviar Bearer token en cada request API.
- Onboarding check por API (preferir /api/v1/user-tenants/me).
- Si 403 USER_NOT_ONBOARDED/USER_DISABLED -> /no-access.
- No depender de glamping_id en JWT.
- No inventar endpoints: usar existentes o indicar "ver Swagger".
- Compilar al final (ng build) y listar archivos cambiados.

Entrega:
- Codigo completo necesario.
- Rutas/componentes/interceptores afectados.
- Pasos de verificacion.
```

## 3) Prompt para agregar feature end-to-end
```text
Actua como maintainer senior full-stack y agrega la feature <feature>.

Incluye:
- Backend: DTOs, servicio, validaciones, controller, migracion si aplica.
- Frontend: vista/listado/form, consumo API, manejo de errores.
- Seguridad: respetar UserTenants (sub -> tenant/role).
- Docker/dev: actualizar config si hace falta.

Restricciones:
- No romper contratos existentes.
- Mantener codigo simple y coherente con el repo.
- Documentar en /docs si la feature cambia flujos.

Entrega:
- Lista de archivos creados/modificados.
- Codigo aplicado.
- Resultado de build backend/frontend.
```

## 4) Prompt para hardening de auth/autorizacion
```text
Revisa y endurece autenticacion/autorizacion del proyecto.

Checklist:
- JWT Firebase issuer/audience.
- Lectura de Firebase__ProjectId por entorno.
- Pipeline: UseAuthentication -> UseAuthorization -> MapControllers.
- Manejo de 401 vs 403 consistente.
- Logs utiles en JwtBearerEvents (temporal si aplica).
- UserTenants y estados Pending/Active/Disabled.
- Verifica /api/v1/user-tenants/me.

Entrega:
- Hallazgos priorizados.
- Cambios concretos con patch.
- Pruebas de curl para caso valido/invalido.
```

