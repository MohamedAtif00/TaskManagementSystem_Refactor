interface BasicInfo {
    id: number;
    name: string;
}

interface NodeAhead {
	id: number;
	name: string;
	nextNodes: BasicInfo[];
	previousNodes: BasicInfo[];
	steps: StepAhead[];
	isComplete: boolean;
	order: number;
}

interface StepAhead {
	id: number;
	name: string;
	group: BasicInfo;
	isComplete: boolean;
}
