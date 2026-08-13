

export interface GetAllSprintsResponse{
    id:number,
    name:string,
    description:string,
    startDate:string,
    endDate:string,
    isArchived:boolean,
    loNumber:number,
    completePercintag:number,
    projectNames?: string[],
    scopeFolderPath?: string,
    learningObjects:IDName[],
}

interface IDName {
    id: number;
    name: string;
}

// Sprint Analytics Types
export interface TagData {
    groupId: number;
    label: string;
    value: number;
    color: string;
    isFilled?: boolean;
}

export interface TaskSummary {
    active: number;
    completed: number;
    rollback: number;
    flagged: number;
    notStarted: number;
    total: number;
}

export interface LearningObjectiveSummary {
    completed: number;
    notStarted: number;
    inProcess:number;
    total: number;

}

export interface SprintOverviewData {
    tags: TagData[];
    taskSummary: TaskSummary;
    loSummary: LearningObjectiveSummary;
    sprintSummary: TaskSummary;
}

export interface LearningObjectiveProgress {
    id: number;
    name: string;
    value: number;
    status: 'On Track' | 'At Risk' | 'Delayed';
    currentPhases: CurrentPhase[];
}

export interface LearningObjectivesProgressData {
    data: LearningObjectiveProgress[];
}

// Learning Objectives Table Types
export interface CurrentPhase {
    groupName: string;
    colorCode: string;
}

export interface LearningObjectiveTableRow {
    id: number;
    name: string;
    subject: string;
    startDate: string;
    endDate: string;
    activeTasks: number;
    currentPhases: CurrentPhase[];
    status: 'On Track' | 'At Risk' | 'Delayed';
    progress: number;
}

export interface LearningObjectivesTableData {
    sprintName:string
    data: LearningObjectiveTableRow[];
}

