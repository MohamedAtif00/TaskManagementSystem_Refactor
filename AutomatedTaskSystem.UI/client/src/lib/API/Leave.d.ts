/// <reference path="../../../app.d.ts" />

declare global {
    // 1. Enums must be declared as 'const enum' or just 'enum' inside global
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

    interface ILeave {
        startDate: string;
        endDate: string;
        reason?: string;
        noteForManager?: string | null;
        type: LeaveRequestType;
    }

    interface ICreateLeave extends ILeave {
        userId?: number;
        medicalCertificate?: File | null;
    }


    interface IGetAllLeavesRequest extends Omit<PageList<any>, 
        "items" | "totalPages" | "totalCount" | "hasNextPage" | "hasPreviousPage"> 
    {
        userId?: number;
        role?: number;
        searchTerm?: string;
        fromDate?: string;
        toDate?: string;
        status?: LeaveRequestStatus | "all";
        type?: LeaveRequestType | "all";  
        [key: string]: string | number | boolean | undefined;
    }

    type IGetAllLeavesRequestNoPagination = Omit<IGetAllLeavesRequest, 'page' | 'pageSize'> & { disablePagination: true };

    interface IGetLeaveRequest extends ILeave {
        id: number;
        status: LeaveRequestStatus;
        duration: number;
        user: { id: number; name: string; role: number } | null;
        dateCreated: string;
    }

    interface IGetLeaveRequestForCalander extends ILeave {
        id: number;
        duration: number;
        status: LeaveRequestStatus;
        myStatus?: LeaveRequestStatus;
        dateCreated: string;
        user: { id: number; name: string };
    }

    interface IGetLeaveRequestForDetails extends ILeave {
        id: number;
        duration: number;
        status: LeaveRequestStatus;
        dateCreated: string;
        user: any; // Changed from IUser to any for safety, or define IUser below
        opinions: IGetOpinion[];
    }

    interface IGetOpinion { // Assuming this replaces IOpinion if not defined
        id: number;
        dateCreated: string;
        comment: string;
    }

    interface LeaveOpinion extends BaseOpinion {
        type: 'leave';
        leaveRequestId: number;
        status: LeaveRequestStatus;
    }

    interface IBulkUpdateStatusRequest {
        ids: number[];
        status: LeaveRequestStatus;
    }
}

// Crucial: This makes the file a module so 'declare global' works
export {};