-- Bootstrap de primer Admin para opcion 3 (UserTenants)
-- Ejecutar en PostgreSQL del proyecto.
--
-- 1) Reemplaza los valores entre comillas en la seccion de parametros.
-- 2) Ejecuta este script una vez por admin inicial (o reutiliza para actualizar).

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Parametros
-- UID Firebase real del usuario admin
-- Ejemplo: 'aBcDeFg123...'
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
  1, -- Admin
  2, -- Active
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

-- Verificacion
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

