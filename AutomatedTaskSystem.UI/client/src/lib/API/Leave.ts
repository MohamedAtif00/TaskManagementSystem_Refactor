import { url } from ".";

import authService from "../Auth";

interface ILeave {
  startDate: string;
  endDate: string;
  reason?: string;
  noteToManager?:string
  type: LeaveRequestType;
}



interface ICreateLeave extends ILeave {
  userId?: number;
  medicalCertificate?:File | null;
}



interface IGetAllLeavesRequest extends ICreateLeave,ICreateLeave{
  id:number
//   name: string;
  duration: number;
  status: LeaveRequestStatus;
  dateCreated:string;
  user:{id:number,name:string,role?:number} | null
}



interface IGetLeaveRequest extends ILeave{
    id:number
    status: LeaveRequestStatus;
    duration:number
    user:{id:number,name:string,role:number} | null
}


type LeaveRequestType = "Annual"|"Sick"|"Emergency"
type LeaveRequestStatus = "Pending"|"Approved"|"Rejected"|"Cancelled";


interface IOpinion{
    leaveRequestId:number,
    comment:string,
    status:LeaveRequestStatus
    isApproved?:boolean,
    user:{id:number,name:string,role:UserRole},

}
///////////////////////////////
// Calander  Models
interface IGetLeaveRequestForCalander extends ILeave{
    id:number,
    duration:number,
    status:LeaveRequestStatus,
    dateCreated:string,
    user:{id:number,name:string}
}


////////////////////////////////////
// Request Details
interface IGetLeaveRequestForDetails extends ILeave{
    id:number,
    duration:number,
    status:LeaveRequestStatus,
    dateCreated:string,
    user:IUser,
    opinions:IOpinion[]

}

interface IGetOpinion extends IOpinion{
    id:number,
    dateCreated:string,

}

///////////////////////////////////
// 




















// interface ResponseServiceWithData<T> extends ResponseService {
//   data: T;
// }

const LEAVE = {
  GET_ALL_DB: async (
  params?: Record<string, string | number | boolean>
): Promise<ResponseService<IGetAllLeavesRequest[]> | false> => {
  try {
    const query = params
      ? `?${new URLSearchParams(
          Object.entries(params).reduce((acc, [key, value]) => {
            acc[key] = String(value); // Ensure all values are strings
            return acc;
          }, {} as Record<string, string>)
        ).toString()}`
      : "";

    const res = await fetch(`${url}/Leave${query}`);    
    const response: ResponseService<IGetAllLeavesRequest[]> = await res.json();
    console.log(response);

    return response;
  } catch (error) {
    console.error(error);
    return false;
  }
  },
  CREATE: async (leaveData: ICreateLeave): Promise<ResponseService<boolean>> => {
    try {
      // Create FormData for file upload
      const formData = new FormData();
      
      // Add basic form fields
      if (leaveData?.userId !== undefined && leaveData?.userId !== null) {
          formData.append('UserId', leaveData.userId.toString());
        }

      formData.append('Type', leaveData.type);
      formData.append('StartDate', leaveData.startDate);
      formData.append('EndDate', leaveData.endDate);
      if (leaveData.reason !== undefined && leaveData.reason !== null) {
        formData.append('Reason', leaveData.reason);
      }


      
      // Add optional note for manager
      if (leaveData.noteToManager) {
        formData.append('NoteForManager', leaveData.noteToManager);
      }

      // Add medical certificate if provided
      if (leaveData.medicalCertificate) {
        formData.append('MedicalCertificate', leaveData.medicalCertificate);
      }

      // Add supporting documents if provided
      // if (leaveData.supportingDocuments && leaveData.supportingDocuments.length > 0) {
      //   leaveData.supportingDocuments.forEach((doc, index) => {
      //     formData.append('SupportingDocuments', doc);
      //   });
      // }

      const res = await fetch(`${url}/Leave`, {
        
        method: "POST",
        // Don't set Content-Type header - let browser set it with boundary for FormData
        body: formData,
      });

      if (!res.ok) {
        throw new Error(`HTTP error! status: ${res.status}`);
      }

      const data: boolean = await res.json();
      return data;
    } catch (error) {
      console.error('Error creating leave request:', error);
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
      const res = await fetch(`${url}/Leave/LeaveRequestsByUserId/${userId}`);
      const data: ResponseService<IGetLeaveRequest[]> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_LEAVE_BY_USER:async (userId:number)=>{
    try {
      const headers = authService.authHeader();
      const res = await fetch(`${url}/Leave/LeaveRequestByUserId/${userId}`,{
        headers: {
          "Content-Type": "application/json",
          ...headers
        }});

      const data: ResponseService<IGetLeaveRequestForDetails[]> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  CREATE_OPINION:async (opinion:IOpinion)=>{
     try {
        const auth = authService.authHeader();
      const res = await fetch(`${url}/Leave/CreateOpinion`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...auth
        },
        body: JSON.stringify(opinion),
      });
      

      const data: IGetOpinion = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_USER_INFO:async (userId:number)=>{
    try {
      const res = await fetch(`${url}/users/${userId}`);
      const data: ResponseServiceWithData<IUser> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_SICK_ATTACHMENT : async (leaveRequestId: number) => {
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

      // Trigger download
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

// Use 'export type' for types to avoid the error when 'isolatedModules' is enabled.
export type { 
    ILeave, 
    ICreateLeave ,
    IGetLeaveRequest,
    IGetLeaveRequestForCalander,
    IGetLeaveRequestForDetails,
    IGetAllLeavesRequest,
    LeaveRequestStatus,
    LeaveRequestType,
    IOpinion,
    IGetOpinion
};

export default LEAVE;
