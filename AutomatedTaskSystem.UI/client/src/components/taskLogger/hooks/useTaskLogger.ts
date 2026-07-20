import { useCallback, useEffect, useState } from "react";
import API from "../../../lib/API";
import { DEFAULT_PAGE_SIZE, createEmptyFilters } from "../constants";

const emptySummary = (): TaskLoggerSummary => ({
    totalTasks: 0,
    totalTimeMinutes: 0,
    rollbackTasks: 0,
    totalPoints: 0,
});

const emptyLookups = (): TaskLoggerLookups => ({
    members: [],
    subjects: [],
    taskNames: [],
    statuses: ["Approved", "Hold", "Rollback"],
});

function unwrapResponse<T>(raw: any): { error: boolean; message: string; data: T | null } {
    if (!raw || typeof raw !== "object") {
        return { error: true, message: "Empty response", data: null };
    }
    return {
        error: Boolean(raw.error ?? raw.Error),
        message: String(raw.message ?? raw.Message ?? ""),
        data: (raw.data ?? raw.Data ?? null) as T | null,
    };
}

function normalizeDashboard(raw: any): TaskLoggerDashboard | null {
    if (!raw || typeof raw !== "object") return null;
    const paged = raw.rows ?? raw.Rows ?? {};
    const summary = raw.summary ?? raw.Summary ?? {};
    const rankings = raw.rankings ?? raw.Rankings ?? [];
    const lookups = raw.lookups ?? raw.Lookups ?? {};

    const mapRow = (r: any): TaskLoggerRow => ({
        taskId: r.taskId ?? r.TaskId ?? 0,
        date: r.date ?? r.Date ?? "",
        member: r.member ?? r.Member ?? "",
        loCode: r.loCode ?? r.LoCode ?? "",
        subject: r.subject ?? r.Subject ?? "",
        taskName: r.taskName ?? r.TaskName ?? "",
        actualMinutes: r.actualMinutes ?? r.ActualMinutes ?? 0,
        expectedMinutes: r.expectedMinutes ?? r.ExpectedMinutes ?? 0,
        points: r.points ?? r.Points ?? 0,
        status: r.status ?? r.Status ?? "Hold",
        notes: r.notes ?? r.Notes ?? "",
    });

    return {
        rows: {
            rows: (paged.rows ?? paged.Rows ?? []).map(mapRow),
            totalCount: paged.totalCount ?? paged.TotalCount ?? 0,
            page: paged.page ?? paged.Page ?? 1,
            pageSize: paged.pageSize ?? paged.PageSize ?? DEFAULT_PAGE_SIZE,
            totalPages: paged.totalPages ?? paged.TotalPages ?? 0,
        },
        summary: {
            totalTasks: summary.totalTasks ?? summary.TotalTasks ?? 0,
            totalTimeMinutes: summary.totalTimeMinutes ?? summary.TotalTimeMinutes ?? 0,
            rollbackTasks: summary.rollbackTasks ?? summary.RollbackTasks ?? 0,
            totalPoints: summary.totalPoints ?? summary.TotalPoints ?? 0,
        },
        rankings: (rankings as any[]).map((r, i) => ({
            member: r.member ?? r.Member ?? "",
            points: r.points ?? r.Points ?? 0,
            rank: r.rank ?? r.Rank ?? i + 1,
        })),
        lookups: {
            members: lookups.members ?? lookups.Members ?? [],
            subjects: lookups.subjects ?? lookups.Subjects ?? [],
            taskNames: lookups.taskNames ?? lookups.TaskNames ?? [],
            statuses: lookups.statuses ?? lookups.Statuses ?? ["Approved", "Hold", "Rollback"],
        },
    };
}

