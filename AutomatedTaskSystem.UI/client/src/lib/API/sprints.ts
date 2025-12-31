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
    GET_ALL_SPRINTS:async (archived?: boolean)=>{
        try {
         const authHeader = authService.authHeader();
                    const queryParam = archived !== undefined ? `?archived=${archived}` : '';
                    const res = await fetch(`${url}/Sprint/get-all-sprint${queryParam}`, {
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
    UPDATE_SPRINT: async (
        sprintId: string,
        {
            name,
            description,
            startDate, // ISO string expected from frontend
            endDate,   // ISO string expected from frontend
            los        // ⭐ This now expects an array of numbers (IDs)
        }: {
            name: string;
            description: string;
            startDate: string;
            endDate: string;
            los: number[]; // Change IDName[] to number[]
        }
    ): Promise<ResponseService<ISprint>> => {
        try {
            const authHeader = authService.authHeader();

            const res = await fetch(`${url}/Sprint/update-sprint/${sprintId}`, {
                method: 'PUT', // Use PUT for updates
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader,
                },
                body: JSON.stringify({
                    name,
                    description,
                    // Format dates to "M/d/yyyy" for the backend
                    // Your C# backend expects "M/d/yyyy" if you use DateOnly.Parse directly.
                    // If DateOnly.Parse is robust enough for "YYYY-MM-DD", then keep it as is.
                    // For consistency, I'll use the format from CREATE_SPRINT.
                    startDate: format(new Date(startDate), "M/d/yyyy"),
                    endDate: format(new Date(endDate), "M/d/yyyy"),
                    los, // Send just the array of numbers
                }),
            });

            if (!res.ok) {
                const errorData = await res.json();
                console.error(`API Error: ${res.status} - ${errorData.message || res.statusText}`);
                return { error: true, message: errorData.message || res.statusText };
            }

            const data: ResponseService<ISprint> = await res.json();
            return data;
        } catch (err) {
            console.error("An unexpected error occurred while updating sprint:", err);
            return { error: true, message: (err as Error).message || "Unknown error occurred" };
        }
    },
    GET_ALL_CARDS_FOR_LO: async (learningObjectId: string | string[],sprintId:string |string[]):Promise<ResponseService<TaskInfo[]>> => {

        const auth = authService.authHeader();
        const res = await fetch(`${url}/sprints/${learningObjectId}/${sprintId}/tasks/cards`, {
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
    GET_ALL_CARDS: async (sprintId:string |string[]):Promise<ResponseService<TaskInfo[]>> => {

        const auth = authService.authHeader();
        const res = await fetch(`${url}/sprints/${sprintId}/tasks/cards`, {
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
    ARCHIVE_SPRINT: async (
        sprintId: number,
        archived: boolean = true
    ): Promise<ResponseService<ISprint>> => {
        try {
            const authHeader = authService.authHeader();

            const res = await fetch(`${url}/Sprint/${sprintId}/archive?archived=${archived}`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeader,
                },
            });

            if (!res.ok) {
                const errorData = await res.json();
                return {
                    error: true,
                    message: errorData.message || 'Failed to archive sprint',
                    data: undefined
                };
            }

            const data: ResponseService<ISprint> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return {
                error: true,
                message: 'An error occurred while archiving the sprint',
                data: undefined
            };
        }
    },

}

export default SPRINTS;