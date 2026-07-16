export const STATUS_COLORS = {
    Approved: "#4ade80",
    Hold: "#f59e0b",
    Rollback: "#dc5e5e",
} as const;

export const STATUS_OPTIONS = ["Approved", "Hold", "Rollback"] as const;

export const PRIORITY_OPTIONS = ["High", "Medium", "Low"] as const;

export const DEFAULT_PAGE_SIZE = 5;

const formatDate = (date: Date) => date.toISOString().slice(0, 10);

const today = new Date();
const sevenDaysAgo = new Date();
sevenDaysAgo.setDate(today.getDate() - 7);

export const EMPTY_FILTERS: DailyReportFilters = {
    from: formatDate(sevenDaysAgo),
    to: formatDate(today),
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
