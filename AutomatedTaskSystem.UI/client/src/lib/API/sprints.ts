import { url } from ".";
import authService from "../Auth";
import { GetAllSprintsResponse } from './Sprints.d'
import {format} from 'date-fns'


const SPRINTS = {
    GET_ONE: async (id: string | string[]) => {
        try {
            const res = await fetch(`${url}/sprint/${id}`);
            if (res.status >= 400) return false;
            const data: {
                data: ISprint;
                error: boolean;
                message: string;
            } = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_ALL_SPRINTS:async ()=>{
        try {
         const authHeader = authService.authHeader();
                    const res = await fetch(`${url}/Sprint/get-all-sprint`, {
                        headers: {
                            ...authHeader,
                        },
                    });
                    const data: ResponseService<GetAllSprintsResponse[]> = await res.json();
                    return data;
                }
                catch (err) {
                    console.error(err);
                    return false;
                }
    },
    CREATE_SPRINT: async ({
        name,
        description,
        startDate,
        endDate,
    }: {
        name: string;
        description: string;
        startDate: string;
        endDate: string;
    }):Promise<ResponseService<GetAllSprintsResponse>> => {
        try {
            
            console.log(`${name} ${description} ${startDate} ${endDate}`);
            
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/Sprint/create-sprint`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader,
                },
                body: JSON.stringify(
                    { 
                        name, 
                        description, 
                        startDate: format(new Date(startDate), "M/d/yyyy"),
                        endDate : format(new Date(endDate), "M/d/yyyy")
                    }
                ),
            });
            const data: ResponseService<GetAllSprintsResponse> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
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
			const res = await fetch(`${url}/sprints/${projectId}/tasks/cards`, {
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
    
}

export default SPRINTS;