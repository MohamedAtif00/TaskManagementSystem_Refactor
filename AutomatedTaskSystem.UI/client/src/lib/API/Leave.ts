import { url } from ".";

interface ILeave {
  startDate: string;
  endDate: string;
  reason?: string;
  type: LeaveRequestType;
}


type LeaveRequestType = "Annual Leave"|"Sick Leave"|"Emergency Leave"
type LeaveRequestStatus = "Pending"|"Approved"|"Rejected"

interface ICreateLeave extends ILeave {
  userId?: number;
}

interface IGetAllLeavesRequest extends ICreateLeave,ICreateLeave{
  name: string;
  duration: number;
  status: LeaveRequestStatus;
}

interface IGetLeaveRequest extends ILeave{
    id:number
    status: LeaveRequestStatus;
    duration:number
}



interface ResponseService {
  error: boolean;
  message: string;
}

interface ResponseServiceWithData<T> extends ResponseService {
  data: T;
}

const Leave = {
  GET_ALL_DB: async (): Promise<ResponseServiceWithData<IGetAllLeavesRequest[]> | false> => {
    try {
      const res = await fetch(`${url}/Leave`);
      const response: ResponseServiceWithData<IGetAllLeavesRequest[]> = await res.json();
      console.log(response);
      
      return response;
    } catch (error) {
      console.error(error);
      return false;
    }
  },

  CREATE: async (payload: ICreateLeave): Promise<boolean> => {
    try {
      const res = await fetch(`${url}/Leave`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      console.log(res);
      

      const data: boolean = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_SINGLE:async(leaveRequestId:number)=>{
    try {
      const res = await fetch(`${url}/Leave/${leaveRequestId}`);
      const data: ResponseServiceWithData<IGetLeaveRequest[]> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }

  },
  GET_ALL_BY_USER:async(userId:number)=>{
    try {
      const res = await fetch(`${url}/Leave/LeaveRequestByUserId/${userId}`);
      const data: ResponseServiceWithData<IGetLeaveRequest[]> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  }
};

// Use 'export type' for types to avoid the error when 'isolatedModules' is enabled.
export type { ILeave, ICreateLeave ,IGetLeaveRequest,LeaveRequestStatus,LeaveRequestType,IGetAllLeavesRequest};

export { Leave };
