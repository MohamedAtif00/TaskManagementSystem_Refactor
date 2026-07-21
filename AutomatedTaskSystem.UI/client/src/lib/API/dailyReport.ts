import { url } from ".";
import authService from "../Auth";

const buildQuery = (filters: DailyReportFilters = {}) => {
    const params = new URLSearchParams();
    if (filters.from) params.set("from", filters.from);
    if (filters.to) params.set("to", filters.to);
    if (filters.team) params.set("team", filters.team);
    if (filters.semester) params.set("semester", filters.semester);
    if (filters.subject) params.set("subject", filters.subject);
    if (filters.grade) params.set("grade", filters.grade);
    if (filters.taskName) params.set("taskName", filters.taskName);
    if (filters.status) params.set("status", filters.status);
    if (filters.problemTypes?.length) {
        filters.problemTypes.forEach((p) => {
            if (p) params.append("problemType", p);
        });
    }
    if (filters.priority) params.set("priority", filters.priority);
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
            // ignore non-JSON error bodies
        }
        return { error: true, message, data: undefined as unknown as T };
    }

    return (await res.json()) as ResponseService<T>;
};

const dailyReport = {
    GET_DASHBOARD: async (filters: DailyReportFilters = {}) => {
        try {
            const endpoint = `${url}/daily-reports/dashboard${buildQuery(filters)}`;
            const res = await fetch(endpoint, { headers: getHeaders() });
            return await parseJson<DailyReportDashboard>(res);
        } catch (err) {
            console.error("GET_DASHBOARD failed:", err);
            return false;
        }
    },
    GET_ROWS: async (filters: DailyReportFilters = {}) => {
        try {
            const res = await fetch(`${url}/daily-reports${buildQuery(filters)}`, {
                headers: getHeaders(),
            });
            return await parseJson<DailyReportPagedResult>(res);
        } catch (err) {
            console.error("GET_ROWS failed:", err);
            return false;
        }
    },
    GET_SUMMARY: async (filters: DailyReportFilters = {}) => {
        try {
            const res = await fetch(`${url}/daily-reports/summary${buildQuery(filters)}`, {
                headers: getHeaders(),
            });
            return await parseJson<DailyReportSummary>(res);
        } catch (err) {
            console.error("GET_SUMMARY failed:", err);
            return false;
        }
    },
    GET_CHARTS: async (filters: DailyReportFilters = {}) => {
        try {
            const res = await fetch(`${url}/daily-reports/charts${buildQuery(filters)}`, {
                headers: getHeaders(),
            });
            return await parseJson<DailyReportChartData>(res);
        } catch (err) {
            console.error("GET_CHARTS failed:", err);
            return false;
        }
    },
    GET_LOOKUPS: async () => {
        try {
            const res = await fetch(`${url}/daily-reports/lookups`, {
                headers: getHeaders(),
            });
            return await parseJson<DailyReportLookups>(res);
        } catch (err) {
            console.error("GET_LOOKUPS failed:", err);
            return false;
        }
    },
    PATCH_NOTES: async (taskId: number, notes: string) => {
        try {
            const res = await fetch(`${url}/daily-reports/${taskId}/notes`, {
                method: "PATCH",
                headers: {
                    ...getHeaders(),
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ notes }),
            });
            return await parseJson<string>(res);
        } catch (err) {
            console.error("PATCH_NOTES failed:", err);
            return false;
        }
    },
};

export default dailyReport;
