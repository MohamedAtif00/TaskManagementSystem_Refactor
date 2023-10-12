interface ITeam {
    id: number;
    name: string;
    members: number;
}

interface IGroup {
    id: number;
    name: string;
    colorCode: string;
    members: number;
    role: string;
    roleId: number;
}

interface IUser {
    id: number;
    name: string;
    group: { id: number; name: string };
    role: { id: number; name: string };
}

interface ISchema {
    description: string;
    id: number;
    name: string;
    type?: { id: number; name: string };
}

interface IStep {
    id: number;
    name: string;
    order: number;
    reviewable: boolean;
    tl: boolean;
    group: { name: string; id: number };
    duration: number;
    priority: TaskPriority;
    taskBankItemId: number;
}

interface INode {
    id: number;
    order: number;
    name: string;
    previous: { name: string; id: number }[];
    requires: { name: string; id: number }[];
    isStart: boolean;
    steps: IStep[];
}

interface IProject {
    id: number;
    description: string;
    name: string;
    year: { id: number; name: string };
    term: boolean;
    status: ProjectStatus;
}

interface ISection {
    id: number;
    name: string;
    head: { id: number; name: string };
    groups: { id: number; name: string }[];
}

interface LearningObjective {
    id: number;
    name: string;
    tag: string;
    environment: string;
    template: string;
    schema: { name: string; id: number };
}

interface Lesson {
    id: number;
    name: string;
    learningObjectives: LearningObjective[];
}

interface Unit {
    id: number;
    name: string;
    lessons: Lesson[];
}

interface ProjectDetails {
    id: number;
    name: string;
    description: string;
    units: Unit[];
    status: number;
}

type CommentInfo = {
    id: number;
    content: string;
    timestamp: string;
    user: string;
    userId: number;
};

type CommentInfo = {
    id: number;
    content: string;
    timestamp: string;
    user: string;
    userId: number;
};

type TaskInfo = {
    id: number;
    name: string;
    status: 0 | 1 | 2 | 3 | 4;
    TL: boolean;
    user?: { id: number; name: string };
    learningObjective: { id: number; name: string };
    isReview: boolean;
    flagged: boolean;
    attention: boolean;
    comments: CommentInfo[];
    isRollback: boolean;
    rollbackCount: number;
    from: string;
    priority: 0 | 1 | 2 | 3;
    paused: boolean;
};

type TaskBankType = 0 | 1;
type TaskActivityType =
    | 0
    | 1
    | 2
    | 3
    | 4
    | 5
    | 6
    | 7
    | 8
    | 9
    | 10
    | 11
    | 12
    | 13
    | 14
    | 15
    | 16
    | 17
    | 18
    | 19
    | 20;
type TaskPriority = 0 | 1 | 2 | 3;
type TaskStatus = 0 | 1 | 2 | 3 | 4;
type UserRole = 0 | 1 | 2 | 3;
type ProjectStatus = 0 | 1 | 2 | 3;
