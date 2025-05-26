// import { url } from ".";
// import authService from "../Auth";

// interface IPermission {
//     id: number;
//     type: PermissionType;  // Assuming `PermissionType` is an enum
//     reason: string;
//     permissionDate: string; // Date of the permission in string format or Date
//     status?: PermissionRequestStatus;
//     from:string;
//     to:string
// }

// interface IGetPermission extends IPermission{
//     createdAt: string; // When the permission was created
//     user:{id:number,name:string}
//     dateCreated:string
// }


// type PermissionRequestStatus = "Pending"|"Approved"|"Rejected"


// // Update your enum to match the options in the image
// export enum PermissionType {
//   EarlyDeparture = "EarlyDeparture", // انصراف مبكر
//   LateArrival = "LateArrival",      // تأخير
//   WorkAssignment = "WorkAssignment", // مأمورية عمل
//   Departure = "departure"                    // انصراف (departure)
// }
// interface CreatePermissionDto {
//     userId: number;
//     type: PermissionType;
//     date: string;
//     from: string,   
//     to: string,
//     reason: string;
// }

// // interface ResponseService {
// //     error: boolean;
// //     message: string;
// //     data?: any;
// // }

// const PERMISSION = {
//     async createPermission(request: CreatePermissionDto): Promise<IPermission | null> {
//     try {
//         console.log("Sending request:", request); // Log the actual payload
        
//         const authHeader = authService.authHeader();
//         const response = await fetch(`${url}/Permission`, {
//             method: 'POST',
//             headers: {
//                 'Content-Type': 'application/json',
//                 ...authHeader
//             },
//             body: JSON.stringify(request), // Remove the {request} wrapper
//         });

//         if (!response.ok) {
//             const errorData = await response.json();
//             console.error("API Error:", errorData);
//             throw new Error(errorData.message || 'Failed to create permission');
//         }

//         const result: ResponseService = await response.json();
//         return result.data;
//     } catch (error) {
//         console.error('Permission API Error:', error);
//         return null;
//     }
// },
//     async GET_ALL(): Promise<ResponseService<IGetPermission[] | null>> {
//         try {
//             const response = await fetch(`${url}/Permission`);
            
//             if (!response.ok) {
//                 throw new Error('Failed to fetch all permissions');
//             }

//             const result: ResponseService<IGetPermission[]> = await response.json();
            
//             if (result.error) {
//                 throw new Error(result.message || 'Failed to fetch all permissions');
//             }

//             return result.data;
//         } catch (error) {
//             console.error('Permission API Error:', error);
//             return null;
//         }
//     },

//     async getAllByUser(userId: number): Promise<IGetPermission[] | null> {
//         try {
//             const response = await fetch(`${url}/Permission/${userId}`);
            
//             if (!response.ok) {
//                 throw new Error('Failed to fetch permissions');
//             }

//             const result: ResponseService = await response.json();
            
//             if (result.error) {
//                 throw new Error(result.message || 'Failed to fetch permissions');
//             }

//             return result.data;
//         } catch (error) {
//             console.error('Permission API Error:', error);
//             return null;
//         }
//     },
//     async GET_BY_SAME_USER():Promise<IPermission[] | null>
//     {
//         try {
//             const response = await fetch(`${url}/Permission/GetPermissionsForSameUser`);
            
//             if (!response.ok) {
//                 throw new Error('Failed to fetch permissions');
//             }

//             const result: ResponseService = await response.json();
            
//             if (result.error) {
//                 throw new Error(result.message || 'Failed to fetch permissions');
//             }

//             return result.data;
//         } catch (error) {
//             console.error('Permission API Error:', error);
//             return null;
//         }
//     }
// };

// export type { CreatePermissionDto, IPermission ,IGetPermission,PermissionRequestStatus};
// export default PERMISSION;


import { url } from ".";
import authService from "../Auth";

interface IPermission {
  id: number;
  type: PermissionType;
  permissionDate: string;  // Changed from 'date'
  fromTime: string;        // Changed from 'from'
  toTime: string;          // Changed from 'to'
  reason: string;
  duration: number;
  status: PermissionRequestStatus;
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
  reason: string;
  PermissionDate:string;
}

interface IGetPermissionDetails {
  id: number;
  type: PermissionType;
  permissionDate: string;
  fromTime: string; 
  toTime: string;
  reason: string;
  noteToManager:string;
  duration: number;
  status: PermissionRequestStatus;
  user?: IUser;  // Now you can make it optional and use IUser
  dateCreated: string;
  opinions?: IOpinion[];
}

type PermissionRequestStatus = "Pending" | "Approved" | "Rejected" | "Cancelled";

export enum PermissionType {
  EarlyDeparture = "EarlyDeparture",
  LateArrival = "LateArrival",
  WorkAssignment = "WorkAssignment",
  Departure = "Departure"
}

interface IOpinion {
  permissionId: number;
  comment: string;
  status: PermissionRequestStatus;
  isApproved?: boolean;
  user?: { id: number; name: string; role: number };
}

interface IGetOpinion extends IOpinion {
  id: number;
  dateCreated: string;
}



const PERMISSION = {
  GET_ALL: async (
    params?: Record<string, string | number | boolean>
  ): Promise<ResponseService<IPermission[]> | false> => {
    try {
      const query = params
        ? `?${new URLSearchParams(
            Object.entries(params).reduce((acc, [key, value]) => {
              acc[key] = String(value);
              return acc;
            }, {} as Record<string, string>)
          ).toString()}`
        : "";

      const res = await fetch(`${url}/Permission${query}`);
      const response: ResponseService<IPermission[]> = await res.json();
      return response;
    } catch (error) {
      console.error(error);
      return false;
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

      const data: boolean = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_SINGLE: async (permissionId: number): Promise<ResponseService<IPermission> | false> => {
    try {
      const res = await fetch(`${url}/Permission/${permissionId}`);
      const data: ResponseService<IPermission> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_ALL_BY_USER: async (userId: number): Promise<ResponseService<IPermission[]> | false> => {
    try {
      const res = await fetch(`${url}/Permission/PermissionsByUserId/${userId}`);
      const data: ResponseService<IPermission[]> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  GET_DETAILS: async (permissionId: number): Promise<ResponseService<IGetPermissionDetails> | false> => {
    try {
      const res = await fetch(`${url}/Permission/GetSinglePermission/${permissionId}`);
      const data: ResponseService<IGetPermissionDetails> = await res.json();
      return data;
    } catch (error) {
      console.error(error);
      return false;
    }
  },
  CREATE_OPINION: async (opinion: IOpinion): Promise<ResponseService<IGetOpinion> | false> => {
    try {
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
    } catch (error) {
      console.error(error);
      return false;
    }
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

export type { 
  IPermission, 
  ICreatePermission,
  IGetPermissionDetails,
  PermissionRequestStatus,
  IOpinion,
  IGetOpinion
};


export default PERMISSION;