import { IReport } from "../../pages/reports/[projectId]";
import authService from "../Auth";
import { url } from "./";

const PROJECTS = {
	REPORT: async (projectId: string | string[]) => {
		try {
			const res = await fetch(`${url}/projects/${projectId}/report`);
			const data: IReport = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	USERS_UNASSIGNED: async (projectId: string | string[]) => {
		try {
			const res = await fetch(
				`${url}/projects/${projectId}/users/unassigned`
			);
			const data: {
				data: IUser[];
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	USERS_ASSIGNED: async (projectId: string | string[]) => {
		try {
			const res = await fetch(
				`${url}/projects/${projectId}/users/assigned`
			);
			const data: {
				data: IUser[];
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	UNASSIGN: async ({
		projectId,
		userIds,
	}: {
		projectId: string | string[];
		userIds: number[];
	}) => {
		try {
			const res = await fetch(`${url}/projects/${projectId}/unassign`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ userIds }),
			});
			const data: IProject = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	ASSIGN: async ({
		projectId,
		userIds,
	}: {
		projectId: string | string[];
		userIds: number[];
	}) => {
		try {
			const res = await fetch(`${url}/projects/${projectId}/assign`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ userIds }),
			});
			const data: IProject = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	CREATE: async ({
		name,
		description,
	}: {
		name: string;
		description: string;
	}) => {
		try {
			const res = await fetch(`${url}/projects`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ name, description }),
			});
			const data: {
				data: IProject;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL_LOS: async (projectId: number) => {
		try {
			const res = await fetch(`${url}/projects/${projectId}/los`);
			const data: {
				error: boolean;
				message: string;
				data: IProject[];
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL: async () => {
		try {
			const res = await fetch(`${url}/projects`);
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
	GET_ONE: async (id: string | string[]) => {
		try {
			const res = await fetch(`${url}/projects/${id}`);
			if (res.status >= 400) return false;
			const data: {
				data: IProject;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ONE_DETAILED: async (id: string | string[]) => {
		try {
			const res = await fetch(`${url}/projects/${id}/details`);
			if (res.status >= 400) return false;
			const data: {
				data: ProjectDetails;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	UNITS: {
		ADD: async ({
			name,
			projectId,
		}: {
			name: string;
			projectId: number;
		}) => {
			try {
				const res = await fetch(`${url}/projects/${projectId}/units`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name }),
				});
				const data: {
					data: Unit;
					error: boolean;
					message: string;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		REMOVE: async (id: number) => {
			try {
				const res = await fetch(`${url}/units/${id}`, {
					method: "DELETE",
					headers: {
						"Content-Type": "application/json",
					},
				});
				const data = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		LESSONS: {
			ADD: async ({ unitId, name }: { unitId: number; name: string }) => {
				try {
					const res = await fetch(`${url}/units/${unitId}/lessons`, {
						method: "POST",
						headers: {
							"Content-Type": "application/json",
						},
						body: JSON.stringify({ name }),
					});
					const data: Lesson = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			REMOVE: async (id: number) => {
				try {
					const res = await fetch(`${url}/lessons/${id}`, {
						method: "DELETE",
						headers: {
							"Content-Type": "application/json",
						},
					});
					const data = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			LEARNING_OBJECTIVES: {
				COMMENT: async (loId: number, comment: string) => {
					try {
						const auth = authService.authHeader();
						if (!auth) {
							console.error("Unathorized");
							return false;
						}
						const res = await fetch(
							`${url}/learning-objectives/${loId}/comment`,
							{
								method: "POST",

								headers: {
									"Content-Type": "application/json",
									...auth,
								},
								body: JSON.stringify({
									comment,
								}),
							}
						);
						const data = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
				CREATE: async ({
					lessonId,
					name,
					schemaId,
					environment,
					tag,
					template,
				}: {
					lessonId: number;
					name: string;
					schemaId: number;
					tag: string;
					template: string;
					environment: string;
				}) => {
					try {
						const res = await fetch(
							`${url}/lessons/${lessonId}/learning-objective`,
							{
								method: "POST",
								headers: {
									"Content-Type": "application/json",
								},
								body: JSON.stringify({
									name,
									schemaId,
									environment,
									tag,
									template,
								}),
							}
						);
						const data: LearningObjective = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
				EDIT: async (
					id: number,
					edit: {
						name: string;
						tag: string;
						environment: string;
						template: string;
						schemaId: number;
						steps: number[];
					}
				) => {
					try {
						const res = await fetch(
							`${url}/learning-objectives/${id}`,
							{
								method: "PATCH",
								headers: {
									"Content-Type": "application/json",
								},
								body: JSON.stringify(edit),
							}
						);
						const data: LearningObjective = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
				REMOVE: async (id: number) => {
					try {
						const res = await fetch(
							`${url}/learning-objectives/${id}`,
							{
								method: "DELETE",
							}
						);
						const data = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
				ASSIGN: async (id: string | string[], userIds: number[]) => {
					try {
						const res = await fetch(
							`${url}/learning-objectives/${id}/assign`,
							{
								method: "POST",
								headers: {
									"Content-Type": "application/json",
								},
								body: JSON.stringify({ userIds }),
							}
						);
						const data: LearningObjective = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
				UNASSIGN: async (id: string | string[], userIds: number[]) => {
					try {
						const res = await fetch(
							`${url}/learning-objectives/${id}/unassign`,
							{
								method: "POST",
								headers: {
									"Content-Type": "application/json",
								},
								body: JSON.stringify({ userIds }),
							}
						);
						const data: LearningObjective = await res.json();
						return data;
					} catch (error) {
						console.error(error);
						return false;
					}
				},
			},
		},
	},
};

export default PROJECTS;
