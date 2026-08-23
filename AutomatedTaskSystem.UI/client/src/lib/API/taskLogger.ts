import { url } from ".";
import authService from "../Auth";

const buildQuery = (filters: TaskLoggerFilters = {}) => {
    const params = new URLSearchParams();
    if (filters.from) params.set("from", filters.from);
    if (filters.to) params.set("to", filters.to);
    if (filters.search) params.set("search", filters.search);
    if (filters.member) params.set("member", filters.member);
    if (filters.subject) params.set("subject", filters.subject);
    if (filters.status) params.set("status", filters.status);
    if (filters.taskName) params.set("taskName", filters.taskName);
    if (filters.rankingRange) params.set("rankingRange", filters.rankingRange);
    if (filters.page) params.set("page", String(filters.page));
    if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
    const query = params.toString();
    return query ? `?${query}` : "";
};

const getHeaders = (): HeadersInit => {
    const authHeader = authService.authHeader();
    if (authHeader) return { ...authHeader };
    return {};
};

const parseJson = async <T>(res: Response): Promise<ResponseService<T> | false> => {
    if (!res.ok) {
        let message = `HTTP ${res.status}`;
        try {
            const body = await res.json();
            message = body?.message ?? body?.Message ?? message;
        } catch {
            // ignore
        }
        return { error: true, message, data: undefined as unknown as T };
    }
    return (await res.json()) as ResponseService<T>;
};

const taskLogger = {
    GET_DASHBOARD: async (filters: TaskLoggerFilters = {}) => {
        try {
            const res = await fetch(`${url}/task-logger/dashboard${buildQuery(filters)}`, {
                headers: getHeaders(),
            });
            return await parseJson<TaskLoggerDashboard>(res);
        } catch (err) {
            console.error("TASK_LOGGER GET_DASHBOARD failed:", err);
            return false;
        }
    },
    GET_ROWS: async (filters: TaskLoggerFilters = {}) => {
        try {
            const res = await fetch(`${url}/task-logger${buildQuery(filters)}`, {
                headers: getHeaders(),
            });
            return await parseJson<TaskLoggerPagedResult>(res);
        } catch (err) {
            console.error("TASK_LOGGER GET_ROWS failed:", err);
            return false;
        }
    },
    GET_LOOKUPS: async () => {
        try {
            const res = await fetch(`${url}/task-logger/lookups`, {
                headers: getHeaders(),
            });
            return await parseJson<TaskLoggerLookups>(res);
        } catch (err) {
            console.error("TASK_LOGGER GET_LOOKUPS failed:", err);
            return false;
        }
    },
};

export default taskLogger;
