import { ITaskBank } from "../../pages/schemas/task-bank";
import { url, CommonResponse, BasicInfo } from "./";

type Node = {
	schemaId: number;
	name: string;
	isStart: boolean;
	previous: number[];
	requires: number[];
};

export interface UnarchivableSchemaResponse {
	name: string;
	id: number;
	units: {
		name: string;
		id: number;
		lessons: {
			name: string;
			id: number;
			learningObjectives: {
				name: string;
				id: number;
			}[];
		}[];
	}[];
}

const SCHEMAS = {
	TASK_BANK: {
		DELETE: async (id: number) => {
			try {
				const res = await fetch(`${url}/schemas/task-bank/${id}`, {
					method: "DELETE",
				});
				const data: CommonResponse = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		EDIT: async (
			id: string | string[],
			request: {
				name: string;
				tl: boolean;
				type: number;
				group: number;
				duration: number;
			}
		) => {
			try {
				const res = await fetch(`${url}/schemas/task-bank/${id}`, {
					method: "PATCH",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify(request),
				});
				const data: ITaskBank = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		ADD: async (request: {
			name: string;
			tl: boolean;
			type: number;
			group: number;
			duration: number;
		}) => {
			try {
				const res = await fetch(`${url}/schemas/task-bank`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify(request),
				});
				const data: ITaskBank = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL: async () => {
			try {
				const res = await fetch(`${url}/schemas/task-bank`);
				const data: ITaskBank[] = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
	},
	DUPLICATE: async (id: number) => {
		try {
			const res = await fetch(`${url}/schemas/${id}/duplicate`, {
				method: "POST",
			});
			const data: {
				data: ISchema;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL_MINI: async () => {
		try {
			const res = await fetch(`${url}/schemas/mini`);
			const data: BasicInfo[] = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ALL: async () => {
		try {
			const res = await fetch(`${url}/schemas`);
			const data: ISchema[] = await res.json();
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
			const res = await fetch(`${url}/schemas`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ name, description }),
			});
			const data: {
				data: ISchema;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	DELETE: async (id: string | string[] | number) => {
		try {
			const res = await fetch(`${url}/schemas/${id}`, {
				method: "DELETE",
			});
			const data:
				| {
						error: false;
						message: string;
				  }
				| {
						error: true;
						message: string;
						data: UnarchivableSchemaResponse[];
				  } = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	GET_ONE: async (id: string | string[]) => {
		try {
			const res = await fetch(`${url}/schemas/${id}`);
			const data: {
				data: {
					description: string;
					id: number;
					name: string;
				};
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	EDIT: async ({
		id,
		name,
		description,
	}: {
		id: number | string | string[];
		name: string;
		description: string;
	}) => {
		try {
			const res = await fetch(`${url}/schemas/${id}`, {
				method: "PATCH",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({ name, description }),
			});
			const data: {
				data: ISchema;
				error: boolean;
				message: string;
			} = await res.json();
			return data;
		} catch (error) {
			console.error(error);
			return false;
		}
	},
	NODES: {
		EDIT: async ({
			id,
			name,
			isStart,
			previous,
			requires,
		}: { id: number } & Node) => {
			try {
				const res = await fetch(`${url}/nodes/${id}`, {
					method: "PATCH",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({
						name,
						isStart,
						previous,
						requires,
					}),
				});
				const data: INode = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		ADD: async ({ schemaId, name, isStart, previous, requires }: Node) => {
			try {
				const res = await fetch(`${url}/nodes/${schemaId}`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({
						name,
						isStart,
						previous,
						requires,
					}),
				});
				const data: INode = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		DELETE: async (id: number) => {
			try {
				const res = await fetch(`${url}/nodes/${id}`, {
					method: "DELETE",
				});
				const data: { info: string; error: boolean }[] =
					await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL_MINI: async (schemaId: number) => {
			try {
				const res = await fetch(`${url}/nodes/${schemaId}/mini`);
				const data: BasicInfo[] = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL: async (schemaId: number) => {
			try {
				const res = await fetch(`${url}/nodes/${schemaId}`);
				const data: INode[] = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		STEPS: {
			GET_MULTIPLE: async (nodes: number[]) => {
				try {
					const res = await fetch(
						`${url}/steps/multiple?${nodes
							.map((_) => `nodeId=${_}`)
							.join("&")}`
					);
					const data: {
						id: number;
						name: string;
						steps: BasicInfo[];
					}[] = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			ADD: async ({
				nodeId,
				taskBankItem,
				duration,
			}: {
				nodeId: string;
				taskBankItem: number;
				duration: number;
			}) => {
				try {
					const res = await fetch(`${url}/steps/${nodeId}`, {
						method: "POST",
						headers: {
							"Content-Type": "application/json",
						},
						body: JSON.stringify({
							taskBankItem,
							duration,
						}),
					});
					const data: IStep = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			UPDATE_PRIO: async ({
				stepId,
				priority,
			}: {
				stepId: string | number;
				priority: number | null;
			}) => {
				try {
					const res = await fetch(`${url}/steps/${stepId}/priority`, {
						method: "PATCH",
						headers: {
							"Content-Type": "application/json",
						},
						body: JSON.stringify({
							priority,
						}),
					});
					const data: {
						data: IStep;
						error: boolean;
						message: string;
					} = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			EDIT: async ({
				stepId,
				taskBankItem,
				duration,
			}: {
				stepId: string;
				taskBankItem: number;
				duration: number;
			}) => {
				try {
					const res = await fetch(`${url}/steps/${stepId}`, {
						method: "PATCH",
						headers: {
							"Content-Type": "application/json",
						},
						body: JSON.stringify({
							taskBankItem,
							duration,
						}),
					});
					const data: IStep = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
			REMOVE: async (id: number) => {
				try {
					const res = await fetch(`${url}/steps/${id}`, {
						method: "DELETE",
					});
					const data = await res.json();
					return data;
				} catch (error) {
					console.error(error);
					return false;
				}
			},
		},
	},
};

export default SCHEMAS;
