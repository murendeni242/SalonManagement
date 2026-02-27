// UPDATED — no longer needs jwtDecode or Microsoft claim URI strings.
// Your backend LoginHandler now returns { token, role, email, expiresAt }
// We store the whole object in localStorage as "salon_auth".
// Reading the role is now just JSON.parse — clean and simple.

const STORAGE_KEY = "salon_auth";

interface StoredAuth {
  token: string;
  role: string;
  email: string;
  expiresAt: string;
}

function getStoredAuth(): StoredAuth | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const auth = JSON.parse(raw) as StoredAuth;

    // Check token hasn't expired
    if (new Date(auth.expiresAt) < new Date()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return auth;
  } catch {
    return null;
  }
}

export function getUserRole(): string | null {
  return getStoredAuth()?.role ?? null;
}

export function getUserEmail(): string | null {
  return getStoredAuth()?.email ?? null;
}

export function getUserId(): string | null {
  // No longer needed for role checks.
  // If you need the staff's own ID for filtering, store it in the auth object.
  // For now, the backend filters by the JWT sub claim server-side.
  return null;
}

export function isLoggedIn(): boolean {
  return getStoredAuth() !== null;
}