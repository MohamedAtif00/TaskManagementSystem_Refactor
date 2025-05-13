import { url } from ".";

interface IPermission {
    id: number;
    userId?: number;
    type: PermissionType;  // Assuming `PermissionType` is an enum
    reason: string;
    permissionDate: string; // Date of the permission in string format or Date
    status?: PermissionRequestStatus;
}

interface IGetPermission extends IPermission{
    createdAt: string; // When the permission was created
}


type PermissionRequestStatus = "Pending"|"Approved"|"Rejected"


export enum PermissionType {
    Morning = "Morning",
    Afternoon = "Afternoon",
    FullDay = "FullDay",
    // You can add other permission types if needed
}

interface CreatePermissionDto {
    userId: number;
    type: PermissionType;
    date: string;
    reason: string;
}

interface ResponseService {
    error: boolean;
    message: string;
    data?: any;
}

const Permission = {
    async createPermission(createPermissionDto: CreatePermissionDto): Promise<IPermission | null> {
        try {
            const response = await fetch(`${url}/Permission`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(createPermissionDto),
            });

            if (!response.ok) {
                throw new Error('Failed to create permission');
            }

            const result: ResponseService = await response.json();
            
            if (result.error) {
                throw new Error(result.message || 'Failed to create permission');
            }

            return result.data;
        } catch (error) {
            console.error('Permission API Error:', error);
            return null;
        }
    },

    async getAllByUser(userId: number): Promise<IPermission[] | null> {
        try {
            const response = await fetch(`${url}/Permission/user/${userId}`);
            
            if (!response.ok) {
                throw new Error('Failed to fetch permissions');
            }

            const result: ResponseService = await response.json();
            
            if (result.error) {
                throw new Error(result.message || 'Failed to fetch permissions');
            }

            return result.data;
        } catch (error) {
            console.error('Permission API Error:', error);
            return null;
        }
    }
};

export type { CreatePermissionDto, IPermission ,IGetPermission};
export default Permission;
