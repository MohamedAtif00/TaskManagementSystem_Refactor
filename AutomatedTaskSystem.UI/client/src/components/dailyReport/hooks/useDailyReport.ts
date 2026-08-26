import { useCallback, useEffect, useState } from "react";
import API from "../../../lib/API";
import { DEFAULT_PAGE_SIZE, createEmptyFilters } from "../constants";
import { PROBLEM_TYPES } from "../../../lib/problemTypes";

const emptySummary = (): DailyReportSummary => ({
    total: 0,
    approved: 0,
    hold: 0,
    rollback: 0,
    activeTeams: 0,
    topTeams: [],
});

const emptyCharts = (): DailyReportChartData => ({
    approved: 0,
    hold: 0,
    rollback: 0,
    problemTypes: [],
});

const emptyLookups = (): DailyReportLookups => ({
    teams: [],
    semesters: ["Term 1", "Term 2"],
    subjects: [],
    grades: [],
    taskNames: [],
    problemTypes: [...PROBLEM_TYPES],
    priorities: ["High", "Medium", "Low"],
});

/** Support both camelCase and PascalCase API payloads. */
function unwrapResponse<T>(raw: any): { error: boolean; message: string; data: T | null } {
    if (!raw || typeof raw !== "object") {
        return { error: true, message: "Empty response", data: null };
    }
    const error = Boolean(raw.error ?? raw.Error);
    const message = String(raw.message ?? raw.Message ?? "");
    const data = (raw.data ?? raw.Data ?? null) as T | null;
    return { error, message, data };
}

function normalizeDashboard(raw: any): DailyReportDashboard | null {
    if (!raw || typeof raw !== "object") return null;

    const paged = raw.rows ?? raw.Rows ?? {};
    const summary = raw.summary ?? raw.Summary ?? {};
    const charts = raw.charts ?? raw.Charts ?? {};
    const lookups = raw.lookups ?? raw.Lookups ?? {};

    return {
        rows: {
            rows: paged.rows ?? paged.Rows ?? [],
            totalCount: paged.totalCount ?? paged.TotalCount ?? 0,
            page: paged.page ?? paged.Page ?? 1,
            pageSize: paged.pageSize ?? paged.PageSize ?? DEFAULT_PAGE_SIZE,
            totalPages: paged.totalPages ?? paged.TotalPages ?? 0,
        },
        summary: {
            total: summary.total ?? summary.Total ?? 0,
            approved: summary.approved ?? summary.Approved ?? 0,
            hold: summary.hold ?? summary.Hold ?? 0,
            rollback: summary.rollback ?? summary.Rollback ?? 0,
            activeTeams: summary.activeTeams ?? summary.ActiveTeams ?? 0,
            topTeams: (summary.topTeams ?? summary.TopTeams ?? []).map((t: any) => ({
                team: t.team ?? t.Team ?? "",
                count: t.count ?? t.Count ?? 0,
            })),
        },
        charts: {
            approved: charts.approved ?? charts.Approved ?? 0,
            hold: charts.hold ?? charts.Hold ?? 0,
            rollback: charts.rollback ?? charts.Rollback ?? 0,
            problemTypes: (charts.problemTypes ?? charts.ProblemTypes ?? []).map((p: any) => ({
                problemType: p.problemType ?? p.ProblemType ?? "",
                count: p.count ?? p.Count ?? 0,
            })),
        },
        lookups: {
            teams: lookups.teams ?? lookups.Teams ?? [],
            semesters: lookups.semesters ?? lookups.Semesters ?? ["Term 1", "Term 2"],
            subjects: lookups.subjects ?? lookups.Subjects ?? [],
            grades: lookups.grades ?? lookups.Grades ?? [],
            taskNames: lookups.taskNames ?? lookups.TaskNames ?? [],
            problemTypes: lookups.problemTypes ?? lookups.ProblemTypes ?? [],
            priorities: lookups.priorities ?? lookups.Priorities ?? ["High", "Medium", "Low"],
        },
    };
}

export function useDailyReport() {
    const [filters, setFilters] = useState<DailyReportFilters>(() => createEmptyFilters());
    const [rows, setRows] = useState<DailyReportRow[]>([]);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        page: 1,
        pageSize: DEFAULT_PAGE_SIZE,
        totalPages: 0,
    });
    const [summary, setSummary] = useState<DailyReportSummary>(emptySummary);
    const [charts, setCharts] = useState<DailyReportChartData>(emptyCharts);
    const [lookups, setLookups] = useState<DailyReportLookups>(emptyLookups);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasLoadedOnce, setHasLoadedOnce] = useState(false);

    const fetchData = useCallback(async (activeFilters: DailyReportFilters) => {
        setLoading(true);
        setError(null);

        try {
            if (!API.DAILY_REPORT?.GET_DASHBOARD) {
                setError("Daily Report API is not available. Restart the Next.js dev server.");
                return;
            }

            const raw = await API.DAILY_REPORT.GET_DASHBOARD(activeFilters);
            const { error: isError, message, data } = unwrapResponse<any>(raw);

            if (isError || !data) {
                setError(message || "Failed to load daily report data. Check Network for /daily-reports/dashboard.");
                return;
            }

            const dashboard = normalizeDashboard(data);
            if (!dashboard) {
                setError("Daily report response was empty or invalid.");
                return;
            }

            setRows(dashboard.rows.rows ?? []);
            setPagination({
                totalCount: dashboard.rows.totalCount,
                page: dashboard.rows.page,
                pageSize: dashboard.rows.pageSize,
                totalPages: dashboard.rows.totalPages,
            });
            setSummary(dashboard.summary);
            setCharts(dashboard.charts);
            setLookups({
                ...dashboard.lookups,
                problemTypes:
                    dashboard.lookups.problemTypes?.length > 0
                        ? dashboard.lookups.problemTypes
                        : [...PROBLEM_TYPES],
            });
        } catch (err) {
            console.error("Daily report fetch failed:", err);
            setError("Daily report request failed. Is the API running on the configured URL?");
        } finally {
            setHasLoadedOnce(true);
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        void fetchData(filters);
    }, [filters, fetchData]);

    const updateFilter = (key: keyof DailyReportFilters, value: string) => {
        setFilters((prev) => ({
            ...prev,
            [key]: value,
            ...(key === "team" ? { taskName: "" } : {}),
            page: key === "page" ? Number(value) || 1 : 1,
        }));
    };

    const setPage = (page: number) => {
        setFilters((prev) => ({ ...prev, page }));
    };

    const setPageSize = (pageSize: number) => {
        setFilters((prev) => ({ ...prev, pageSize, page: 1 }));
    };

    const setProblemTypes = (problemTypes: string[]) => {
        setFilters((prev) => ({ ...prev, problemTypes, page: 1 }));
    };

    const resetFilters = () => setFilters(createEmptyFilters());

    const fetchAllRowsForExport = async () => {
        const raw = await API.DAILY_REPORT.GET_ROWS({
            ...filters,
            page: 1,
            pageSize: 500,
        });
        const { error: isError, data } = unwrapResponse<any>(raw);
        if (!isError && data) {
            const list = data.rows ?? data.Rows ?? [];
            return list;
        }
        return rows;
    };

    return {
        filters,
        rows,
        pagination,
        summary,
        charts,
        lookups,
        loading,
        error,
        hasLoadedOnce,
        updateFilter,
        setPage,
        setPageSize,
        setProblemTypes,
        resetFilters,
        fetchAllRowsForExport,
        refetch: () => fetchData(filters),
    };
}
