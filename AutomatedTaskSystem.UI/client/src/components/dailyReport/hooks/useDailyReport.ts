import { useCallback, useEffect, useState } from "react";
import API from "../../../lib/API";
import { DEFAULT_PAGE_SIZE, EMPTY_FILTERS } from "../constants";

export function useDailyReport() {
    const [filters, setFilters] = useState<DailyReportFilters>({ ...EMPTY_FILTERS });
    const [rows, setRows] = useState<DailyReportRow[]>([]);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        page: 1,
        pageSize: DEFAULT_PAGE_SIZE,
        totalPages: 0,
    });
    const [summary, setSummary] = useState<DailyReportSummary | null>(null);
    const [charts, setCharts] = useState<DailyReportChartData | null>(null);
    const [lookups, setLookups] = useState<DailyReportLookups | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchData = useCallback(async (activeFilters: DailyReportFilters) => {
        setLoading(true);
        setError(null);

        const dashboardRes = await API.DAILY_REPORT.GET_DASHBOARD(activeFilters);

        if (!dashboardRes || dashboardRes.error || !dashboardRes.data) {
            setError("Failed to load daily report data");
            setLoading(false);
            return;
        }

        const { rows: paged, summary: summaryData, charts: chartsData, lookups: lookupsData } =
            dashboardRes.data;

        setRows(paged?.rows ?? []);
        setPagination({
            totalCount: paged?.totalCount ?? 0,
            page: paged?.page ?? 1,
            pageSize: paged?.pageSize ?? DEFAULT_PAGE_SIZE,
            totalPages: paged?.totalPages ?? 0,
        });
        setSummary(summaryData ?? null);
        setCharts(chartsData ?? null);
        setLookups(lookupsData ?? null);
        setLoading(false);
    }, []);

    useEffect(() => {
        fetchData(filters);
    }, [filters, fetchData]);

    const updateFilter = (key: keyof DailyReportFilters, value: string) => {
        setFilters((prev) => ({
            ...prev,
            [key]: value,
            page: key === "page" ? Number(value) || 1 : 1,
        }));
    };

    const setPage = (page: number) => {
        setFilters((prev) => ({ ...prev, page }));
    };

    const setPageSize = (pageSize: number) => {
        setFilters((prev) => ({ ...prev, pageSize, page: 1 }));
    };

    const resetFilters = () => setFilters({ ...EMPTY_FILTERS });

    const updateNotes = async (taskId: number, notes: string) => {
        const res = await API.DAILY_REPORT.PATCH_NOTES(taskId, notes);
        if (res && !res.error) {
            setRows((prev) =>
                prev.map((row) => (row.taskId === taskId ? { ...row, notes } : row))
            );
            return true;
        }
        return false;
    };

    const fetchAllRowsForExport = async () => {
        const res = await API.DAILY_REPORT.GET_ROWS({
            ...filters,
            page: 1,
            pageSize: 500,
        });
        if (res && !res.error && res.data) {
            return res.data.rows;
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
        updateFilter,
        setPage,
        setPageSize,
        resetFilters,
        updateNotes,
        fetchAllRowsForExport,
        refetch: () => fetchData(filters),
    };
}
