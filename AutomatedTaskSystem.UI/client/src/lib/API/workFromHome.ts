// src/types/workFromHomeTypes.d.ts (or similar central types file)

// Assuming these are defined elsewhere, e.g., in a global types file or ResponseService.ts
// Re-declaring for completeness if not globally available:
declare interface ResponseService<T> {
  data?: T;
  error?: boolean;
  message?: string;
}

declare interface PageList<T> {
  items: T;
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

declare interface ResponseServiceWithData<T> extends ResponseService<T> {
  data: T;
}


// Common interfaces (if not already declared in a shared file)
interface IDName {
  id: number;
  name: string;
}

interface IDNameWithRole extends IDName {
  role: UserRole;
}


// Work From Home Specific Interfaces
interface IWorkFromHome {
  date: string; // Single date for WFH
  noteForManager?: string;
}

interface ICreateWorkFromHome extends IWorkFromHome {
  userId?: number;
}

interface IGetAllWorkFromHomeRequest {
  userId?: number;
  role?: number;
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  fromDate?: string; // For filtering by date range
  toDate?: string; // For filtering by date range
  status?: WorkFromHomeStatus | "all";
  myStatus?: WorkFromHomeStatus | "all";
  [key: string]: string | number | boolean | undefined; // Index signature for flexible params
}

interface IGetWorkFromHomeRequest extends IWorkFromHome {
  id: number;
  status: WorkFromHomeStatus;
  user: IDName | null;
  dateCreated: string;
  myStatus?: WorkFromHomeStatus; // Manager's opinion status
}

interface IGetWorkFromHomeRequestForDetails extends IWorkFromHome {
  id: number;
  status: WorkFromHomeStatus;
  dateCreated: string;
  user: IUser;
  opinions: IGetOpinionWorkFromHome[];
}

interface ICreateWorkFromHomeOpinion {
  workFromHomeId: number;
  comment?: string;
  isApproved: boolean;
}

interface IGetOpinionWorkFromHome {
  id: number;
  workFromHomeId: number;
  isApproved: boolean;
  comment?: string;
  dateCreated: string;
  user: IDNameWithRole | null; // User who gave the opinion
}

// Work From Home Enums
enum WorkFromHomeStatus {
  Pending = "Pending",
  Approved = "Approved",
  Rejected = "Rejected",
  Cancelled = "Cancelled",
}

// Utility type for requests without pagination for export
type IGetAllWorkFromHomeRequestNoPagination = Omit<IGetAllWorkFromHomeRequest, "page" | "pageSize"> & { disablePagination: true };




import { format } from "date-fns"; // Make sure date-fns is installed (npm install date-fns or yarn add date-fns)
import { url } from "./index"; // Assuming 'url' is your base API URL
import authService from "../Auth";


const WORK_FROM_HOME = {
  // --- Get All Work From Home Requests (Paginated) ---
  GET_ALL: async (
    params?: Record<string, string | number | boolean | undefined>
  ): Promise<ResponseService<PageList<IGetWorkFromHomeRequest[]>>> => {
    try {
      const headers = authService.authHeader();

      const filteredParams = Object.entries(params || {}).reduce(
        (acc, [key, value]) => {
          if (
            value !== undefined &&
            value !== null &&
            String(value).trim() !== ""
          ) {
            acc[key] = String(value);
          }
          return acc;
        },
        {} as Record<string, string>
      );

      const query =
        Object.keys(filteredParams).length > 0
          ? `?${new URLSearchParams(filteredParams).toString()}`
          : "";

      const res = await fetch(`${url}/WorkFromHome${query}`, {
        headers: {
          "Content-Type": "application/json",
          ...headers,
        },
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        return {
          error: true,
          message: errorResponse.message || `Failed to fetch work from home requests: HTTP status ${res.status}`,
          data: undefined,
        };
      }

      const response: ResponseService<PageList<IGetWorkFromHomeRequest[]>> =
        await res.json();
      return response;
    } catch (error) {
      console.error("Error fetching all work from home requests:", error);
      return {
        error: true,
        message: `Failed to fetch work from home requests: ${
          error instanceof Error ? error.message : String(error)
        }`,
        data: undefined,
      };
    }
  },

  // --- Get All Work From Home Requests (For Export - No Pagination) ---
  GET_ALL_FOR_EXPORT: async (
    params: IGetAllWorkFromHomeRequestNoPagination
  ): Promise<ResponseService<PageList<IGetWorkFromHomeRequest[]>>> => {
    try {
      const headers = authService.authHeader();

      const finalParams: IGetAllWorkFromHomeRequest = {
        ...params,
        disablePagination: true,
        page: undefined, // Ensure these are not sent
        pageSize: undefined, // Ensure these are not sent
      };

      const filteredParams = Object.entries(finalParams || {}).reduce(
        (acc, [key, value]) => {
          if (
            value !== undefined &&
            value !== null &&
            String(value).trim() !== ""
          ) {
            acc[key] = String(value);
          }
          return acc;
        },
        {} as Record<string, string>
      );

      const query =
        Object.keys(filteredParams).length > 0
          ? `?${new URLSearchParams(filteredParams).toString()}`
          : "";

      const res = await fetch(`${url}/WorkFromHome${query}`, {
        headers: {
          "Content-Type": "application/json",
          ...headers,
        },
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        return {
          error: true,
          message: errorResponse.message || `Failed to fetch work from home requests for export: HTTP status ${res.status}`,
          data: undefined,
        };
      }

      const response: ResponseService<PageList<IGetWorkFromHomeRequest[]>> =
        await res.json();
      return response;
    } catch (error) {
      console.error("Error fetching all work from home requests for export:", error);
      return {
        error: true,
        message: `Failed to fetch work from home requests for export: ${
          error instanceof Error ? error.message : String(error)
        }`,
        data: undefined,
      };
    }
  },

  // --- Create Work From Home Request ---
  CREATE: async (
    workFromHomeData: ICreateWorkFromHome
  ): Promise<ResponseService<boolean>> => {
    try {
      const headers = authService.authHeader(); // Assuming authHeader provides object, not string directly

      // Work From Home requests typically don't involve file uploads like medical certificates
      // So we can send JSON directly instead of FormData if no files.
      const res = await fetch(`${url}/WorkFromHome`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json", // Send as JSON
          ...headers,
        },
        body: JSON.stringify(workFromHomeData), // Stringify the JSON payload
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        return {
          error: true,
          message: errorResponse.message || `Failed to create work from home request: HTTP status ${res.status}`,
          data: false,
        };
      }

      const data: ResponseService<boolean> = await res.json();
      return data;
    } catch (error) {
      console.error("Error creating work from home request:", error);
      return {
        data: false,
        error: true,
        message: `Error creating work from home request: ${
          error instanceof Error ? error.message : String(error)
        }`,
      };
    }
  },

  // --- Get Single Work From Home Request by ID ---
  GET_SINGLE: async (
    workFromHomeRequestId: number
  ): Promise<ResponseService<IGetWorkFromHomeRequestForDetails> | false> => {
    try {
      const headers = authService.authHeader();
      const res = await fetch(`${url}/WorkFromHome/${workFromHomeRequestId}`, {
        headers: {
          "Content-Type": "application/json",
          ...headers,
        },
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        // Return a proper ResponseService object on error
        return {
          error: true,
          message: errorResponse.message || `Failed to fetch work from home request details: HTTP status ${res.status}`,
          data: undefined
        };
      }

      const data: ResponseService<IGetWorkFromHomeRequestForDetails> = await res.json();
      return data;
    } catch (error) {
      console.error("Error fetching single work from home request:", error);
      // Return a proper ResponseService object on error
      return {
        error: true,
        message: `Error fetching single work from home request: ${error instanceof Error ? error.message : String(error)}`,
        data: undefined
      };
    }
  },

  // --- Get All Work From Home Requests by User ID ---
  GET_ALL_BY_USER: async (
    userId: number,
    params?: Record<string, string | number | boolean | undefined | Date>
  ): Promise<ResponseService<PageList<IGetWorkFromHomeRequest[]>>> => {
    try {
      const finalParams: IGetAllWorkFromHomeRequest = {
        ...params,
        userId: userId,
      };

      const filteredParams = Object.entries(finalParams).reduce(
        (acc, [key, value]) => {
          if (
            value !== undefined &&
            value !== null &&
            String(value).trim() !== ""
          ) {
            if (value instanceof Date) {
              acc[key] = format(value, "yyyy-MM-dd");
            } else {
              acc[key] = String(value);
            }
          }
          return acc;
        },
        {} as Record<string, string>
      );

      const query =
        Object.keys(filteredParams).length > 0
          ? `?${new URLSearchParams(filteredParams).toString()}`
          : "";

      const res = await fetch(`${url}/WorkFromHome/user/${userId}${query}`, { // Updated endpoint
        headers: {
          "Content-Type": "application/json",
          ...authService.authHeader(),
        },
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        return {
          error: true,
          message: errorResponse.message || `HTTP error! status: ${res.status}`,
          data: undefined,
        };
      }

      const data: ResponseService<PageList<IGetWorkFromHomeRequest[]>> =
        await res.json();
      return data;
    } catch (error) {
      console.error("Error fetching work from home requests by user:", error);
      return {
        error: true,
        message:
          error instanceof Error ? error.message : "Unknown error occurred",
        data: undefined,
      };
    }
  },

  // --- Submit Opinion on Work From Home Request ---
  CREATE_OPINION: async (
    opinion: ICreateWorkFromHomeOpinion
  ): Promise<ResponseService<IGetOpinionWorkFromHome>> => {
    try {
        const auth = authService.authHeader();
        const res = await fetch(`${url}/WorkFromHome/give-opinion`, { // Updated endpoint
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                ...auth,
            },
            body: JSON.stringify(opinion),
        });

        if (!res.ok) {
            const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
            console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
            return {
                error: true,
                message: errorResponse.message || `Failed to submit opinion: HTTP status ${res.status}`,
                data: undefined
            };
        }

        const data: ResponseService<IGetOpinionWorkFromHome> = await res.json();
        return data;
    } catch (error) {
        console.error("Error submitting work from home opinion:", error);
        return {
            error: true,
            message: `Error submitting work from home opinion: ${error instanceof Error ? error.message : String(error)}`,
            data: undefined
        };
    }
  },

  // --- Cancel Work From Home Request ---
  CANCEL: async (
    workFromHomeRequestId: number
  ): Promise<ResponseService<boolean>> => {
    try {
      const auth = authService.authHeader();
      const res = await fetch(`${url}/WorkFromHome/cancel/${workFromHomeRequestId}`, { // Updated endpoint
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          ...auth,
        },
      });

      if (!res.ok) {
        const errorResponse = await res.json().catch(() => ({ message: res.statusText }));
        console.error(`HTTP error! Status: ${res.status}, Message: ${errorResponse.message}`);
        return {
          error: true,
          message: errorResponse.message || `Failed to cancel work from home request: HTTP status ${res.status}`,
          data: false,
        };
      }

      const data: ResponseService<boolean> = await res.json();
      return data;
    } catch (error) {
      console.error("Error cancelling work from home request:", error);
      return {
        error: true,
        message: `Error cancelling work from home request: ${
          error instanceof Error ? error.message : String(error)
        }`,
        data: false,
      };
    }
  },
};

export type {
  IWorkFromHome,
  ICreateWorkFromHome,
  IGetWorkFromHomeRequest,
  IGetWorkFromHomeRequestForDetails,
  IGetAllWorkFromHomeRequest,
  ICreateWorkFromHomeOpinion,
  IGetOpinionWorkFromHome,
  IGetAllWorkFromHomeRequestNoPagination,
};

export { WorkFromHomeStatus };

export default WORK_FROM_HOME;