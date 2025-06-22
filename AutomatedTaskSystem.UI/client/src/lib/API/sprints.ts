import { url } from ".";
import authService from "../Auth";
import { GetAllSprintsResponse } from './Sprints.d'
import {format} from 'date-fns'
import { IDName } from "./workFromHome";
import { ISprint } from "../../../app";


const SPRINTS = {
    GET_ONE: async (id: string | string[]): Promise<ResponseService<ISprint>> => {
 
        const res = await fetch(`${url}/sprint/${id}`);
        if (res.status >= 400) return false;
        const data: {
            data: ISprint;
            error: boolean;
            message: string;
        } = await res.json();
        return data;

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
        los
    }: {
        name: string;
        description: string;
        startDate: string;
        endDate: string;
        los: IDName[];
    }): Promise<ResponseService<GetAllSprintsResponse>> => {
        try {
            // Get authentication headers
            const authHeader = authService.authHeader();

            // Make the POST request to the API
            const res = await fetch(`${url}/Sprint/create-sprint`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader, // Include authentication headers
                },
                body: JSON.stringify(
                    {
                        name,
                        description,
                        // Format dates to "M/d/yyyy" as per your original code
                        startDate: format(new Date(startDate), "M/d/yyyy"),
                        endDate: format(new Date(endDate), "M/d/yyyy"),
                        los, // Include the 'los' array in the request body
                    }
                ),
            });

            // Check if the response was successful (status code 2xx)
            if (!res.ok) {
                const errorData = await res.json();
                console.error(`API Error: ${res.status} - ${errorData.message || res.statusText}`);
                return { success: false, error: errorData.message || res.statusText };
            }

            // Parse the JSON response
            const data: ResponseService<GetAllSprintsResponse> = await res.json();
            return data;
        } catch (err) {
            // Handle network errors or issues with JSON parsing
            console.error("An unexpected error occurred:", err);
            // Return a structured error response
            return { success: false, error: (err as Error).message || "Unknown error occurred" };
        }
    },
    GET_ALL_CARDS: async (learningObjectId: string | string[]):Promise<ResponseService<TaskInfo[]>> => {

        const auth = authService.authHeader();
        const res = await fetch(`${url}/sprints/${learningObjectId}/tasks/cards`, {
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

	},
    
}

export default SPRINTS;