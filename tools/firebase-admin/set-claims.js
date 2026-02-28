#!/usr/bin/env node

const fs = require('node:fs');
const path = require('node:path');
const admin = require('firebase-admin');

const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function parseArgs() {
  const args = process.argv.slice(2);
  const parsed = {};

  for (const arg of args) {
    if (!arg.startsWith('--')) continue;
    const [rawKey, rawValue] = arg.slice(2).split('=');
    if (!rawKey) continue;
    parsed[rawKey] = rawValue ?? '';
  }

  return parsed;
}

function requireArg(name, value) {
  if (!value) {
    throw new Error(`Missing required argument --${name}`);
  }

  return value;
}

async function main() {
  const args = parseArgs();

  const uid = requireArg('uid', args.uid);
  const glampingId = requireArg('glamping_id', args.glamping_id);
  const role = args.role;
  const serviceAccountPath = requireArg('serviceAccount', args.serviceAccount || process.env.SERVICE_ACCOUNT_JSON);

  if (!GUID_REGEX.test(glampingId)) {
    throw new Error('glamping_id must be a valid GUID.');
  }

  const absolutePath = path.resolve(serviceAccountPath);
  if (!fs.existsSync(absolutePath)) {
    throw new Error(`Service account file not found: ${absolutePath}`);
  }

  const serviceAccount = JSON.parse(fs.readFileSync(absolutePath, 'utf8'));

  admin.initializeApp({
    credential: admin.credential.cert(serviceAccount)
  });

  const claims = {
    glamping_id: glampingId
  };

  if (role) {
    claims.role = role;
  }

  await admin.auth().setCustomUserClaims(uid, claims);

  console.log('Claims assigned successfully.');
  console.log({ uid, claims });
}

main().catch((error) => {
  console.error(error.message || error);
  process.exit(1);
});
