interface IDName {
    id: number;
    name: string;
}

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

interface ICreateUser{
    archived: boolean;
    code: string;
    onBoard: boolean;
    name: string;
    group?: {
        id: number;
        name: string;
    };
    accountType: AccountType;
    annual_leave_MAX: number;
    annual_leave: number;
    sick_leave: number;
    emergency_leave_MAX: number;
    emergency_leave: number;
    permission_MAX: number;
    permission: number;
    hrCode: string;
    email?: string | null;
    teamleader?: IUser | null;
    teamleaderId?: number | null;
    groupId?: number;
    role: UserRole;
    vacation?: IVacation;
 }

 interface IGetUser{
    id: number;
 }

//  interface IUser {
//     id: number;
//     archived: boolean;
//     code: string;
//     onBoard: boolean;
//     name: string;
//     group?: {
//         id: number;
//         name: string;
//     };
//     accountType: AccountType;
//     annual_leave_MAX: number;
//     annual_leave: number;
//     sick_leave: number;
//     emergency_leave_MAX: number;
//     emergency_leave: number;
//     permission_MAX: number;
//     permission: number;
//     hrCode: string;
//     email?: string | null;
//     teamleader?: IUser | null;
//     teamleaderId?: number | null;
//     groupId?: number;
//     role: UserRole;
//     vacation?: IVacation;
// }

type TaskType = "sprints"|"tasks"|"task-sprint";

interface IVacation{
    annual:number;
    sick:number;    
    emergency:number;
    annual_MAX:number,
    emergency_MAX:number
}

interface IGetVacation{
    annual_leave_MAX: number;
    annual_leave: number;
    sick_leave: number;
    emergency_leave_MAX: number;
    emergency_leave: number;
}

interface IGetVacation{
    id:number,
    startDate:string,
    endDate:string,
    reason:string,
    status:LeaveRequestStatus
    type:LeaveRequestType
    dateCreated:string
}



// Define interface for user changes data
interface IUserChange {
    id: number;
    userId: number;
    changedByUserId: number;
    changedByUserName: string;
    action: string;
    changes: string;
    changedAt: string;
}

interface IUser {
    id: number;
    archived: boolean;
    code: string;
    onBoard: boolean;
    name: string;
    group?: {
        id: number;
        name: string;
    };
    accountType: AccountType;
    annual_leave_MAX: number;
    annual_leave: number;
    sick_leave: number;
    emergency_leave_MAX: number;
    emergency_leave: number;
    permission_MAX: number;
    permission: number;
    workFromHome:number;
    workFromHome_MAX:number;
    hrCode: string;
    email?: string | null;
    teamleader?: IUser | null;
    teamleaderId?: number | null;
    groupId?: number;
    role: UserRole;
    vacation?: IVacation;
    phone:string;
    title:string;
    isArchived:boolean
}

interface IVacation{
    annual:number;
    sick:number;    
    emergency:number;
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
    isStart: boolean;
    steps: IStep[];
}

interface IProject {
    id: number;
    description: string;
    name: string;
    rootProjectId: number;
    projectYearId: number;
    termId: number;
    rootProject: IDName;
    projectYear: IDName;
    term: IDName;
    status: ProjectStatus;
    count?: number;
    progressPercent?: number;
}


interface ISprint{
    id:number,
    name:string,
    description:string,
    startDate:string,
    endDate:string,
    isArchived:boolean,
    loNumber:number,
    completePercintag:number,
    learningObjects:IDName[]
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
    duration:number;
    baseDuration:number;
    priority: 0 | 1 | 2 | 3;
    paused: boolean;
    // Date fields for export filtering
    createdAt: string;
    startedAt: string | null;
    doneAt: string | null;
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
type UserRole = 0 | 1 | 2 | 3| 4;
type ProjectStatus = 0 | 1 | 2 | 3;

interface BaseOpinion {
  comment?: string;
  isApproved: boolean;
  user: { id: number; name: string; role: UserRole };
}

interface ProjectSheet {
    id: number;
    name: string;
    units: UnitChip[];
    workableTasks: BasicInfo[];
}

interface UnitChip {
    id: number;
    name: string;
    lessons: LessonChip[];
}

interface LessonChip {
    id: number;
    name: string;
    los: LoChip[]
}

interface LoChip {
    id: number;
    name: string;
    tag: string;
    environment: string;
    template: string;
    schema: BasicInfo;
    tasks: TaskChip[];
}

interface TaskChip {
    id: number;
    name: string;
    user: null | BasicInfo
    group: BasicInfo;
    status: 0 | 1 | 2 | 3 | 4;
    rollbackCounts: 0;
    isRollback: boolean;
    paused: false;
}
