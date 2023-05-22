import authService from "../Auth";
import { url } from "./";

const RESOURCES = {
	GROUPS: {
		CREATE: async ({
			name,
			colorCode,
		}: {
			name: string;
			colorCode: string;
		}) => {
			try {
				const res = await fetch(`${url}/groups`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name, colorCode }),
				});
				if (res.status == 404) return false;
				const data: {
					data: IGroup;
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
				const res = await fetch(`${url}/groups/mini`);
				const data: {
					data: { id: number; name: string }[];
					error: boolean;
					message: string;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL: async () => {
			try {
				const res = await fetch(`${url}/groups`);
				const data: {
					data: IGroup[];
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
			colorCode,
		}: {
			id: string | string[];
			name: string;
			colorCode: string;
		}) => {
			try {
				const res = await fetch(`${url}/groups/${id}`, {
					method: "PATCH",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name, colorCode }),
				});
				const data: {
					data: IGroup;
					error: boolean;
					message: string;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
	},
	USERS: {
		CREATE: async ({
			name,
			groupId,
			roleId,
		}: {
			name: string;
			groupId: number;
			roleId: number;
		}) => {
			try {
				const res = await fetch(`${url}/users`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name, groupId, roleId }),
				});
				const data: {
					error: boolean;
					message: string;
					data: { code: string; user: IUser };
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
			groupId,
			roleId,
		}: {
			id: string | number;
			name: string;
			groupId: number;
			roleId: number;
		}) => {
			try {
				const res = await fetch(`${url}/users/${id}`, {
					method: "PATCH",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name, groupId, roleId }),
				});
				const data: {
					error: boolean;
					message: string;
					data: IUser;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		DELETE: async ({ id }: { id: string | number }) => {
			try {
				const res = await fetch(`${url}/users/${id}`, {
					method: "DELETE",
				});
				const data: {
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
				const res = await fetch(`${url}/users/mini`);
				const data: {
					id: number;
					name: string;
					group: { id: number; name: string };
				}[] = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL: async () => {
			try {
				const res = await fetch(`${url}/users`);
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
		GET_ONE: async (id: number | string) => {
			try {
				const res = await fetch(`${url}/users/${id}`);
				const data: {
					data: IUser;
					error: boolean;
					message: string;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
	},
	TEAMS: {
		GET_ALL: async () => {
			try {
				const res = await fetch(`${url}/teams`);
				const data: ITeam[] = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		CREATE: async ({ name }: { name: string }) => {
			try {
				const res = await fetch(`${url}/teams`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name }),
				});
				if (res.status == 404) return false;
				const data: ITeam = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
	},
	SECTIONS: {
		GET_ONE: async (id: string | string[]) => {
			try {
				const auth = authService.authHeader();
				if (auth) {
					const res = await fetch(`${url}/sections/${id}`, {
						headers: {
							...auth,
						},
					});
					const data: {
						data: ISection;
						error: boolean;
						message: string;
					} = await res.json();
					return data;
				}
				return false;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_ALL: async () => {
			try {
				const auth = authService.authHeader();
				if (auth) {
					const res = await fetch(`${url}/sections`, {
						headers: {
							...auth,
						},
					});
					const data: {
						error: boolean;
						message: string;
						data: { id: number; name: string }[];
					} = await res.json();
					return data;
				}
				return false;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		CREATE: async ({
			name,
			groups,
			headId,
		}: {
			name: string;
			headId: number;
			groups: number[];
		}) => {
			try {
				const auth = authService.authHeader();
				if (auth) {
					const res = await fetch(`${url}/sections`, {
						method: "POST",
						headers: {
							"Content-Type": "application/json",
							...auth,
						},
						body: JSON.stringify({ name, headId, groups }),
					});
					if (res.status == 404) return false;
					const data: { id: number; name: string } = await res.json();
					return data;
				}
				return false;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
	},
};

export default RESOURCES;