export function useTaskLogger() {
    const [filters, setFilters] = useState<TaskLoggerFilters>(() => createEmptyFilters());
    const [rows, setRows] = useState<TaskLoggerRow[]>([]);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        page: 1,
        pageSize: DEFAULT_PAGE_SIZE,
        totalPages: 0,
    });
    const [summary, setSummary] = useState<TaskLoggerSummary>(emptySummary);
    const [rankings, setRankings] = useState<TaskLoggerMemberRank[]>([]);
    const [lookups, setLookups] = useState<TaskLoggerLookups>(emptyLookups);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasLoadedOnce, setHasLoadedOnce] = useState(false);
    const [dupMode, setDupMode] = useState(false);
    const [dupOnly, setDupOnly] = useState(false);
    const [dupRows, setDupRows] = useState<TaskLoggerRow[] | null>(null);

    const fetchData = useCallback(async (activeFilters: TaskLoggerFilters) => {
        setLoading(true);
        setError(null);
        try {
            if (!API.TASK_LOGGER?.GET_DASHBOARD) {
                setError("Task Logger API is not available. Restart the Next.js dev server.");
                return;
            }
            const raw = await API.TASK_LOGGER.GET_DASHBOARD(activeFilters);
            const { error: isError, message, data } = unwrapResponse<any>(raw);
            if (isError || !data) {
                setError(message || "Failed to load task logger data.");
                return;
            }
            const dashboard = normalizeDashboard(data);
            if (!dashboard) {
                setError("Task logger response was empty or invalid.");
                return;
            }
            setRows(dashboard.rows.rows);
            setPagination({
                totalCount: dashboard.rows.totalCount,
                page: dashboard.rows.page,
                pageSize: dashboard.rows.pageSize,
                totalPages: dashboard.rows.totalPages,
            });
            setSummary(dashboard.summary);
            setRankings(dashboard.rankings);
            setLookups(dashboard.lookups);
            setDupRows(null);
            setDupMode(false);
            setDupOnly(false);
        } catch (err) {
            console.error(err);
            setError("Task logger request failed. Is the API running?");
        } finally {
            setHasLoadedOnce(true);
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        void fetchData(filters);
    }, [filters, fetchData]);

    const updateFilter = (key: keyof TaskLoggerFilters, value: string) => {
        setFilters((prev) => ({
            ...prev,
            [key]: value,
            page: key === "page" || key === "rankingRange" ? prev.page : 1,
            ...(key === "rankingRange" ? { rankingRange: value as TaskLoggerRankingRange } : {}),
        }));
    };

    const setPage = (page: number) => setFilters((prev) => ({ ...prev, page }));
    const setPageSize = (pageSize: number) => setFilters((prev) => ({ ...prev, pageSize, page: 1 }));
    const setRankingRange = (rankingRange: TaskLoggerRankingRange) =>
        setFilters((prev) => ({ ...prev, rankingRange }));
    const resetFilters = () => setFilters(createEmptyFilters());

    const fetchAllRowsForExport = async () => {
        const raw = await API.TASK_LOGGER.GET_ROWS({
            ...filters,
            page: 1,
            pageSize: 500,
        });
        const { error: isError, data } = unwrapResponse<any>(raw);
        if (!isError && data) {
            const list = data.rows ?? data.Rows ?? [];
            return list.map((r: any) => ({
                taskId: r.taskId ?? r.TaskId ?? 0,
                date: r.date ?? r.Date ?? "",
                member: r.member ?? r.Member ?? "",
                loCode: r.loCode ?? r.LoCode ?? "",
                subject: r.subject ?? r.Subject ?? "",
                taskName: r.taskName ?? r.TaskName ?? "",
                actualMinutes: r.actualMinutes ?? r.ActualMinutes ?? 0,
                expectedMinutes: r.expectedMinutes ?? r.ExpectedMinutes ?? 0,
                points: r.points ?? r.Points ?? 0,
                status: r.status ?? r.Status ?? "Hold",
                notes: r.notes ?? r.Notes ?? "",
            })) as TaskLoggerRow[];
        }
        return rows;
    };

    const toggleDetectDuplicates = async () => {
        if (dupMode) {
            setDupMode(false);
            setDupOnly(false);
            setDupRows(null);
            return;
        }
        const loaded = await fetchAllRowsForExport();
        setDupRows(loaded);
        setDupMode(true);
    };

    const toggleShowDuplicatesOnly = () => {
        if (!dupMode) return;
        setDupOnly((v) => !v);
    };

    const displayRows = (() => {
        const source = dupRows ?? rows;
        if (!dupMode || !dupOnly) return dupMode && dupRows ? dupRows : rows;

        const counts = new Map<string, number>();
        source.forEach((r) => {
            const key = (r.loCode || "").toLowerCase();
            if (!key) return;
            counts.set(key, (counts.get(key) || 0) + 1);
        });
        const dups = new Set([...counts.entries()].filter(([, c]) => c > 1).map(([k]) => k));
        return source
            .filter((r) => dups.has((r.loCode || "").toLowerCase()))
            .sort((a, b) => a.loCode.toLowerCase().localeCompare(b.loCode.toLowerCase()));
    })();

    const duplicateColorMap = (() => {
        const map = new Map<number, string>();
        if (!dupMode) return map;
        const source = dupRows ?? rows;
        const groups = new Map<string, number[]>();
        source.forEach((r) => {
            const key = (r.loCode || "").toLowerCase();
            if (!key) return;
            if (!groups.has(key)) groups.set(key, []);
            groups.get(key)!.push(r.taskId);
        });
        const colors = ["bg-yellow-100", "bg-lime-100", "bg-orange-100", "bg-blue-100", "bg-purple-100"];
        let i = 0;
        groups.forEach((ids) => {
            if (ids.length > 1) {
                const color = colors[i % colors.length];
                ids.forEach((id) => map.set(id, color));
                i++;
            }
        });
        return map;
    })();

    return {
        filters,
        rows: displayRows,
        pagination,
        summary,
        rankings,
        lookups,
        loading,
        error,
        hasLoadedOnce,
        dupMode,
        dupOnly,
        duplicateColorMap,
        updateFilter,
        setPage,
        setPageSize,
        setRankingRange,
        resetFilters,
        fetchAllRowsForExport,
        toggleDetectDuplicates,
        toggleShowDuplicatesOnly,
        refetch: () => fetchData(filters),
    };
}
