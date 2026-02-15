import { url } from ".";
import authService from "../Auth";
import { GetAllSprintsResponse, SprintOverviewData, LearningObjectivesProgressData, LearningObjectivesTableData, TagData, TaskSummary, LearningObjectiveSummary, LearningObjectiveTableRow, CurrentPhase } from './Sprints.d'
import {format} from 'date-fns'
import { IDName } from "./workFromHome";

// ResponseService type declaration (matches backend ResponseService<T>)
declare interface ResponseService<T> {
    data?: T;
    error?: boolean;
    message?: string;
}

// Re-export types for use in other files
export type { TagData, SprintOverviewData, LearningObjectivesProgressData, LearningObjectivesTableData, LearningObjectiveTableRow, CurrentPhase, TaskSummary, LearningObjectiveSummary };

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
    GET_ALL_CARDS_STREAM: async (
        sprintId: string | string[],
        onProgress?: (tasks: TaskInfo[], totalCount: number) => void
    ): Promise<TaskInfo[]> => {
	        try {
	            const auth = authService.authHeader();
	            const response = await fetch(`${url}/sprints/${sprintId}/tasks/cards/stream`, {
	                headers: {
	                    ...auth,
	                },
	            });
	
	            if (!response.ok) {
	                throw new Error(`HTTP error! status: ${response.status}`);
	            }
	
	            // Backend returns a single JSON array of task cards
	            const tasks: TaskInfo[] = await response.json();
	
	            // Optionally provide progressive updates in batches from the
	            // already-parsed array. This keeps the API surface similar to
	            // true streaming without fragile partial JSON parsing.
	            if (onProgress && tasks.length > 0) {
	                const batchSize = 50;
	                for (let i = batchSize; i < tasks.length; i += batchSize) {
	                    onProgress(tasks.slice(0, i), tasks.length);
	                    // Yield back to the event loop to allow the UI to paint.
	                    await new Promise((resolve) => setTimeout(resolve, 0));
	                }
	                onProgress(tasks, tasks.length);
	            }
	
	            return tasks;
	        } catch (error) {
	            console.error('Error streaming tasks:', error);
	            throw error;
	        }
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

    /**
     * Get sprint overview analytics including task summary, tag distribution, and learning objectives summary
     * @param sprintId - The ID of the sprint
     * @param timePeriod - Optional time period filter: 1=Today, 2=Last Week, 3=Last Month, 4=All Time (default)
     */
    GET_SPRINT_OVERVIEW: async (sprintId: string | string[], timePeriod?: number): Promise<ResponseService<SprintOverviewData>> => {
        try {
            const authHeader = authService.authHeader();

            // Build URL with optional timePeriod query parameter
            let apiUrl = `${url}/sprints/${sprintId}/analytics/overview`;
            if (timePeriod !== undefined && timePeriod !== null) {
                apiUrl += `?timePeriod=${timePeriod}`;
            }

            const res = await fetch(apiUrl, {
                headers: {
                    ...authHeader,
                },
            });

            if (!res.ok) {
                const errorData = await res.json();
                return {
                    error: true,
                    message: errorData.message || 'Failed to fetch sprint overview',
                    data: undefined
                };
            }

            const data: ResponseService<SprintOverviewData> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return {
                error: true,
                message: 'An error occurred while fetching sprint overview',
                data: undefined
            };
        }
    },

    /**
     * Get learning objectives progress data for the sprint
     */
    GET_SPRINT_LO_PROGRESS: async (sprintId: string | string[]): Promise<ResponseService<LearningObjectivesProgressData>> => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/sprints/${sprintId}/analytics/learning-objectives-progress`, {
                headers: {
                    ...authHeader,
                },
            });

            if (!res.ok) {
                const errorData = await res.json();
                return {
                    error: true,
                    message: errorData.message || 'Failed to fetch learning objectives progress',
                    data: undefined
                };
            }

            const data: ResponseService<LearningObjectivesProgressData> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return {
                error: true,
                message: 'An error occurred while fetching learning objectives progress',
                data: undefined
            };
        }
    },

    /**
     * Get learning objectives table data for the sprint
     */
    GET_SPRINT_LO_TABLE: async (sprintId: string | string[]): Promise<ResponseService<LearningObjectivesTableData>> => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/sprints/${sprintId}/analytics/learning-objectives-table`, {
                headers: {
                    ...authHeader,
                },
            });

            if (!res.ok) {
                const errorData = await res.json();
                return {
                    error: true,
                    message: errorData.message || 'Failed to fetch learning objectives table',
                    data: undefined
                };
            }

            const data: ResponseService<LearningObjectivesTableData> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return {
                error: true,
                message: 'An error occurred while fetching learning objectives table',
                data: undefined
            };
        }
    },

}

export default SPRINTS;