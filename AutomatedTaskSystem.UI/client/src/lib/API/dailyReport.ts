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
    if (filters.problemType) params.set("problemType", filters.problemType);
    if (filters.priority) params.set("priority", filters.priority);
    if (filters.page) params.set("page", String(filters.page));
    if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
    const query = params.toString();
    return query ? `?${query}` : "";
};

const dailyReport = {
    GET_DASHBOARD: async (filters: DailyReportFilters = {}) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports/dashboard${buildQuery(filters)}`, {
                headers: { ...authHeader },
            });
            const data: ResponseService<DailyReportDashboard> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_ROWS: async (filters: DailyReportFilters = {}) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports${buildQuery(filters)}`, {
                headers: { ...authHeader },
            });
            const data: ResponseService<DailyReportPagedResult> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_SUMMARY: async (filters: DailyReportFilters = {}) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports/summary${buildQuery(filters)}`, {
                headers: { ...authHeader },
            });
            const data: ResponseService<DailyReportSummary> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_CHARTS: async (filters: DailyReportFilters = {}) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports/charts${buildQuery(filters)}`, {
                headers: { ...authHeader },
            });
            const data: ResponseService<DailyReportChartData> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_LOOKUPS: async () => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports/lookups`, {
                headers: { ...authHeader },
            });
            const data: ResponseService<DailyReportLookups> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    PATCH_NOTES: async (taskId: number, notes: string) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/daily-reports/${taskId}/notes`, {
                method: "PATCH",
                headers: {
                    ...authHeader,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ notes }),
            });
            const data: ResponseService<string> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
};

export default dailyReport;
