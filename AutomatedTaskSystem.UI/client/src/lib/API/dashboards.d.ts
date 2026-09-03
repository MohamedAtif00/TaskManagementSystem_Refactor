interface SubjectOverviewItem {
    id: number;
    name: string;
    activeTasks: number;
}

interface ProjectManagerDashboard {
    projects: number;
    sprints: number;
    learningObjectives: number;
    users: number;
    activeTasks: number;
    teamsWorkload: ProjectManagerTeamWorkload[];
    learningObjectivesOverview: {
        completed: number;
        uncompleted: number;
        total: number;
    };
    subjectsOverview: SubjectOverviewItem[];
    tasksOverview: {
        toDo: number;
        doing: number;
        rollback: number;
        flagged: number;
        done: number;
        total: number;
    };
    projectsTable: ProjectManagerProjectRow[];
    flaggedRollbackTasks: ProjectManagerFlaggedRollback[];
    sprintsTable: ProjectManagerSprintRow[];
    activityLog: ProjectManagerActivity[];
}

interface ProjectManagerTeamWorkload {
    id: number;
    name: string;
    taskCount: number;
    workloadPercent: number;
}

interface ProjectManagerProjectRow {
    id: number;
    name: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface ProjectManagerSprintRow {
    id: number;
    name: string;
    projectName: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface ProjectManagerFlaggedRollback {
    taskId: number;
    projectId: number;
    userName: string;
    taskName: string;
    type: "flagged" | "rollback";
    timestamp: string;
}

interface ProjectManagerActivity {
    id: number;
    userName: string;
    initials: string;
    message: string;
    createdAt: string;
}

interface TeamLeaderDashboard {
    projects: number;
    sprints: number;
    learningObjectives: number;
    users: number;
    teamPerformance: number;
    activeTasks: number;
    membersWorkload: TeamLeaderMemberWorkload[];
    learningObjectivesOverview: {
        completed: number;
        uncompleted: number;
        total: number;
    };
    subjectsOverview: SubjectOverviewItem[];
    tasksOverview: {
        toDo: number;
        doing: number;
        rollback: number;
        flagged: number;
        done: number;
        total: number;
    };
    projectsTable: TeamLeaderProjectRow[];
    flaggedRollbackTasks: TeamLeaderFlaggedRollback[];
    sprintsTable: TeamLeaderSprintRow[];
    activityLog: TeamLeaderActivity[];
}

interface TeamLeaderMemberWorkload {
    id: number;
    name: string;
    taskCount: number;
    workloadPercent: number;
}

interface TeamLeaderProjectRow {
    id: number;
    name: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface TeamLeaderSprintRow {
    id: number;
    name: string;
    projectName: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface TeamLeaderFlaggedRollback {
    taskId: number;
    projectId: number;
    userName: string;
    taskName: string;
    type: "flagged" | "rollback";
    timestamp: string;
}

interface TeamLeaderActivity {
    id: number;
    userName: string;
    initials: string;
    message: string;
    createdAt: string;
}

interface SectionHeadDashboard {
    projects: number;
    sprints: number;
    learningObjectives: number;
    users: number;
    teams: number;
    activeTasks: number;
    teamsWorkload: SectionHeadTeamWorkload[];
    learningObjectivesOverview: {
        completed: number;
        uncompleted: number;
        total: number;
    };
    subjectsOverview: SubjectOverviewItem[];
    tasksOverview: {
        toDo: number;
        doing: number;
        rollback: number;
        flagged: number;
        done: number;
        total: number;
    };
    projectsTable: SectionHeadProjectRow[];
    flaggedRollbackTasks: SectionHeadFlaggedRollback[];
    sprintsTable: SectionHeadSprintRow[];
    activityLog: SectionHeadActivity[];
}

interface SectionHeadTeamWorkload {
    id: number;
    name: string;
    taskCount: number;
    workloadPercent: number;
}

interface SectionHeadProjectRow {
    id: number;
    name: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface SectionHeadSprintRow {
    id: number;
    name: string;
    projectName: string;
    year: string;
    status: "on_track" | "completed" | "at_risk";
    progressPercent: number;
    deadline: string;
}

interface SectionHeadFlaggedRollback {
    taskId: number;
    projectId: number;
    userName: string;
    taskName: string;
    type: "flagged" | "rollback";
    timestamp: string;
}

interface SectionHeadActivity {
    id: number;
    userName: string;
    initials: string;
    message: string;
    createdAt: string;
}

interface MemberDashboard {
    projects: number;
    activeProjects: number;
    sprints: number;
    learningObjectives: number;
    learningObjectivesThisMonth: number;
    teamPerformance: number;
    activeTasks: number;
    learningObjectivesOverview: {
        completed: number;
        uncompleted: number;
        total: number;
    };
    subjectsOverview: SubjectOverviewItem[];
    tasksOverview: {
        toDo: number;
        doing: number;
        rollback: number;
        flagged: number;
        done: number;
        total: number;
    };
    toDoTasks: MemberTaskBoardItem[];
    inProgressTasks: MemberTaskBoardItem[];
    doneTasks: MemberTaskBoardItem[];
    sprintDeadlines: MemberSprintDeadline[];
    escalatedTasks: MemberEscalatedTask[];
    workUpdates: MemberWorkUpdate[];
}

interface MemberTaskBoardItem {
    id: number;
    name: string;
    projectName: string;
    projectId: number;
    priority: number;
    dueDate: string | null;
    isDueToday: boolean;
    isCompleted: boolean;
}

interface MemberSprintDeadline {
    sprintId: number;
    sprintName: string;
    taskName: string;
    daysLeft: number;
    progressPercent: number;
    deadlineDate: string;
    urgency: "critical" | "warning" | "normal";
}

interface MemberEscalatedTask {
    taskId: number;
    taskName: string;
    escalatedBy: string;
    escalatedAt: string;
    projectId: number;
}

interface MemberWorkUpdate {
    id: number;
    authorName: string;
    projectName: string;
    message: string;
    createdAt: string;
}
