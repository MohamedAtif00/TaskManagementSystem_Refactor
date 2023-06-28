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
