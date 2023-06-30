interface LearningObjectiveReport {
	id: number;
	name: string;
	started: string;
	done: string;
}

interface LessonReport {
	id: number;
	name: string;
	runningLearningObjectives: number;
	doneLearningObjectives: number;
	learningObjectives: LearningObjectiveReport[];
}

interface UnitReport {
	id: number;
	name: string;
	runningLearningObjectives: number;
	doneLearningObjectives: number;
	lessons: LessonReport[];
}

interface DetailedReport {
	id: number;
	name: string;
	description: string;
	term: string;
	year: string;
	runningLearningObjectives: number;
	doneLearningObjectives: number;
	units: UnitReport[];
}

interface Report {
	id: number;
	name: string;
	description: string;
	term: string;
	year: string;
	runningLearningObjectives: number;
	doneLearningObjectives: number;
}

interface ResponseService {
	error: boolean;
	message: string;
}

interface ResponseService<T> extends ResponseService {
	data: T;
}
