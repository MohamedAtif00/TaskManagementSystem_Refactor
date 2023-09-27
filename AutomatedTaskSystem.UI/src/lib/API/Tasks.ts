import { ITask } from "../../components/taskDetails";
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
    COMMENT: async (id: number, content: string) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(
                `${url}/tasks/${id}/comment`,
                {
                    method: "POST",
                    headers: {
                        ...authHeader,
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        comment: content,
                    }),
                }
            );
            const data: ResponseService<{
                user: {
                    id: number;
                    name: string;
                };
                id: number;
                content: string;
                timestamp: string;
            }> = await res.json();
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
    GET_ALL: async (projectId: string | string[]) => {
        try {
            const auth = authService.authHeader();
            if (!auth) {
                console.error("Unathorized");
                return false;
            }
            const res = await fetch(`${url}/projects/${projectId}/tasks`, {
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
                body: JSON.stringify({ stepId }),
            });
            const data: {
                data: ITask;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.log(error);
            return false;
        }
    },
    FLAG_TASK: async (taskId: number) => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/tasks/${taskId}/flag`, {
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
            console.log(error);
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
            console.log(error);
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
                data: {
                    assignedUser?: { id: number; name: string };
                    assignableUsers: { id: number; name: string }[];
                };
                error: boolean;
                message: string;
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
            console.log(error);
            return false;
        }
    },
    JUMP_TASK: async (taskId: number, options: {nodeId: number, stepId: number}[]) => {
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
            console.log(error);
            return false;
        }
    },
};

export default TASKS;
