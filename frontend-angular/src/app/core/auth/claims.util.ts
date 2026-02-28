export interface DecodedClaims {
  sub?: string;
  email?: string;
  role?: string;
}

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
