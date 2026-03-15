# Onboarding de Usuarios

Ver tambien: [Contexto](./CONTEXT.md), [Arquitectura](./ARCHITECTURE.md), script [bootstrap_admin.sql](../scripts/sql/bootstrap_admin.sql).

## Idea clave
No hay login/register en backend.

El acceso depende de:
1. Token Firebase valido
2. Registro en tabla `UserTenants` con `Status=Active`

Si falta (2), backend responde `403 USER_NOT_ONBOARDED`.

## 1) Habilitar primer Admin (SQL directo)
Usa el script:

- `scripts/sql/bootstrap_admin.sql`

Resumen:
- Define `firebaseUid` real del usuario Firebase
- Define `email`
- Define `glampingId`
- Ejecuta script en PostgreSQL

Eso crea/actualiza el registro y fuerza:
- `Role=1 (Admin)`
- `Status=2 (Active)`

## 2) Agregar mas usuarios (via API)
Requiere un Admin ya activo.

Endpoint:
- `POST /api/v1/user-tenants`

Ejemplo `curl`:
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

## 3) Validar usuario actual
Endpoint recomendado para onboarding check:
- `GET /api/v1/user-tenants/me`

Ejemplo:
```bash
curl -i "http://localhost:4400/api/v1/user-tenants/me" \
  -H "Authorization: Bearer <TOKEN_USUARIO>"
```

## Troubleshooting rapido

### 401 Unauthorized
Significa token invalido/no autenticado.
Revisar:
- `Firebase__ProjectId` del backend
- Issuer/audience del token
- Header `Authorization: Bearer <idToken>`

### 403 USER_NOT_ONBOARDED
Token valido, pero UID no existe en `UserTenants` o no esta activo.
Accion:
- Crear/actualizar registro en `UserTenants`

### 403 USER_DISABLED
Usuario existe pero esta `Status=3`.
Accion:
- Cambiar a `Status=2` o mantener bloqueado segun politica

### 404 en endpoint de onboarding
Si un endpoint no existe en tu version (`/me`, `/current`), usa el disponible y revisa Swagger.

