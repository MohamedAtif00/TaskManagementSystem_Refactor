import { url } from "./index";
import authService from "../Auth";

export interface UserSessionRow {
    id: number;
    userId: number;
    userName: string;
    loginAt: string;
    logoutAt?: string | null;
    durationMinutes?: number | null;
    totalHours?: number | null;
    durationFormatted: string;
    reason?: string | null;
    reasonCode?: string | null;
    ipAddress?: string | null;
}

export interface UserSessionList {
    items: UserSessionRow[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface DayStorySegment {
    start: string;
    end: string;
    kind: "Active" | "Inactive" | string;
    hours: number;
    durationFormatted: string;
    reason?: string | null;
    sessionId?: number | null;
}

export interface UserDayStoryline {
    userId: number;
    userName: string;
    date: string;
    dayStart: string;
    dayEnd: string;
    activeHours: number;
    inactiveHours: number;
    sessionCount: number;
    segments: DayStorySegment[];
    sessions: UserSessionRow[];
}

export interface GetSessionsParams {
    userId?: number;
    from?: string;
    to?: string;
    reason?: number;
    page?: number;
    pageSize?: number;
}

const SESSIONS = {
    GET: async (params: GetSessionsParams = {}): Promise<{
        data?: UserSessionList;
        error?: boolean;
        message?: string;
    } | false> => {
        try {
            const headers = authService.authHeader();
            if (!headers) return false;

            const query = new URLSearchParams();
            if (params.userId != null) query.set("userId", String(params.userId));
            if (params.from) query.set("from", params.from);
            if (params.to) query.set("to", params.to);
            if (params.reason != null) query.set("reason", String(params.reason));
            if (params.page != null) query.set("page", String(params.page));
            if (params.pageSize != null) query.set("pageSize", String(params.pageSize));

            const qs = query.toString();
            const res = await fetch(`${url}/auth/sessions${qs ? `?${qs}` : ""}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    ...headers,
                },
            });

            if (res.status === 401 || res.status === 403) {
                return { error: true, message: "Unauthorized" };
            }

            return await res.json();
        } catch (error) {
            console.error(error);
            return false;
        }
    },

    GET_DAY: async (userId: number, date: string): Promise<{
        data?: UserDayStoryline;
        error?: boolean;
        message?: string;
    } | false> => {
        try {
            const headers = authService.authHeader();
            if (!headers) return false;

            const query = new URLSearchParams({
                userId: String(userId),
                date,
            });

            const res = await fetch(`${url}/auth/sessions/day?${query}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    ...headers,
                },
            });

            if (res.status === 401 || res.status === 403) {
                return { error: true, message: "Unauthorized" };
            }

            return await res.json();
        } catch (error) {
            console.error(error);
            return false;
        }
    },

    FORCE_LOGOUT: async (userId: number): Promise<{
        error?: boolean;
        message?: string;
    } | false> => {
        try {
            const headers = authService.authHeader();
            if (!headers) return false;

            const res = await fetch(`${url}/auth/force-logout/${userId}`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...headers,
                },
            });

            return await res.json();
        } catch (error) {
            console.error(error);
            return false;
        }
    },
};

export default SESSIONS;
