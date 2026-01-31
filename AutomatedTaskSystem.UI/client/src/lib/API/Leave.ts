import { format } from "date-fns";
import { url } from ".";
import authService from "../Auth";
import { BaseOpinion, PageList, ResponseService } from "../../../app";



declare type UserRole = number; // Assuming UserRole is a number type

interface ILeave { //this model is used as a base model for other models
    startDate: string;
    endDate: string;
    reason?: string;
    noteForManager?: string | null;
    type: LeaveRequestType;
}

enum LeaveRequestType {
    Annual = "Annual",
    Sick = "Sick",
    Emergency = "Emergency",
}

enum LeaveRequestStatus {
    Pending = "Pending",
    Approved = "Approved",
    Rejected = "Rejected",
    Cancelled = "Cancelled",
}


/*
    shared props


*/

interface ICreateLeave extends ILeave {
    userId?: number;
    medicalCertificate?: File | null;
}

interface IGetAllLeavesApiResponse {
    items: IGetLeaveRequestForCalander[];
    totalCount: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

// We Omit everything except 'page' and 'pageSize'
export interface IGetAllLeavesRequest extends Omit<PageList<any>, 
    "items" | "totalPages" | "totalCount" | "hasNextPage" | "hasPreviousPage"> 
{
    userId?: number;
    role?: number;
    searchTerm?: string;
    fromDate?: string;
    toDate?: string;
    status?: LeaveRequestStatus | "all";
    type?: LeaveRequestType | "all";  
    // Index signature preserved
    [key: string]: string | number | boolean | undefined;
}

interface IGetLeaveRequest extends ILeave {
    id: number;
    status: LeaveRequestStatus;
    duration: number
    user: { id: number; name: string; role: number } | null
    dateCreated:string
}

// Specific opinion types
interface LeaveOpinion extends BaseOpinion {
  type: 'leave';
  leaveRequestId: number;
  status: LeaveRequestStatus;
}

interface IBulkUpdateStatusRequest {
    ids: number[];
    status: LeaveRequestStatus;
}

type IGetAllLeavesRequestNoPagination = Omit<IGetAllLeavesRequest, 'page' | 'pageSize'> & { disablePagination: true };

///////////////////////////////
// Calander  Models
interface IGetLeaveRequestForCalander extends ILeave {
    id: number,
    duration: number,
    status: LeaveRequestStatus,
    myStatus?:LeaveRequestStatus,
    dateCreated: string,
    user: { id: number, name: string }
}


////////////////////////////////////
// Request Details
interface IGetLeaveRequestForDetails extends ILeave {
    id: number,
    duration: number,
    status: LeaveRequestStatus,
    dateCreated: string,
    user: IUser,
    opinions: IGetOpinion[]

}

interface IGetOpinion extends IOpinion {
    id: number,
    dateCreated: string,

}

// The GET_ALL_DB function needs to expect a paginated response type
const LEAVE = {
    GET_ALL_DB: async (
        params?: Record<string, string | number | boolean | undefined> // Added undefined for optional params
    ): Promise<ResponseService<PageList<IGetLeaveRequestForCalander[]>>> => { // Changed return type to IGetAllLeavesApiResponse
        try {
            const headers = authService.authHeader();
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

            const res = await fetch(`${url}/Leave${query}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...headers
                }
            });
            // The response from the backend should now be an object with 'items' and 'totalCount'
            const response: ResponseService<PageList<IGetLeaveRequestForCalander[]>> = await res.json();

            return response;
        } // ... inside catch block
        catch (error) {
            console.error("Error fetching all leaves from DB:", error);
            return {
                error: true, // Assuming ResponseService has an 'error' boolean or specific error message string
                message: `Failed to fetch leaves: ${error instanceof Error ? error.message : String(error)}`,
                data: undefined // Or null, depending on your ResponseService definition
            };
        }
    },
     // This method is for getting all filtered data (used by export button)
    GET_ALL_FOR_EXPORT: async (params: Omit<IGetAllLeavesRequest, 'page' | 'pageSize'>) => {
        try {
            const headers = authService.authHeader();
            // Explicitly set disablePagination to true for export
            const finalParams: IGetAllLeavesRequest = { ...params, disablePagination: true, page: undefined, pageSize: undefined };

            const filteredParams = Object.entries(finalParams || {}).reduce((acc, [key, value]) => {
                if (value !== undefined && value !== null && String(value).trim() !== '') {
                    acc[key] = String(value);
                }
                return acc;
            }, {} as Record<string, string>);

            const query = Object.keys(filteredParams).length > 0
                ? `?${new URLSearchParams(filteredParams).toString()}`
                : "";

            const res = await fetch(`${url}/Leave?${query}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...headers
                }
            }); // Use the same C# controller endpoint
            const response: ResponseService<IGetAllLeavesApiResponse> = await res.json(); // Still expect PageList structure
            return response;
        } catch (error) {
            console.error("Error fetching all leaves for export:", error);
            throw error;
        }
    },

    CREATE: async (leaveData: ICreateLeave): Promise<ResponseService<boolean>> => {
        try {
            const formData = new FormData();

            if (leaveData?.userId !== undefined && leaveData?.userId !== null) {
                formData.append('UserId', leaveData.userId.toString());
            }

            formData.append('Type', leaveData.type);
            formData.append('StartDate', leaveData.startDate);
            formData.append('EndDate', leaveData.endDate);
            if (leaveData.reason !== undefined && leaveData.reason !== null) {
                formData.append('Reason', leaveData.reason);
            }

            if (leaveData.noteToManager) {
                formData.append('NoteForManager', leaveData.noteToManager);
            }

            if (leaveData.medicalCertificate) {
                formData.append('MedicalCertificate', leaveData.medicalCertificate);
            }

            const res = await fetch(`${url}/Leave`, {
                method: "POST",
                body: formData,
            });

            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }

            const data: ResponseService<boolean> = await res.json(); // Assuming your backend returns ResponseService<boolean>
            return data;
        } catch (error) {
            console.error('Error creating leave request:', error);
            return { data: false, error: true,message:"Error creating leave request" }; // Return a consistent ResponseService type
        }
    },
    GET_SINGLE: async (leaveRequestId: number): Promise<ResponseService<IGetLeaveRequest[]> | false> => {
        try {
            const res = await fetch(`${url}/Leave/${leaveRequestId}`);
            const data: ResponseService<IGetLeaveRequest[]> = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },

   GET_ALL_BY_USER: async (
        userId: number,
        params?: Record<string, string | number | boolean | undefined | Date>
    ): Promise<ResponseService<PageList<IGetLeaveRequest[]>>> => {
        try {
          
            const finalParams: IGetAllLeavesRequest = { 
                ...params, 
                userId: userId
            };

            const filteredParams = Object.entries(finalParams).reduce((acc, [key, value]) => {
                if (value !== undefined && value !== null && String(value).trim() !== '') {
                    if (value instanceof Date) {
                        acc[key] = format(value, 'yyyy-MM-dd');
                    } else {
                        acc[key] = String(value);
                    }
                }
                return acc;
            }, {} as Record<string, string>);

            const query = Object.keys(filteredParams).length > 0
                ? `?${new URLSearchParams(filteredParams).toString()}`
                : "";

            const res = await fetch(`${url}/Leave/LeaveRequestsByUserId${query}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...authService.authHeader()
                }
            });

            if (!res.ok) {
                const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
                console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
                return {
                    error: true,
                    message: errorResponse.message || `HTTP error! status: ${res.status}`,
                    data: undefined
                };
            }

            const data: ResponseService<PageList<IGetLeaveRequest[]>> = await res.json();
            return data;
        } catch (error) {
            console.error('Error fetching leaves by user:', error);
            return {
                error: true,
                message: error instanceof Error ? error.message : 'Unknown error occurred',
                data: undefined
            };
        }
    },
    GET_LEAVE_BY_USER: async (userId: number): Promise<ResponseService<IGetLeaveRequestForDetails>> => {

        const headers = authService.authHeader();
        const res = await fetch(`${url}/Leave/LeaveRequestByUserId/${userId}`, {
            headers: {
                "Content-Type": "application/json",
                ...headers
            }
        });

        const data: ResponseService<IGetLeaveRequestForDetails> = await res.json();
        return data;
   
    },
    CREATE_OPINION: async (opinion: IOpinion): Promise<ResponseService< IGetOpinion>> => { // Changed return type

        const auth = authService.authHeader();
        const res = await fetch(`${url}/Leave/CreateOpinion`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                ...auth
            },
            body: JSON.stringify(opinion),
        });

        const data: ResponseService< IGetOpinion>= await res.json();
        return data;

    },
    BULK_UPDATE_STATUS: async (request: IBulkUpdateStatusRequest): Promise<ResponseService<boolean>> => {
        try {
            
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Leave/CreateBulkOpinion`, { // Assuming this is the new endpoint
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                },
                body: JSON.stringify(request),
            });

            if (!res.ok) {
                const errorResponse = await res.json().catch(() => ({ message: `HTTP error! status: ${res.status}` }));
                return {
                    error: true,
                    message: errorResponse.message || `Failed to perform bulk update.`,
                    data: false
                };
            }

            return await res.json();
        } catch (error) {
            console.error("Error during bulk status update:", error);
            return {
                error: true,
                message: error instanceof Error ? error.message : 'An unknown error occurred during bulk update.',
                data: false
            };
        }
    },
    GET_USER_INFO: async (userId: number): Promise<ResponseServiceWithData<IUser> | false> => {
        try {
            const res = await fetch(`${url}/users/${userId}`);
            const data: ResponseServiceWithData<IUser> = await res.json();
            return data;
        } catch (error) {
            console.error(error);
            return false;
        }
    },
    GET_SICK_ATTACHMENT: async (leaveRequestId: number) => {
        try {
            const response = await fetch(`${url}/medical-certificate/${leaveRequestId}`, {
                method: "GET",
            });

            if (!response.ok) {
                throw new Error("Failed to fetch medical certificate");
            }

            const blob = await response.blob();
            const contentDisposition = response.headers.get("Content-Disposition");
            const fileNameMatch = contentDisposition?.match(/filename="?([^"]+)"?/);
            const fileName = fileNameMatch?.[1] || "medical-certificate";

            const downloadUrl = window.URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = downloadUrl;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(downloadUrl);

            return true;
        } catch (error) {
            console.error("Error downloading medical certificate:", error);
            return false;
        }
    },
    CANCEL_LEAVE: async (leaveRequestId: number): Promise<ResponseService<boolean> | false> => {
        try {
            const auth = authService.authHeader();
            const res = await fetch(`${url}/Leave/cancel/${leaveRequestId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    ...auth
                }
            });

            const data: ResponseService<boolean> = await res.json();
            return data;
        } catch (error) {
            console.error("Error cancelling leave:", error);
            return false;
        }
    },
};

export type {
    ILeave,
    ICreateLeave,
    IGetLeaveRequest,
    IGetLeaveRequestForCalander,
    IGetLeaveRequestForDetails,
    IGetAllLeavesRequest,
    IOpinion,
    IGetOpinion,
    IGetAllLeavesRequestNoPagination,
    IBulkUpdateStatusRequest
};

export { LeaveRequestStatus,
    LeaveRequestType,}

export default LEAVE;