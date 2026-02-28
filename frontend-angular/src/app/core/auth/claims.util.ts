export interface DecodedClaims {
  sub?: string;
  email?: string;
  role?: string;
  glamping_id?: string;
}

const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export function parseJwtClaims(token: string): DecodedClaims | null {
  const chunks = token.split('.');
  if (chunks.length < 2) {
    return null;
  }

  try {
    const base64 = chunks[1].replace(/-/g, '+').replace(/_/g, '/');
    const decoded = JSON.parse(atob(base64)) as DecodedClaims;
    return decoded;
  } catch {
    return null;
  }
}

export function getGlampingIdFromClaims(claims: DecodedClaims | null): string | null {
  const value = claims?.glamping_id;
  if (!value) {
    return null;
  }

  return GUID_REGEX.test(value) ? value : null;
}
