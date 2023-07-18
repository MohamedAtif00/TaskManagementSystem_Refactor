interface ProjectManagerDashboard {
    projectsReport: {
        id: number;
        name: string;
        description: string;
        term: string;
        year: string;
        idleLearningObjectives: number;
        runningLearningObjectives: number;
        doneLearningObjectives: number;
        totalLearningObjectives: number;
    }[];
    groupsCount: {
        id: number;
        name: string;
        usersCount: number;
    }[];
    numberOfProject: number;
    numberOfUsers: number;
    numberOfSchemas: number;
    numberOfActiveTasks: number;
}

interface TeamLeaderDashboard {
    members: number;
    activeTasks: number;
    projects: number;
    projectsDetails: {
        id: number;
        name: string;
        tasksCount: number;
    }[];
    tasksPerUser: {
        id: number;
        name: string;
        tasksCount: number;
    }[];
}
