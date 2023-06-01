import { ITask } from "../../components/taskDetails";
import authService from "../Auth";
import { CommonResponse, ErrorResponse, url } from "./";

const TASKS = {
	UPDATE_PRIORITY: async (id: number, priority: number | null) => {
		try {
			const res = await fetch(`${url}/tasks/${id}/priority`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ priority }),
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (err) {
			console.error(err);
			return false;
		}
	},
	ADD_TASK: async (params: {
		userId: number;
		TaskBankItemId: number;
		learningObjectiveId: number;
	}) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks`, {
				method: "POST",
				headers: {
					...authHeader,
					"Content-Type": "application/json",
				},
				body: JSON.stringify(params),
			});
			const data = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	COMMENT: async (id: number, content: string) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${id}/comment`, {
				method: "POST",
				headers: {
					...authHeader,
					"Content-Type": "application/json",
				},
				body: JSON.stringify({
					comment: content,
				}),
			});
			const data: {
				user: {
					id: number;
					name: string;
				};
				id: number;
				content: string;
				timestamp: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	PROJECTS: async () => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/projects/assignment`, {
				headers: { ...authHeader },
			});
			const data: {
				data: IProject[];
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ONE: async (taskId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/${taskId}`, {
				headers: {
					...auth,
				},
			});
			const data: ITask = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL: async (projectId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/projects/${projectId}`, {
				headers: {
					...auth,
				},
			});
			const data: TaskInfo[] = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	ADD_TODO: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/todo/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	DOING: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/doing/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	APPROVE: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/approve/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	COMPLETE: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/done/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	ROLLBACK: async ({
		currentTask,
		stepId,
	}: {
		currentTask: string | string[];
		stepId: number;
	}) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/rollback/${currentTask}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
				body: JSON.stringify({ taskId: currentTask, stepId }),
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	FLAG_TASK: async (taskId: number) => {
		try {
			const res = await fetch(`${url}/tasks/flag/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	UNPAUSE: async (taskId: number) => {
		try {
			const res = await fetch(`${url}/tasks/${taskId}/unpause`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	PAUSE: async (taskId: number) => {
		try {
			const res = await fetch(`${url}/tasks/${taskId}/pause`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	UNFLAG_TASK: async (taskId: number) => {
		try {
			const res = await fetch(`${url}/tasks/unflag/${taskId}`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
			});
			const data: ITask | ErrorResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	PREVIOUS_TASKS: async (taskId: string) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/previous/${taskId}`, {
				headers: {
					...auth,
				},
			});
			const data: { id: number; name: string }[] = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	TASK_ASSIGNMENT: async (taskId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/${taskId}/assigned`);
			const data: {
				assignedUser?: { id: number; name: string };
				assignableUsers: { id: number; name: string }[];
			} = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
	ASSIGN_TO_TASK: async ({
		taskId,
		userId,
	}: {
		taskId: string | string[];
		userId: number;
	}) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/${taskId}/assign`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
				body: JSON.stringify({ userId }),
			});
			const data: CommonResponse = await res.json();
			return data;
		} catch (error) {
			console.log(error);
			return false;
		}
	},
};

export default TASKS;
