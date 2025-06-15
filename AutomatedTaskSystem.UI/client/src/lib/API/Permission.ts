import { format } from "date-fns";
import { url } from ".";
import authService from "../Auth";

// Assuming these are defined elsewhere, e.g., in a global types file or ResponseService.ts
// Re-declare if not globally available, or import from a common types file
// declare interface ResponseService<T> {
//     data?: T;
//     error?: string;
//     message?: string;
//     // For pagination, your .NET backend's PageList<T> should provide these:
//     totalCount?: number;
//     currentPage?: number;
//     pageSize?: number;
//     totalPages?: number;    
// }

// declare interface IUser {
//     id: number;
//     name: string;
//     role?: number;
//     // ... any other user properties
// }

declare type UserRole = number; // Assuming UserRole is a number type

declare interface ResponseService<T> {
    data?: T;
    error?: boolean;
    message?: string;
}


declare interface ResponseServiceWithData<T> extends ResponseService<T> {
    data: T;
}


declare interface PageList<T>
{
    items:T;
    page:number;
    pageSize:number;
    totalPages:number;
    totalCount:number;
    hasNextPage:boolean;
    hasPreviousPage:boolean;
}

interface IPermission {
    id: number;
    type: PermissionType;
    permissionDate: string;
    fromTime: string;
    toTime: string;
    reason: string;
    duration: number;
    status: PermissionRequestStatus;
    myStatus?: PermissionRequestStatus;
    user: {
        id: number;
        name: string;
    };
    dateCreated: string;
}

interface ICreatePermission {
    userId?: number;
    type: PermissionType;
    date: string;
    from: string;
    to: string;
    reason?: string;
    PermissionDate: string;
}

interface IGetPermissionDetails {
    id: number;
    type: PermissionType;
    permissionDate: string;
    fromTime: string;
    toTime: string;
    reason: string;
    noteToManager: string;
    duration: number;
    status: PermissionRequestStatus;
    user?: IUser;
    dateCreated: string;
    opinions?: IGetOpinion[];
}

 enum PermissionRequestStatus {
    Pending = "pending", // Assign string values for clarity and easier debugging
    Approved = "approved",
    Rejected = "rejected",
    Cancelled = "cancelled"
}

 enum PermissionType {
    EarlyDeparture = "EarlyDeparture",
    LateArrival = "LateArrival",
    WorkAssignment = "WorkAssignment",
    Departure = "Departure"
}

interface IOpinion {
    permissionId: number;
    comment?: string;
    status: PermissionRequestStatus;
    isApproved?: boolean;
    user?: { id: number; name: string; role: number };
}

interface IGetOpinion extends IOpinion {
    id: number;
    dateCreated: string;
}

// Updated interface to reflect server-side pagination response structure
interface IGetAllPermissionsApiResponse {
    items: IPermission[];
    totalCount: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

interface IGetAllPermissionsRequest {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    fromDate?: string;
    toDate?: string;
    type?: PermissionType | "all";
    status?: PermissionRequestStatus | "all";
    disablePagination:boolean;
}

const PERMISSION = {
    GET_ALL: async (
        params?: Record<string, string | number | boolean | undefined> // Added undefined for optional params
    ): Promise<ResponseService<IGetAllPermissionsApiResponse> | false> => { // Changed return type
        try {
            const auth = authService.authHeader();
            // Filter out undefined, null, or empty string values from params
            const filteredParams = Object.entries(params || {}).reduce((acc, [key, value]) => {
                if (value !== undefined && value !== null && String(value).trim() !== '') {
                    acc[key] = String(value);
                }
                return acc;
            }, {} as Record<string, string>);


            const query = Object.keys(filteredParams).length > 0
                ? `?${new URLSearchParams(filteredParams).toString()}`
                : "";

            const res = await fetch(`${url}/Permission${query}`, {
                method: 'GET', // Or other HTTP methods like 'POST', 'PUT', etc.
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                },
            });
            const response: ResponseService<IGetAllPermissionsApiResponse> = await res.json();
            return response;
        } catch (error) {
            console.error("Error fetching all permissions:", error);
            return false;
        }
    },
    GET_ALL_FOR_EXPORT: async (params: Omit<IGetAllPermissionsRequest, 'page' | 'pageSize'>) => {
        try {
            const headrs = authService.authHeader();
            // Explicitly set disablePagination to true for export and ensure page/pageSize are undefined
            const finalParams: IGetAllPermissionsRequest = { ...params, disablePagination: true, page: undefined, pageSize: undefined };

            // Filter out undefined, null, or empty string values for cleaner query parameters
            const filteredParams = Object.entries(finalParams || {}).reduce((acc, [key, value]) => {
                if (value !== undefined && value !== null && String(value).trim() !== '') {
                    acc[key] = String(value);
                }
                return acc;
            }, {} as Record<string, string>);

            const query = Object.keys(filteredParams).length > 0
                ? `?${new URLSearchParams(filteredParams).toString()}`
                : "";

            // Use the correct C# controller endpoint for Permissions
            const res = await fetch(`${url}/Permission${query}`,{
                headers:{
                    "Content-Type": "application/json",
                    ...headrs
                }
            });
            const response: ResponseService<IGetAllPermissionsApiResponse> = await res.json(); // Still expect PageList structure
            return response;
        } catch (error) {
            console.error("Error fetching all permissions for export:", error);
            throw error;
        }
    },
    
