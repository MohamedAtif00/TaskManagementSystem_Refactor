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
}

interface IStep {
	id: number;
	name: string;
	order: number;
	reviewable: boolean;
	tl: boolean;
	group: { name: string; id: number };
	duration: number;
}

interface INode {
	id: number;
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
	lesson: string;
	project: string;
	projectDescription: string;
	projectId: number;
	unit: string;
	name: string;
	status: "Done" | "Doing" | "To Do" | "Backlog" | "Rollback";
	step?: string;
	stepId: number;
	TL: boolean;
	group: string;
	groupId: number;
	user: string;
	userId: string;
	learningObjective: string;
	learningObjectiveId: number;
	isReview: boolean;
	flagged: boolean;
	attention: boolean;
	tag: string;
	environment: string;
	template: string;
	comments: CommentInfo[];
	isRollback: boolean;
	rollbackCount: number;
	from: string;
};
