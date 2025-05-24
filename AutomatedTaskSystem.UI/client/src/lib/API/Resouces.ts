import { AccountType } from "../../components/pageComponent/users/addUser";
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
		GET_ONE: async (id: string | string[] | number) => {
			try {
				const res = await fetch(`${url}/groups/${id}`);
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
		Get_Tm_leaders:async (id:number)=>{
			try {
				const res = await fetch(`${url}/groups/GetTeamLeader/${id}`);
				const data: {
					data: {id:number,name:string}[];
					error: boolean;
					message: string;
				} = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		}
	},
	USERS: {
		CREATE: async ({
			name,
			groupId,
			role,
			hrCode,
			email,
			teamLeaderId,
			accountType,
			vacation,
			permission,
			permission_MAX
		}: {
			name: string;
			groupId: number;
			role: UserRole;
			hrCode:string,
			email:string,
			teamLeaderId:number | null,
			accountType:number,
			vacation:IVacation
			permission:number,
			permission_MAX:number
		}) => {
			try {
				const res = await fetch(`${url}/users`, {
					method: "POST",
					headers: {
						"Content-Type": "application/json",
					},
					body: JSON.stringify({ name, groupId, role,hrCode,
					email,
					teamLeaderId,
					accountType,
					vacation,
					permission,
					permission_MAX }),
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
				hrCode,
				email,
				onBoard,
				archived,
				groupId,
				role,
				accountType,
				teamLeaderId,
				title,
				phone,
				vacation,
				permission,
				permission_MAX
			}: {
				id: string | number;
				name: string;
				hrCode: string;
				email: string;
				onBoard: boolean;
				archived: boolean;
				groupId: number;
				role: number;
				accountType: AccountType;
				teamLeaderId: number | null;
				title:string | null,
				phone:string | null,
				vacation: IVacation;
				permission: number;
				permission_MAX:number
			}) => {
				try {
					const authHeader = authService.authHeader();
					const res = await fetch(`${url}/users/${id}`, {
						method: "PATCH",
						headers: {
							"Content-Type": "application/json",
							...authHeader
						},

						body: JSON.stringify({ 
							name, 
							hrCode,
							email,
							onBoard,
							archived,
							groupId, 
							role,
							accountType,
							teamLeaderId,
							title,
							phone,
							vacation,
							permission,
							permission_MAX
						}),
					});
					
					if (!res.ok) {
						throw new Error(`HTTP error! status: ${res.status}`);
					}

					const data: {
						error: boolean;
						message: string;
						data: IUser;
					} = await res.json();

					if (data.error) {
						console.error(data.message);
						return { ...data, error: true };
					}

					return data;
				} catch (error) {
					console.error("Error editing user:", error);
					return {
						error: true,
						message: error instanceof Error ? error.message : "Unknown error occurred",
						data: null
					};
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
		GET_ALL_USER_TASKS: async () => {
			try {
				const auth = authService.authHeader();
				const res = await fetch(`${url}/users/tasks`, {
					headers: {
						...auth,
					},
				});
				const data: ResponseService<UserTaskCount[]> = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_USER_TASKS: async (id: string | string[]) => {
			try {
				const res = await fetch(`${url}/users/${id}/tasks`);
				const data: ResponseService<UserTaskInfo> = await res.json();
				return data;
			} catch (error) {
				console.error(error);
				return false;
			}
		},
		GET_USER_CHANGES: async (id:number ) => {
    try {
        // Get authentication headers
        const authHeader = authService.authHeader();
        
        // Make API request to the user changes endpoint
        const res = await fetch(`${url}/users/GetUserChanges/${id}`, {
            method: "GET",
            headers: {
                ...authHeader
            }
        });
        
        // Check if response is successful
        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }
        
        // Parse the response data
        const data: {
            data: IUserChange[];
            error: boolean;
            message: string;
        } = await res.json();
        
        return data;
    } catch (error) {
        // Handle errors and provide useful error information
        console.error("Error fetching user changes:", error);
        return {
            error: true,
            message: error instanceof Error ? error.message : "Unknown error occurred",
            data: []
        };
    }
}
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