    CREATE: async (permission: ICreatePermission): Promise<ResponseService<boolean>> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                },
                body: JSON.stringify(permission),
            });

            const data: ResponseService<boolean> = await res.json(); // Assuming backend returns ResponseService<boolean>
            return data;
        } catch (error) {
            console.error(error);
            return { data: false,error:true, message: "Error creating permission" };
        }
    },
    GET_SINGLE: async (permissionId: number): Promise<ResponseService<IPermission>> => {
   
        const res = await fetch(`${url}/Permission/${permissionId}`);
        const data: ResponseService<IPermission> = await res.json();
        return data;

    },
    GET_ALL_BY_USER: async (
        userId: number,
        // Accept params as a Record for flexibility, similar to your Leave service
        params: Record<string, string | number | boolean | undefined | Date> = {}
    ): Promise<ResponseService<PageList<IPermission[]>>> => { // Changed return type to PageList<IPermission[]>
        // try {
        // Filter out undefined, null, or empty string values and format dates
        const filteredParams = Object.entries(params || {}).reduce((acc, [key, value]) => {
            if (value !== undefined && value !== null && String(value).trim() !== '') {
                let formattedValue = String(value);

                // Special handling for Date objects, assuming ISO format for backend
                if (value instanceof Date) {
                    formattedValue = format(value, "yyyy-MM-dd");
                }

                // Map frontend keys to backend query parameter names if necessary
                switch (key) {
                    case "pageNumber": // Direct mapping for pageNumber
                        acc[key] = formattedValue;
                        break;
                    case "pageSize": // Direct mapping for pageSize
                        acc[key] = formattedValue;
                        break;
                    case "searchTerm": // Direct mapping for searchTerm
                        acc[key] = formattedValue;
                        break;
                    case "date": // Mapping 'date' (from IGetAllPermissionsRequest)
                        acc[key] = formattedValue;
                        break;
                    case "type": // Mapping 'type'
                        acc[key] = formattedValue;
                        break;
                    case "status": // Mapping 'status'
                        acc[key] = formattedValue;
                        break;
                    default:
                        // Fallback for any other keys that match directly
                        acc[key] = formattedValue;
                        break;
                }
            }
            return acc;
        }, {} as Record<string, string>);

        const queryString = new URLSearchParams(filteredParams).toString();
        const fullUrl = `${url}/Permission/GetPermissionsByUserId/${userId}${queryString ? `?${queryString}` : ""}`;

        const res = await fetch(fullUrl);

        if (!res.ok) {
            // Handle HTTP errors
            const errorResponse: ResponseService<any> = await res.json();
            console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message || res.statusText}`);
            return {
                error: true,
                message: errorResponse.message || res.statusText,
                data: undefined,
                // statusCode: res.status
            };
        }

        // Expecting PageList structure from backend for consistency with leaves
        const data: ResponseService<PageList<IPermission[]>> = await res.json();
        return data;
        // } catch (error) {
        //     console.error("Error fetching permissions by user ID:", error);
        //     return {
        //         error: true,
        //         message: `Failed to fetch permissions: ${error instanceof Error ? error.message : String(error)}`,
        //         data: undefined
        //     };
        // }
    },
    GET_DETAILS: async (permissionId: number): Promise<ResponseService<IGetPermissionDetails>> => {
        // try {
            const res = await fetch(`${url}/Permission/GetSinglePermission/${permissionId}`);
            const data: ResponseService<IGetPermissionDetails> = await res.json();
            return data;
        // } catch (error) {
        //     console.error(error);
        //     return false;
        // }
    },
    CREATE_OPINION: async (opinion: IOpinion): Promise<ResponseService<IGetOpinion>> => {
        // try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission/approve`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                },
                body: JSON.stringify(opinion),
            });

            const data: ResponseService<IGetOpinion> = await res.json();
            return data;
        // } catch (error) {
        //     console.error(error);
        //     return false;
        // }
    },
    UPDATE: async (permissionId: number, request: Partial<ICreatePermission>): Promise<boolean> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission/${permissionId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                },
                body: JSON.stringify(request),
            });

            return res.ok;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    DELETE: async (permissionId: number): Promise<boolean> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission/${permissionId}`, {
                method: "DELETE",
                headers: {
                    ...auth
                },
            });

            return res.ok;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    CANCEL_PERMISSION: async (permissionId: number): Promise<ResponseService<boolean> | false> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission/cancel/${permissionId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                }
            });

            const data: ResponseService<boolean> = await res.json();
            return data;
        } catch (error) {
            console.error("Error cancelling permission:", error);
            return false;
        }
    },

    GET_BY_SAME_USER: async (): Promise<ResponseService<IPermission[]> | false> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Permission/GetPermissionsForSameUser`, {
                headers: {
                    ...auth
                },
            });

            const data: ResponseService<IPermission[]> = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    }
};

export type{
    IPermission,
    ICreatePermission,
    IGetPermissionDetails,
    IOpinion,
    IGetOpinion
};

export {PermissionRequestStatus,
    PermissionType,}

export default PERMISSION;