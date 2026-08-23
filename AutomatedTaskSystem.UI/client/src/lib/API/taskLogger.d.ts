type TaskLoggerStatus = "Approved" | "Hold" | "Rollback";
type TaskLoggerRankingRange = "all" | "week" | "month";

interface TaskLoggerFilters {
    from?: string;
    to?: string;
    search?: string;
    member?: string;
    subject?: string;
    status?: TaskLoggerStatus | "";
    taskName?: string;
    rankingRange?: TaskLoggerRankingRange;
    page?: number;
    pageSize?: number;
}

interface TaskLoggerRow {
    taskId: number;
    date: string;
    member: string;
    loCode: string;
    subject: string;
    taskName: string;
    actualMinutes: number;
    expectedMinutes: number;
    points: number;
    status: TaskLoggerStatus;
    notes: string;
}

interface TaskLoggerPagedResult {
    rows: TaskLoggerRow[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

interface TaskLoggerSummary {
    totalTasks: number;
    totalTimeMinutes: number;
    rollbackTasks: number;
    totalPoints: number;
}

interface TaskLoggerMemberRank {
    member: string;
    points: number;
    rank: number;
}

interface TaskLoggerLookups {
    members: string[];
    subjects: string[];
    taskNames: string[];
    statuses: string[];
}

interface TaskLoggerDashboard {
    rows: TaskLoggerPagedResult;
    summary: TaskLoggerSummary;
    rankings: TaskLoggerMemberRank[];
    lookups: TaskLoggerLookups;
}
