# Firebase Admin Tools

Script utilitario para asignar custom claims requeridos por el backend (`glamping_id`, `role`).

## Uso

1. Descarga tu Service Account JSON desde Firebase Console.
2. Instala dependencias:

```bash
npm install
```

3. Ejecuta el script:

```bash
node set-claims.js --serviceAccount=./serviceAccountKey.json --uid=USER_UID --glamping_id=11111111-1111-1111-1111-111111111111 --role=Admin
```

Argumentos:

- `--serviceAccount` (required): path al archivo json de service account
- `--uid` (required): UID del usuario Firebase
- `--glamping_id` (required): GUID del glamping
- `--role` (optional): rol (Admin, Reception, Restaurant, Inventory)

Tras asignar claims, el usuario debe reloguearse para que el token traiga los nuevos claims.
