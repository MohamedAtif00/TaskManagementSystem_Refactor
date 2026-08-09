export const STATUS_COLORS = {
    Approved: "#4ade80",
    Hold: "#f59e0b",
    Rollback: "#dc5e5e",
    "Red Flag": "#ef4444",
} as const;

export const STATUS_OPTIONS = ["Approved", "Hold", "Rollback", "Red Flag"] as const;

export const DEFAULT_PAGE_SIZE = 5;

const formatLocalDate = (date: Date) => {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, "0");
    const d = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
};

export const createEmptyFilters = (): TaskLoggerFilters => {
    const today = new Date();
    const oneMonthAgo = new Date();
    oneMonthAgo.setMonth(today.getMonth() - 1);

    return {
        from: formatLocalDate(oneMonthAgo),
        to: formatLocalDate(today),
        search: "",
        member: "",
        subject: "",
        status: "",
        taskName: "",
        rankingRange: "all",
        page: 1,
        pageSize: DEFAULT_PAGE_SIZE,
    };
};
