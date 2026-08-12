import { url } from "./API";

const ACCESS_TOKEN_KEY = "access-token";
/** Refresh a bit before access JWT expires so the user is not kicked mid-use. */
const REFRESH_SKEW_MS = 60_000;

const parseJwtPayload = (token: string): { exp?: number } | null => {
    try {
        const part = token.split(".")[1];
        if (!part) return null;
        const normalized = part.replace(/-/g, "+").replace(/_/g, "/");
        const json = atob(normalized);
        return JSON.parse(json);
    } catch {
        return null;
    }
};

const getAccessToken = () =>
    typeof window === "undefined" ? null : localStorage.getItem(ACCESS_TOKEN_KEY);

/** True when there is no token or JWT `exp` is in the past. */
const isAccessTokenExpired = (token?: string | null): boolean => {
    const t = token ?? getAccessToken();
    if (!t) return true;
    const payload = parseJwtPayload(t);
    if (!payload?.exp) return true;
    return Date.now() >= payload.exp * 1000;
};

/** True when token is missing, expired, or within the refresh skew window. */
const shouldRefreshAccessToken = (token?: string | null): boolean => {
    const t = token ?? getAccessToken();
    if (!t) return true;
    const payload = parseJwtPayload(t);
    if (!payload?.exp) return true;
    return Date.now() >= payload.exp * 1000 - REFRESH_SKEW_MS;
};

/** Milliseconds until JWT expiry, or 0 if already expired / missing. */
const getMsUntilExpiry = (): number => {
    const t = getAccessToken();
    if (!t) return 0;
    const payload = parseJwtPayload(t);
    if (!payload?.exp) return 0;
    return Math.max(0, payload.exp * 1000 - Date.now());
};

/** Milliseconds until we should silently refresh (expiry minus skew). */
const getMsUntilRefresh = (): number => {
    const t = getAccessToken();
    if (!t) return 0;
    const payload = parseJwtPayload(t);
    if (!payload?.exp) return 0;
    return Math.max(0, payload.exp * 1000 - REFRESH_SKEW_MS - Date.now());
};

const login = async (code: string) => {
    const res = await fetch(`${url}/auth/login`, {
        method: "POST",
        body: JSON.stringify({ code }),
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
        },
    });
    if (res.status >= 400) {
        return false;
    }
    const data: { data: string; error: boolean; message: string } =
        await res.json();
    if (!data.error) localStorage.setItem(ACCESS_TOKEN_KEY, data.data);
    return true;
};

const authHeader = () => {
    const token = getAccessToken();
    if (!token || isAccessTokenExpired(token)) return false;
    return { Authorization: `Bearer ${token}` };
};

const logout = async () => {
    await fetch(`${url}/auth/logout`, {
        method: "POST",
        credentials: "include",
    });
    localStorage.removeItem(ACCESS_TOKEN_KEY);
};

/** Marks the open session as logged out by the system (token/session expired). */
const endSessionExpired = async () => {
    try {
        await fetch(`${url}/auth/session-expired`, {
            method: "POST",
            credentials: "include",
        });
    } catch (error) {
        console.error(error);
    }
    localStorage.removeItem(ACCESS_TOKEN_KEY);
};

let refreshInFlight: Promise<boolean> | null = null;

/**
 * Silently renew access JWT (+ refresh cookie) via HttpOnly refresh token.
 * Returns true if a usable access token is available afterwards.
 */
const refreshAccessToken = async (): Promise<boolean> => {
    if (refreshInFlight) return refreshInFlight;

    refreshInFlight = (async () => {
        try {
            const res = await fetch(`${url}/auth/refresh-token`, {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (!res.ok) {
                return false;
            }

            const data: { data?: string; error?: boolean; message?: string } =
                await res.json();

            if (data.error || !data.data) {
                return false;
            }

            localStorage.setItem(ACCESS_TOKEN_KEY, data.data);
            return true;
        } catch (error) {
            console.error(error);
            return false;
        } finally {
            refreshInFlight = null;
        }
    })();

    return refreshInFlight;
};

/**
 * Ensures a valid access token. If expired / near expiry, refreshes behind the scenes.
 * Only ends the server session when refresh fails (user truly idle / logged out).
 */
const ensureValidSession = async (): Promise<boolean> => {
    if (!getAccessToken() && !document.cookie) {
        // May still have HttpOnly refresh cookie even if access token was cleared
    }

    if (!shouldRefreshAccessToken()) {
        return true;
    }

    const refreshed = await refreshAccessToken();
    if (refreshed && !isAccessTokenExpired()) {
        return true;
    }

    // Refresh failed — session is over
    await endSessionExpired();
    return false;
};

const getUser = async () => {
    try {
        if (!(await ensureValidSession())) {
            return false;
        }

        const _authHeader = authHeader();
        if (_authHeader) {
            const res = await fetch(`${url}/auth/about-me`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ..._authHeader,
                },
            });
            if (res.status === 401 || res.status === 403) {
                // Try one silent refresh then retry about-me
                const refreshed = await refreshAccessToken();
                if (refreshed) {
                    const retryHeader = authHeader();
                    if (retryHeader) {
                        const retry = await fetch(`${url}/auth/about-me`, {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json",
                                ...retryHeader,
                            },
                        });
                        if (retry.ok) {
                            return await retry.json();
                        }
                    }
                }
                await endSessionExpired();
                return false;
            }
            if (res.status == 404) {
                await logout();
                return false;
            }
            const data: {
                data: {
                    name: string;
                    role: UserRole;
                    id: number;
                    group: string;
                };
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        }
        return false;
    } catch (error) {
        console.error(error);
        return false;
    }
};

const authService = {
    login,
    authHeader,
    logout,
    endSessionExpired,
    getUser,
    isAccessTokenExpired,
    shouldRefreshAccessToken,
    getMsUntilExpiry,
    getMsUntilRefresh,
    ensureValidSession,
    refreshAccessToken,
};

export default authService;
