type DailyReportStatus = "Approved" | "Hold" | "Rollback";

interface DailyReportFilters {
    from?: string;
    to?: string;
    team?: string;
    semester?: string;
    subject?: string;
    grade?: string;
    taskName?: string;
    status?: DailyReportStatus | "";
    problemType?: string;
    priority?: string;
    page?: number;
    pageSize?: number;
}

interface DailyReportPagedResult {
    rows: DailyReportRow[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

interface DailyReportDashboard {
    rows: DailyReportPagedResult;
    summary: DailyReportSummary;
    charts: DailyReportChartData;
    lookups: DailyReportLookups;
}

interface DailyReportRow {
    taskId: number;
    date: string;
    team: string;
    semester: string;
    subjects: string;
    grade: string;
    taskName: string;
    loCode: string;
    loType: string;
    assignedTo: string;
    status: DailyReportStatus;
    problemType: string;
    priority: string;
    notes: string;
}

interface DailyReportTeamCount {
    team: string;
    count: number;
}

interface DailyReportSummary {
    total: number;
    approved: number;
    hold: number;
    rollback: number;
    activeTeams: number;
    topTeams: DailyReportTeamCount[];
}

interface DailyReportProblemCount {
    problemType: string;
    count: number;
}

interface DailyReportChartData {
    approved: number;
    hold: number;
    rollback: number;
    problemTypes: DailyReportProblemCount[];
}

interface DailyReportLookups {
    teams: string[];
    semesters: string[];
    subjects: string[];
    grades: string[];
    taskNames: string[];
    problemTypes: string[];
    priorities: string[];
}
