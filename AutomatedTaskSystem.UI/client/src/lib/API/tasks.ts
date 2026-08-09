import { IComment, ITask } from "../../components/taskDetails";
import authService from "../Auth";
import { BasicInfo, CommonResponse, url } from "./";

const TASKS = {
	GET_CREATABLE_TASKS: async (projectId: string | string[] | number) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/creatables/${projectId}`, {
				headers: {
					...authHeader,
				},
			});
			const data: ResponseService<{
				learningObjectives: BasicInfo[];
				assignees: {
					id: number;
					name: string;
					group: BasicInfo;
				}[];
				options: {
					id: number;
					name: string;
					group: BasicInfo;
					teamLead: boolean;
				}[];
			}> = await res.json();
			return data;
		} catch (err) {
			console.error(err);
			return false;
		}
	},
	UPDATE_PRIORITY: async (id: number, priority: TaskPriority) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${id}/priority`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
					...authHeader,
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
	ADD_COMMENT: async (id: number, content: string) => {
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
			const data: ResponseService<IComment> = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	EDIT_COMMENT: async (
		taskId: number,
		commentId: number,
		content: string
	) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${taskId}/comment`, {
				method: "PATCH",
				headers: {
					...authHeader,
					"Content-Type": "application/json",
				},
				body: JSON.stringify({
					comment: content,
					commentId,
				}),
			});
			const data: ResponseService<IComment> = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	DELETE_COMMENT: async (taskId: number, commentId: number) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${taskId}/comment`, {
				method: "DELETE",
				headers: {
					...authHeader,
					"Content-Type": "application/json",
				},
				body: JSON.stringify({
					commentId,
				}),
			});
			const data: ResponseService<IComment> = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	PROJECTS: async () => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/subjects/assignment`, {
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
			const data: {
				error: boolean;
				message: string;
				data: ITask;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ONE_FOR_SPRINT: async (sprintId:string|string[],taskId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/sprint/${sprintId}/task/${taskId}`, {
				headers: {
					...auth,
				},
			});
			const data: {
				error: boolean;
				message: string;
				data: ITask;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL_CARDS: async (projectId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/subjects/${projectId}/tasks/cards`, {
				headers: {
					...auth,
				},
			});
			const data: {
				data: TaskInfo[];
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_PROJECT_SHEET: async (projectId: string | string[]) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/subjects/${projectId}/tasks/sheet`, {
				headers: {
					...auth,
				},
			});
			const data: {
				data: ProjectSheet;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	PROCEED: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/${taskId}/proceed`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: {
				error: boolean;
				message: string;
				data: ITask;
			} = await res.json();
			return data;
		} catch (error) {
			// console.log(error);
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
			const res = await fetch(`${url}/tasks/${taskId}/complete`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
					...auth,
				},
			});
			const data: {
				error: boolean;
				message: string;
				data: ITask;
			} = await res.json();
			return data;
		} catch (error) {
			// console.log(error);
			return false;
		}
	},
	ROLLBACK: async ({
		taskId,
		stepId,
		clarification,
		logs,
		attachments,
	}: {
		taskId: string | string[] | number;
		stepId: number;
		clarification?: string;
		logs: {
			stepId: number;
			note: string;
		}[];
		attachments?: File[];
	}) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const form = new FormData();
			form.append("StepId", String(stepId));
			if (clarification) form.append("Clarification", clarification);
			logs.forEach((l, i) => {
				form.append(`Logs[${i}].StepId`, String(l.stepId));
				form.append(`Logs[${i}].Note`, l.note ?? "");
			});
			attachments?.forEach((f) => form.append("Attachments", f));

			const res = await fetch(`${url}/tasks/rollback/${taskId}`, {
				method: "POST",
				headers: {
					...auth,
				},
				body: form,
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	FLAG_TASK: async ({
		taskId,
		teamLeaderId,
		comment,
	}: {
		taskId: number;
		teamLeaderId?: number;
		comment?: string;
	}) => {
		try {
			const authHeader = authService.authHeader();
			const body =
				teamLeaderId !== undefined && comment !== undefined
					? JSON.stringify({ teamLeaderId, comment })
					: undefined;
			const res = await fetch(`${url}/tasks/${taskId}/flag`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
					...authHeader,
				},
				body,
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	SKIP: async (taskId: number) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${taskId}/skip`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					...authHeader,
				},
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	PAUSE: async (taskId: number) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${taskId}/pause`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
					...authHeader,
				},
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	PREVIOUS_TASKS: async (taskId: string | number) => {
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
			//console.log(error);
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
				data: {
					assignedUser?: { id: number; name: string };
					assignableUsers: { id: number; name: string }[];
				};
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
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
			//console.log(error);
			return false;
		}
	},
	GET_JUMP_POINTS: async (taskId: number) => {
		try {
			const auth = authService.authHeader();
			if (!auth) {
				console.error("Unathorized");
				return false;
			}
			const res = await fetch(`${url}/tasks/${taskId}/jump-points`, {
				method: "GET",
				headers: {
					...auth,
				},
			});
			const data: ResponseService<NodeAhead[]> = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	JUMP_TASK: async (
		taskId: number,
		options: { nodeId: number; stepId: number }[]
	) => {
		try {
			const authHeader = authService.authHeader();
			const res = await fetch(`${url}/tasks/${taskId}/jump`, {
				method: "PUT",
				headers: {
					"Content-Type": "application/json",
					...authHeader,
				},
				body: JSON.stringify(options),
			});
			const data: {
				data: ITask;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
	GET_ROLLBACK_ATTACHMENT_URL: (attachmentId: number) =>
		`${url}/tasks/rollback/attachment/${attachmentId}`,
	GET_ROLLBACK_HISTORY: async (taskId: number) => {
		try {
			const res = await fetch(`${url}/tasks/${taskId}/history`);
			const data: {
				data: {
					rollbacks: {
						id: number;
						clarification?: string;
						task: BasicInfo;
						attachments: {
							id: number;
							fileName: string;
							contentType: string;
							fileSize: number;
						}[];
					}[];
					issues: {
						id: number;
						note?: string;
						task: BasicInfo;
					}[];
				};
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			//console.log(error);
			return false;
		}
	},
};

export default TASKS;
