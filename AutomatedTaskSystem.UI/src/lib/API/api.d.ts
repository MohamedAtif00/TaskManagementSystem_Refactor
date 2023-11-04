interface NodePoint {
	id: number;
	name: string;
	nextNodes: BasicInfo[];
	previousNodes: BasicInfo[];
	steps: StepPoint[];
	order: number;
}

interface StepPoint {
	id: number;
	name: string;
	group: BasicInfo;
}

interface UserTaskCount {
	id: number;
	name: string;
	group: BasicInfo;
	tasks: {
		todo: number;
		doing: number;
	}
}

interface UserTask {
	id: number;
	name: string;
	learningObjective: BasicInfo;
	projectId: number;
}

interface UserTaskInfo {
	id: number;
	name: string;
	group: BasicInfo;
	backlogCount: number;
	todoTasks: UserTask[];
	doingTasks: UserTask[];
}