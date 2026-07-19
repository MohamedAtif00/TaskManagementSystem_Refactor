export const STATUS_COLORS = {
    Approved: "#4ade80",
    Hold: "#f59e0b",
    Rollback: "#dc5e5e",
} as const;

export const STATUS_OPTIONS = ["Approved", "Hold", "Rollback"] as const;

export const PRIORITY_OPTIONS = ["High", "Medium", "Low"] as const;

export const DEFAULT_PAGE_SIZE = 5;

/** Local calendar date as YYYY-MM-DD (avoid UTC shift from toISOString). */
const formatLocalDate = (date: Date) => {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, "0");
    const d = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
};

const buildDefaultFilters = (): DailyReportFilters => {
    const today = new Date();
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(today.getDate() - 7);

    return {
        from: formatLocalDate(sevenDaysAgo),
        to: formatLocalDate(today),
        team: "",
        semester: "",
        subject: "",
        grade: "",
        taskName: "",
        status: "",
        problemType: "",
        priority: "",
        page: 1,
        pageSize: DEFAULT_PAGE_SIZE,
    };
};

export const EMPTY_FILTERS: DailyReportFilters = buildDefaultFilters();

export const createEmptyFilters = () => buildDefaultFilters();
