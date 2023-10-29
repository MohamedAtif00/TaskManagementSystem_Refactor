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