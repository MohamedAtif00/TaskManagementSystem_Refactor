import { useState, useEffect, useCallback, useMemo } from "react";
import { format } from "date-fns";
import LEAVE, { IGetAllLeavesRequestNoPagination, IGetLeaveRequestForCalander, IOpinion, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import PERMISSION, { IPermission, PermissionRequestStatus, PermissionType } from "../../lib/API/Permission";
import ExportButton from "../../components/button/ExportButton"; // This is likely for client-side export
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import DataTable from "../../components/table/tablePagination";
import ServerExportButton from "../../components/button/serverExportButtonProps";
import WORK_FROM_HOME, { WorkFromHomeStatus, IGetAllWorkFromHomeRequestNoPagination, IGetWorkFromHomeRequest, IDName } from "../../lib/API/workFromHome";
import { toast } from "react-toastify";
import EyeIcon from "../../assets/Icons/Eye";
import CheckIcon from "../../assets/Icons/Check";
import XIcon from "../../assets/Icons/x";

// Assume these interfaces are imported or defined globally if they are not part of LEAVE/PERMISSION
// Re-declaring for clarity within this file's context, but ideally these would be in a shared types file.
interface ResponseService<T> {
    data?: T;
    error?: boolean;
    message?: string;
}

interface PageList<T> {
    items: T;
    page: number;
    pageSize: number;
    totalPages: number;
    totalCount: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

// Interfaces for Leave (already present in your code)
interface IGetAllLeavesRequest {
    userId?: number;
    role?: number;
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    fromDate?: string;
    toDate?: string;
    status?: LeaveRequestStatus | "all";
    myStatus?: LeaveRequestStatus | "all"
    type?: LeaveRequestType | "all";
    [key: string]: string | number | boolean | undefined;
}

// Interfaces for Permission (already present in your code)
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
    role: number;
    date?: string;
    type?: PermissionType | "all";
    status?: PermissionRequestStatus | "all";
    [key: string]: string | number | boolean | undefined;
}

interface IGetAllPermissionsRequestNoPagination {
    userId?: number;
    role: number;
    searchTerm?: string;
    date?: string;
    type?: PermissionType | "all";
    status?: PermissionRequestStatus | "all";
    myStatus?: PermissionRequestStatus | "all";
    disablePagination: true;
}


// New Interfaces for Work From Home (corresponding to backend DTOs)
// These should ideally come from your 'types/workFromHomeTypes.d.ts' as suggested in the previous response
interface IGetAllWorkFromHomeRequest {
    userId?: number;
    role?: number;
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    fromDate?: string; // For filtering by date range
    toDate?: string; // For filtering by date range
    status?: WorkFromHomeStatus | "all";
    myStatus?: WorkFromHomeStatus | "all"; // Manager's opinion status
    [key: string]: string | number | boolean | undefined;
}

// Assuming the IGetWorkFromHomeRequest is similar to IGetLeaveRequestForCalander
// but for WFH, it would have 'date' instead of 'startDate'/'endDate' and 'duration'
interface IGetWorkFromHomeRequestForCalanderDisplay {
    id: number;
    date: string; // Single date for WFH
    noteForManager?: string;
    status: WorkFromHomeStatus;
    myStatus?: WorkFromHomeStatus; // Manager's opinion status
    dateCreated: string;
    user: IDName | null;
}


const Calendar = () => {

    //#region  States and variables 

    const [activeTab, setActiveTab] = useState<"vacancy" | "permission" | "workFromHome">("vacancy"); // Added 'workFromHome'
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [vacancies, setVacancies] = useState<IGetLeaveRequestForCalander[]>([]);
    const [permissions, setPermissions] = useState<IPermission[]>([]);
    const [workFromHomeRequests, setWorkFromHomeRequests] = useState<IGetWorkFromHomeRequestForCalanderDisplay[]>([]); // New state for WFH

    const [totalVacanciesCount, setTotalVacanciesCount] = useState<number>(0);
    const [totalPermissionsCount, setTotalPermissionsCount] = useState<number>(0);
    const [totalWorkFromHomeCount, setTotalWorkFromHomeCount] = useState<number>(0); // New state for WFH count

    const [loading, setLoading] = useState({
        vacancies: true,
        permissions: true,
        workFromHome: true, // New loading state for WFH
    });

    // Pagination & Filter state for Vacancies
    const [vacancyCurrentPage, setVacancyCurrentPage] = useState<number>(1);
    const [vacancyItemsPerPage, setVacancyItemsPerPage] = useState<number>(10);
    const [vacancyFilters, setVacancyFilters] = useState<Record<string, string | number | undefined>>({
        fromDate: undefined,
        toDate: undefined,
        status: "all",
        type: "all"
    });
    const [vacancySearchText, setVacancySearchText] = useState<string>("");

    // Pagination & Filter state for Permissions
    const [permissionCurrentPage, setPermissionCurrentPage] = useState<number>(1);
    const [permissionItemsPerPage, setPermissionItemsPerPage] = useState<number>(10);
    const [permissionFilters, setPermissionFilters] = useState<Record<string, string | number | undefined>>({
        date: undefined,
        type: "all",
        status: "all"
    });
    const [permissionSearchText, setPermissionSearchText] = useState<string>("");

    // Pagination & Filter state for Work From Home (NEW)
    const [workFromHomeCurrentPage, setWorkFromHomeCurrentPage] = useState<number>(1);
    const [workFromHomeItemsPerPage, setWorkFromHomeItemsPerPage] = useState<number>(10);
    const [workFromHomeFilters, setWorkFromHomeFilters] = useState<Record<string, string | number | undefined>>({
        fromDate: undefined, // WFH can also have a date range filter
        toDate: undefined,
        status: "all",
    });
    const [workFromHomeSearchText, setWorkFromHomeSearchText] = useState<string>("");

    // Separate selection states for each tab
    const [vacancySelectedRows, setVacancySelectedRows] = useState(new Set<string | number>());
    const [permissionSelectedRows, setPermissionSelectedRows] = useState(new Set<string | number>());
    const [workFromHomeSelectedRows, setWorkFromHomeSelectedRows] = useState(new Set<string | number>());


    const auth = useAppSelector((s) => s.authSlice);
    //#endregion

    // Helper functions to get current selection state and setter based on active tab
    const getCurrentSelectedRows = useCallback(() => {
        switch (activeTab) {
            case 'vacancy':
                return vacancySelectedRows;
            case 'permission':
                return permissionSelectedRows;
            case 'workFromHome':
                return workFromHomeSelectedRows;
            default:
                return new Set<string | number>();
        }
    }, [activeTab, vacancySelectedRows, permissionSelectedRows, workFromHomeSelectedRows]);

    const setCurrentSelectedRows = useCallback((newSelection: Set<string | number>) => {
        switch (activeTab) {
            case 'vacancy':
                setVacancySelectedRows(newSelection);
                break;
            case 'permission':
                setPermissionSelectedRows(newSelection);
                break;
            case 'workFromHome':
                setWorkFromHomeSelectedRows(newSelection);
                break;
        }
    }, [activeTab]);

    const handleSelectionChange = useCallback((newSelection: Set<string | number>) => {
        debugger
        // Determine the correct data source based on the active tab
        const dataMap = {
            vacancy: vacancies,
            permission: permissions,
            workFromHome: workFromHomeRequests,
        };
        const currentData = dataMap[activeTab];

        // Create a new selection set that will contain only valid pending items
        const validSelection = new Set<string | number>();
        const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1;

        // Process each item in the new selection
        newSelection.forEach(id => {
            // Find the item in the current data source
            const item = currentData.find(x => x.id == id);
            if (item) {
                // For privileged users, their actionable items are those with 'myStatus' as Pending.
                // If 'myStatus' isn't available, it falls back to the overall 'status'.
                // For other users, it's just the overall 'status'.
                const statusToCheck = isPrivilegedUser ? (item.myStatus ?? item.status) : item.status;

                // Only add the item to the selection if its status is "Pending"
                if (statusToCheck === "Pending") {
                    validSelection.add(id);
                }
            }
        });

        // Handle deselection: if an item was previously selected but is not in newSelection,
        // it means the user unchecked it, so we should respect that action
        // The validSelection already contains only the items that should remain selected

        setCurrentSelectedRows(validSelection);
    }, [activeTab, vacancies, permissions, workFromHomeRequests, auth.role, setCurrentSelectedRows]);

    // Function to determine if a row can be selected (only pending items)
    const isRowSelectable = useCallback((row: Record<string, any>) => {
        const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1;

        // Get the status to check based on user role
        const statusToCheck = isPrivilegedUser ? (row["My Status"] ?? row["Final Status"]) : row["Final Status"];

        // Only pending items can be selected
        return statusToCheck === "Pending";
    }, [auth.role]);

    const handleBulkOpinion = async (status: 'Approved' | 'Rejected') => {
        const currentSelection = getCurrentSelectedRows();
        const selectedIds = Array.from(currentSelection).map(id => Number(id));
        if (selectedIds.length === 0) {
            toast.warn("No rows selected for bulk action.");
            return;
        }

        setIsSubmitting(true);
        toast.info(`Submitting bulk ${status.toLowerCase()} for ${selectedIds.length} items...`);

        try {
            let response: ResponseService<any> | undefined;
            const opinionStatus = status === 'Approved' ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;

            // NOTE: The following API calls are assumed to exist and need to be implemented in your API service files.
            // They should accept an object with an array of IDs and the new status.
            switch (activeTab) {
                case 'vacancy':
                    response = await LEAVE.BULK_UPDATE_STATUS({ ids: selectedIds, status: opinionStatus });
                    break;
                case 'permission':
                    response = await PERMISSION.BULK_UPDATE_STATUS({ ids: selectedIds, status: opinionStatus as unknown as PermissionRequestStatus });
                    break;
                // case 'workFromHome':
                //     response = await WORK_FROM_HOME.BULK_UPDATE_STATUS({ ids: selectedIds, status: opinionStatus as unknown as WorkFromHomeStatus });
                //     break;
            }

            if (response && !response.error) {
                toast.success(response.message || `Successfully ${status.toLowerCase()}ed ${selectedIds.length} requests.`);
                setCurrentSelectedRows(new Set()); // Clear selection after action
                refetchData(); // Refresh the data in the table
            } else {
                toast.error(response?.message || `Failed to submit bulk action. Please ensure you have the correct permissions.`);
            }
        } catch (error) {
            toast.error(`An error occurred during the bulk ${status.toLowerCase()} process.`);
            console.error(`Error during bulk ${status.toLowerCase()} for tab ${activeTab}:`, error);
        } finally {
            setIsSubmitting(false);
        }
    };

    // fetch all


     const fetchVacancies = useCallback(async (page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setLoading(prev => ({ ...prev, vacancies: true }));
        try {
            const params: IGetAllLeavesRequest = {
                page: page,
                pageSize: itemsPerPage,
                searchTerm: searchText === "" ? undefined : searchText,
                fromDate: filters.fromDate as string | undefined,
                toDate: filters.toDate as string | undefined,
                status: filters.status === "all" ? undefined : filters.status as LeaveRequestStatus,
                type: filters.type === "all" ? undefined : filters.type as LeaveRequestType,
                myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as LeaveRequestStatus : undefined,
            };

            const response = await LEAVE.GET_ALL_DB(params as Record<string, string | number | boolean | undefined>);

            if (response && response.data) {
                setVacancies(response.data.items || []);
                setTotalVacanciesCount(response.data.totalCount || 0);
            } else {
                console.error("Invalid response format or error from LEAVE.GET_ALL_DB:", response);
                setVacancies([]);
                setTotalVacanciesCount(0);
            }
        } catch (error) {
            console.error("Error fetching vacancies:", error);
            setVacancies([]);
            setTotalVacanciesCount(0);
        } finally {
            setLoading(prev => ({ ...prev, vacancies: false }));
        }
    }, [auth.id, auth.role]);
    

    const fetchPermissions = useCallback(async (page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setLoading(prev => ({ ...prev, permissions: true }));
        try {
            const params: IGetAllPermissionsRequest = {
                page: page,
                pageSize: itemsPerPage,
                role: auth.role,
                searchTerm: searchText === "" ? undefined : searchText,
                date: filters.date as string | undefined,
                type: filters.type === "all" ? undefined : filters.type as PermissionType,
                status: filters.status === "all" ? undefined : filters.status as PermissionRequestStatus,
                myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as PermissionRequestStatus : undefined,
            };

            const response = await PERMISSION.GET_ALL(params as Record<string, string | number | boolean | undefined>);

            if (response && response.data && !response.error) {
                setPermissions(response.data.items || []);
                setTotalPermissionsCount(response.data.totalCount || 0);
            } else {
                console.error("Invalid response format or error from PERMISSION.GET_ALL:", response);
                setPermissions([]);
                setTotalPermissionsCount(0);
            }
        } catch (error) {
            console.error("Error fetching permissions:", error);
            setPermissions([]);
            setTotalPermissionsCount(0);
        } finally {
            setLoading(prev => ({ ...prev, permissions: false }));
        }
    }, [auth.role]);

    // New: Fetch function for Work From Home requests
    const fetchWorkFromHome = useCallback(async (page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setLoading(prev => ({ ...prev, workFromHome: true }));
        try {
            const params: IGetAllWorkFromHomeRequest = {
                page: page,
                pageSize: itemsPerPage,
                searchTerm: searchText === "" ? undefined : searchText,
                fromDate: filters.fromDate as string | undefined,
                toDate: filters.toDate as string | undefined,
                status: filters.status === "all" ? undefined : filters.status as WorkFromHomeStatus,
                myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as WorkFromHomeStatus : undefined,
            };

            const response = await WORK_FROM_HOME.GET_ALL(params as Record<string, string | number | boolean | undefined>);

            if (response && response.data && !response.error) {
                setWorkFromHomeRequests(response.data.items || []); // Assuming response.data.items matches IGetWorkFromHomeRequestForCalanderDisplay
                setTotalWorkFromHomeCount(response.data.totalCount || 0);
            } else {
                console.error("Invalid response format or error from WORK_FROM_HOME.GET_ALL:", response);
                setWorkFromHomeRequests([]);
                setTotalWorkFromHomeCount(0);
            }
        } catch (error) {
            console.error("Error fetching work from home requests:", error);
            setWorkFromHomeRequests([]);
            setTotalWorkFromHomeCount(0);
        } finally {
            setLoading(prev => ({ ...prev, workFromHome: false }));
        }
    }, [auth.role]);
    //

    const formatDateForDisplay = useCallback((dateStr: string | Date) => {
        if (!dateStr) return "N/A";
        try {
            const date = new Date(dateStr);
            return format(date, "dd MMM yyyy");
        } catch (e) {
            console.error("Date formatting error:", e);
            return String(dateStr);
        }
    }, []);

     var refetchData = useCallback(() => {
    if (activeTab === 'vacancy') {
        fetchVacancies(vacancyCurrentPage, vacancyItemsPerPage, vacancyFilters, vacancySearchText);
    } else if (activeTab === 'permission') {
        fetchPermissions(permissionCurrentPage, permissionItemsPerPage, permissionFilters, permissionSearchText);
    } else if (activeTab === 'workFromHome') {
        fetchWorkFromHome(workFromHomeCurrentPage, workFromHomeItemsPerPage, workFromHomeFilters, workFromHomeSearchText);
    }
    }, [
        activeTab,
        fetchVacancies, vacancyCurrentPage, vacancyItemsPerPage, vacancyFilters, vacancySearchText,
        fetchPermissions, permissionCurrentPage, permissionItemsPerPage, permissionFilters, permissionSearchText,
        fetchWorkFromHome, workFromHomeCurrentPage, workFromHomeItemsPerPage, workFromHomeFilters, workFromHomeSearchText
    ]);

    const transformedVacancyData = useMemo(() => {
        const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1; // Project Manager or Team Leader
        return vacancies.map(vacancy => {
            const baseData: Record<string, any> = {
                "User name": vacancy.user?.name ?? "N/A",
                "Request date": formatDateForDisplay(vacancy.dateCreated),
                "Vacancy type": vacancy.type,
                "Final Status": vacancy.status,
                "Vacancy date": formatDateForDisplay(vacancy.startDate),
            };
            if (isPrivilegedUser) {
                baseData["My Status"] = vacancy.myStatus ?? vacancy.status;
            }
            baseData["Actions"] = vacancy.id;
            return baseData;
        });
    }, [vacancies, formatDateForDisplay, auth.role]);

    const transformedPermissionData = useMemo(() => {
        const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1; // Project Manager or Team Leader
        return permissions.map(permission => {
            const baseData: Record<string, any> = {
                "User name": permission.user?.name ?? "N/A",
                "Request date": formatDateForDisplay(permission.dateCreated),
                "Permission Type": permission.type,
                "Final Status": permission.status,
                "Permission date": formatDateForDisplay(permission.permissionDate),
            };
            if (isPrivilegedUser) {
                baseData["My Status"] = permission.myStatus ?? permission.status;
            }
            baseData["Actions"] = permission.id;
            return baseData;
        });
    }, [permissions, formatDateForDisplay, auth.role]);

    // New: Transformed data for Work From Home requests
    const transformedWorkFromHomeData = useMemo(() => {
        const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1; // Project Manager or Team Leader
        return workFromHomeRequests.map(wfh => {
            const baseData: Record<string, any> = {
                "User name": wfh.user?.name ?? "N/A",
                "Request date": formatDateForDisplay(wfh.dateCreated),
                "Work From Home Date": formatDateForDisplay(wfh.date), // Single date for WFH
                "Final Status": wfh.status,
                // Optional: "Note for Manager": wfh.noteForManager, if you want to display this
            };
            if (isPrivilegedUser) {
                baseData["My Status"] = wfh.myStatus ?? wfh.status;
            }
            baseData["Actions"] = wfh.id;
            return baseData;
        });
    }, [workFromHomeRequests, formatDateForDisplay, auth.role]);


    const vacancyFilterConfig = useMemo(() => {
        const config = [
            {
                key: "fromDate",
                label: "Vacancy From",
                type: "date" as const
            },
            {
                key: "toDate",
                label: "Vacancy To",
                type: "date" as const
            },
            {
                key: "status",
                label: "Final Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: LeaveRequestStatus.Pending, label: "Pending" },
                    { value: LeaveRequestStatus.Approved, label: "Approved" },
                    { value: LeaveRequestStatus.Rejected, label: "Rejected" },
                    { value: LeaveRequestStatus.Cancelled, label: "Cancelled" }
                ]
            },
            {
                key: "type",
                label: "Type",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: LeaveRequestType.Annual, label: "Annual" },
                    { value: LeaveRequestType.Sick, label: "Sick" },
                    { value: LeaveRequestType.Emergency, label: "Emergency" }
                ]
            }
        ];
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            config.push({
                key: "myStatus",
                label: "My Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: LeaveRequestStatus.Pending, label: "Pending" },
                    { value: LeaveRequestStatus.Approved, label: "Approved" },
                    { value: LeaveRequestStatus.Rejected, label: "Rejected" },
                    { value: LeaveRequestStatus.Cancelled, label: "Cancelled" }
                ]
            });
        }
        return config;
    }, [auth.role]);

    const permissionFilterConfig = useMemo(() => {
        const config = [
            {
                key: "date",
                label: "Permission Date",
                type: "date" as const
            },
            {
                key: "type",
                label: "Type",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: PermissionType.EarlyDeparture, label: "Early Departure" },
                    { value: PermissionType.LateArrival, label: "Late Arrival" },
                    { value: PermissionType.WorkAssignment, label: "Work Assignment" },
                    { value: PermissionType.Departure, label: "Departure" }
                ]
            },
            {
                key: "status",
                label: "Final Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: PermissionRequestStatus.Pending, label: "Pending" },
                    { value: PermissionRequestStatus.Approved, label: "Approved" },
                    { value: PermissionRequestStatus.Rejected, label: "Rejected" },
                    { value: PermissionRequestStatus.Cancelled, label: "Cancelled" }
                ]
            }
        ];
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            config.push({
                key: "myStatus",
                label: "My Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: PermissionRequestStatus.Pending, label: "Pending" },
                    { value: PermissionRequestStatus.Approved, label: "Approved" },
                    { value: PermissionRequestStatus.Rejected, label: "Rejected" },
                    { value: PermissionRequestStatus.Cancelled, label: "Cancelled" }
                ]
            });
        }
        return config;
    }, [auth.role]);

    // New: Filter config for Work From Home requests
    const workFromHomeFilterConfig = useMemo(() => {
        const config = [
            {
                key: "fromDate",
                label: "WFH From",
                type: "date" as const
            },
            {
                key: "toDate",
                label: "WFH To",
                type: "date" as const
            },
            {
                key: "status",
                label: "Final Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: WorkFromHomeStatus.Pending, label: "Pending" },
                    { value: WorkFromHomeStatus.Approved, label: "Approved" },
                    { value: WorkFromHomeStatus.Rejected, label: "Rejected" },
                    { value: WorkFromHomeStatus.Cancelled, label: "Cancelled" }
                ]
            }
        ];
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            config.push({
                key: "myStatus",
                label: "My Status",
                type: "select" as const,
                options: [
                    { value: "all", label: "All" },
                    { value: WorkFromHomeStatus.Pending, label: "Pending" },
                    { value: WorkFromHomeStatus.Approved, label: "Approved" },
                    { value: WorkFromHomeStatus.Rejected, label: "Rejected" },
                    { value: WorkFromHomeStatus.Cancelled, label: "Cancelled" }
                ]
            });
        }
        return config;
    }, [auth.role]);

    const commonStatusRenderer = useCallback((value: string) => {
        const getStatusColor = (status: string) => {
            switch (status?.toLowerCase()) {
                case 'approved':
                case 'accepted':
                    return 'text-green-500';
                case 'rejected':
                    return 'text-red-500';
                case 'pending':
                    return 'text-blue-500';
                case 'cancelled':
                    return 'text-red-600 bg-red-100 p-2 rounded ';
                default:
                    return '';
            }
        };
        return <span className={`font-medium ${getStatusColor(value)}`}>{value}</span>;
    }, []);


type RequestType = "vacancy" | "permission" | "workFromHome";

// ... (existing LeaveRequestStatus, PermissionRequestStatus enums)

    const handleSubmitOpinion = useCallback(async (
        id: number,
        status: LeaveRequestStatus | PermissionRequestStatus | WorkFromHomeStatus,
        type: RequestType
        ) => {
        setIsSubmitting(true);
        try {
            const commonFields = {
            comment: "",
            isApproved: status === LeaveRequestStatus.Approved || 
                        status === PermissionRequestStatus.Approved ||
                        status === WorkFromHomeStatus.Approved,
            user: { id: auth.id, name: auth.name, role: auth.role }
            };

            let op: IOpinion;
            switch (type) {
            case 'vacancy':
                op = {
                type: 'leave',
                leaveRequestId: id,
                status: status as LeaveRequestStatus,
                ...commonFields
                };
                break;
            case 'permission':
                op = {
                type: 'permission',
                permissionId: id,
                status: status as PermissionRequestStatus,
                ...commonFields
                };
                break;
            case 'workFromHome':
                op = {
                type: 'workFromHome',
                workFromHomeId: id,
                status: status as WorkFromHomeStatus,
                ...commonFields
                };
                break;
            default:
                throw new Error(`Unknown request type: ${type}`);
            }

            let response;
            switch (op.type) {
            case 'leave':
                response = await LEAVE.CREATE_OPINION(op);
                break;
            case 'permission':
                response = await PERMISSION.CREATE_OPINION(op);
                break;
            case 'workFromHome':
                response = await WORK_FROM_HOME.CREATE_OPINION(op);
                break;
            }

            if (response && !response.error) {
            toast.success(response.message || "Opinion submitted successfully!");
            refetchData();
            } else {
            toast.error(response?.message || "Failed to submit opinion.");
            }
        } catch (error) {
            toast.error("An error occurred while submitting your opinion.");
            console.error('Error submitting opinion:', error);
        } finally {
            setIsSubmitting(false);
        }
        }, [auth.id, auth.name, auth.role, refetchData]);


    const commonActionsRenderer = useCallback((id: number, path: string, currentStatus: LeaveRequestStatus | PermissionRequestStatus | WorkFromHomeStatus) => (
        <div className="flex gap-2">
            {currentStatus === LeaveRequestStatus.Pending || 
            currentStatus === PermissionRequestStatus.Pending ||
            currentStatus === WorkFromHomeStatus.Pending ? (
                <div className="flex gap-1">
                    <button 
                        onClick={() => {
                            // Determine the approved status based on activeTab
                            const approvedStatus = 
                                activeTab === "vacancy" ? LeaveRequestStatus.Approved :
                                activeTab === "permission" ? PermissionRequestStatus.Approved :
                                activeTab === "workFromHome" ? WorkFromHomeStatus.Approved :
                                LeaveRequestStatus.Approved; // default fallback
                            
                            handleSubmitOpinion(id, approvedStatus , activeTab);
                        }} 
                        disabled={isSubmitting} 
                        className="bg-green-600 rounded p-2 text-white disabled:bg-gray-400"
                    >
                        <CheckIcon/>
                    </button>
                    <button 
                        onClick={() => {
                            // Determine the rejected status based on activeTab
                            const rejectedStatus = 
                                activeTab === "vacancy" ? LeaveRequestStatus.Rejected :
                                activeTab === "permission" ? PermissionRequestStatus.Rejected :
                                activeTab === "workFromHome" ? WorkFromHomeStatus.Rejected :
                                LeaveRequestStatus.Rejected; // default fallback
                            
                            handleSubmitOpinion(id, rejectedStatus, activeTab);
                        }} 
                        disabled={isSubmitting} 
                        className="bg-red-600 rounded p-2 text-white disabled:bg-gray-400"
                    >
                        <XIcon/>
                    </button>
                </div>
            ) : null}
            <Link href={`/calendar/${path}/${id}`} className="text-gray-600 hover:text-gray-900 transition-colors">
                <button className="bg-blue-600 rounded p-2 text-white"><EyeIcon /></button>
            </Link>
        </div>
    ), [handleSubmitOpinion, isSubmitting, activeTab]);

   

    const vacancyColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            // FIX: Make 'row' optional and handle it, then pass the specific status
            "Actions": (value: number, row?: Record<string, any>) => {
                let status ;
            if(auth.role == 4)
                status =  row ? row["Final Status"]  : undefined;
            else
                status =  row ?  row["My Status"] : undefined;
                return commonActionsRenderer(value, 'vacancy', status);
            }
        };
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);


    const permissionColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            "Actions": (value: number, row?: Record<string, any>) => {
                let status ;
            if(auth.role == 4)
                status =  row ? row["Final Status"]  : undefined;
            else
                status =  row ?  row["My Status"] : undefined;
                return commonActionsRenderer(value, 'permission', status);
            }
        };
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);

    // New: Column renderers for Work From Home requests
    const workFromHomeColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            "Actions": (value: number, row?: Record<string, any>) => {
                let status ;
            if(auth.role == 4)
                status =  row ? row["Final Status"]  : undefined;
            else
                status =  row ?  row["My Status"] : undefined;
                return commonActionsRenderer(value, 'workFromHome', status);
            }
        };
        if (auth.role === 0 || auth.role === 2 || auth.role === 1) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);


    // Use useEffect to trigger data fetches when pagination/filter states change
    useEffect(() => {
        if (activeTab === "vacancy") {
            fetchVacancies(vacancyCurrentPage, vacancyItemsPerPage, vacancyFilters, vacancySearchText);
        }
    }, [activeTab, vacancyCurrentPage, vacancyItemsPerPage, vacancyFilters, vacancySearchText, fetchVacancies]);

    useEffect(() => {
        if (activeTab === "permission") {
            fetchPermissions(permissionCurrentPage, permissionItemsPerPage, permissionFilters, permissionSearchText);
        }
    }, [activeTab, permissionCurrentPage, permissionItemsPerPage, permissionFilters, permissionSearchText, fetchPermissions]);

    useEffect(() => {
        if (activeTab === "workFromHome") { // New useEffect for WFH
            fetchWorkFromHome(workFromHomeCurrentPage, workFromHomeItemsPerPage, workFromHomeFilters, workFromHomeSearchText);
        }
    }, [activeTab, workFromHomeCurrentPage, workFromHomeItemsPerPage, workFromHomeFilters, workFromHomeSearchText, fetchWorkFromHome]);


    // Handle tab change (preserve filters and pagination state)
    const handleTabChange = useCallback((tab: "vacancy" | "permission" | "workFromHome") => {
        setActiveTab(tab);
        // setSelectedRows(new Set()); // Clear selection on tab change
        // Note: We no longer reset pagination/filters to preserve user's filter state
        // Each tab maintains its own filter state independently
    }, []);

    const handleVacancyDataTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setVacancyCurrentPage(page);
        setVacancyItemsPerPage(itemsPerPage);
        setVacancyFilters(filters);
        setVacancySearchText(searchText);
    }, []);

    const handlePermissionDataTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setPermissionCurrentPage(page);
        setPermissionItemsPerPage(itemsPerPage);
        setPermissionFilters(filters);
        setPermissionSearchText(searchText);
    }, []);

    // New: Data Table change handler for Work From Home
    const handleWorkFromHomeDataTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setWorkFromHomeCurrentPage(page);
        setWorkFromHomeItemsPerPage(itemsPerPage);
        setWorkFromHomeFilters(filters);
        setWorkFromHomeSearchText(searchText);
    }, []);

    // --- Data Fetching Functions for ServerExportButton ---
    const getVacanciesForExport = useCallback(async () => {
        const params: IGetAllLeavesRequestNoPagination = {
            userId: auth.id,
            role: auth.role,
            searchTerm: vacancySearchText === "" ? undefined : vacancySearchText,
            fromDate: vacancyFilters.fromDate as string | undefined,
            toDate: vacancyFilters.toDate as string | undefined,
            status: vacancyFilters.status === "all" ? undefined : vacancyFilters.status as LeaveRequestStatus,
            type: vacancyFilters.type === "all" ? undefined : vacancyFilters.type as LeaveRequestType,
            disablePagination: true,
            myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && vacancyFilters.myStatus && vacancyFilters.myStatus !== "all" ? vacancyFilters.myStatus as LeaveRequestStatus : undefined,
        };
        const response = await LEAVE.GET_ALL_FOR_EXPORT(params);

        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1;
            return response.data.items.map(item => {
                const exportItem: Record<string, any> = {
                    "Id": item.id,
                    "User Name": item.user?.name ?? "N/A",
                    "Request Date": formatDateForDisplay(item.dateCreated),
                    "Start Date": formatDateForDisplay(item.startDate),
                    "End Date": formatDateForDisplay(item.endDate),
                    "Duration": item.duration,
                    "Type": item.type,
                    "Status": item.status,
                    "Reason": item.reason,
                };
                if (isPrivilegedUser) exportItem["My Status"] = item.myStatus ?? item.status;
                return exportItem;
            });
        }
        return [];
    }, [auth.id, auth.role, vacancySearchText, vacancyFilters, formatDateForDisplay]);

    const getPermissionsForExport = useCallback(async () => {
        const params: IGetAllPermissionsRequestNoPagination = {
            role: auth.role,
            searchTerm: permissionSearchText === "" ? undefined : permissionSearchText,
            date: permissionFilters.date as string | undefined,
            type: permissionFilters.type === "all" ? undefined : permissionFilters.type as PermissionType,
            status: permissionFilters.status === "all" ? undefined : permissionFilters.status as PermissionRequestStatus,
            disablePagination: true,
            myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && permissionFilters.myStatus && permissionFilters.myStatus !== "all" ? permissionFilters.myStatus as PermissionRequestStatus : undefined,
        };
        const response = await PERMISSION.GET_ALL_FOR_EXPORT(params);
        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1;
            return response.data.items.map((item: IPermission) => {
                const exportItem: Record<string, any> = {
                    "Id": item.id,
                    "User Name": item.user?.name ?? "N/A",
                    "Request Date": formatDateForDisplay(item.dateCreated),
                    "Permission Date": formatDateForDisplay(item.permissionDate),
                    "Start Time": item.fromTime,
                    "End Time": item.toTime,
                    "Duration": item.duration,
                    "Type": item.type,
                    "Status": item.status,
                    "Reason": item.reason,
                };
                if (isPrivilegedUser) exportItem["My Status"] = item.myStatus ?? item.status;
                return exportItem;
            });
        }
        return [];
    }, [auth.role, permissionSearchText, permissionFilters, formatDateForDisplay]);

    // New: Export function for Work From Home requests
    const getWorkFromHomeForExport = useCallback(async () => {
        const params: IGetAllWorkFromHomeRequestNoPagination = {
            role: auth.role,
            searchTerm: workFromHomeSearchText === "" ? undefined : workFromHomeSearchText,
            fromDate: workFromHomeFilters.fromDate as string | undefined,
            toDate: workFromHomeFilters.toDate as string | undefined,
            status: workFromHomeFilters.status === "all" ? undefined : workFromHomeFilters.status as WorkFromHomeStatus,
            disablePagination: true,
            myStatus: (auth.role === 0 || auth.role === 2 || auth.role === 1) && workFromHomeFilters.myStatus && workFromHomeFilters.myStatus !== "all" ? workFromHomeFilters.myStatus as WorkFromHomeStatus : undefined,
        };
        const response = await WORK_FROM_HOME.GET_ALL_FOR_EXPORT(params);
        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2 || auth.role === 1;
            return response.data.items.map((item: IGetWorkFromHomeRequest) => { // Use IGetWorkFromHomeRequest type
                const exportItem: Record<string, any> = {
                    "Id": item.id,
                    "User Name": item.user?.name ?? "N/A",
                    "Request Date": formatDateForDisplay(item.dateCreated),
                    "Work From Home Date": formatDateForDisplay(item.date),
                    "Status": item.status,
                    "Note for Manager": item.noteForManager, // Include if you want to export this
                };
                if (isPrivilegedUser) exportItem["My Status"] = item.myStatus ?? item.status;
                return exportItem;
            });
        }
        return [];
    }, [auth.role, workFromHomeSearchText, workFromHomeFilters, formatDateForDisplay]);


    return (
        <div className="w-full p-5">
            <div className="flex items-center mb-6 bg-white h-20 p-3" style={{ borderRadius: 8 }}>
                <div className="text-xl font-semibold flex items-center bg-white">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    Calendar
                </div>
            </div>

            <div className="border-t border-gray-200 my-4"></div>

            <div className="mb-4 bg-white h-20 p-3" style={{ borderRadius: 8 }}>
                <div className="flex border-b border-gray-200">
                    <button
                        className={`px-4 py-2 font-medium focus:outline-none ${
                            activeTab === "vacancy"
                                ? "border-b-2 border-blue-500 text-blue-500"
                                : "text-gray-500 hover:text-gray-700"
                        }`}
                        onClick={() => handleTabChange("vacancy")}
                    >
                        Vacancy
                    </button>
                    <button
                        className={`px-4 py-2 font-medium focus:outline-none ${
                            activeTab === "permission"
                                ? "border-b-2 border-blue-500 text-blue-500"
                                : "text-gray-500 hover:text-gray-700"
                        }`}
                        onClick={() => handleTabChange("permission")}
                    >
                        Permissions
                    </button>
                    <button
                        className={`px-4 py-2 font-medium focus:outline-none ${
                            activeTab === "workFromHome" // New tab button
                                ? "border-b-2 border-blue-500 text-blue-500"
                                : "text-gray-500 hover:text-gray-700"
                        }`}
                        onClick={() => handleTabChange("workFromHome")}
                    >
                        Work From Home
                    </button>
                </div>
            </div>

            <div className="flex justify-end gap-3 mb-4">
                {getCurrentSelectedRows().size > 0 && (auth.role === 0 || auth.role === 2 || auth.role === 1|| auth.role === 4) && (
                    <>
                        <button
                            onClick={() => handleBulkOpinion('Approved')}
                            disabled={isSubmitting}
                            className="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-all disabled:bg-gray-400"
                        >
                            Approve Selected ({getCurrentSelectedRows().size})
                        </button>
                        <button
                            onClick={() => handleBulkOpinion('Rejected')}
                            disabled={isSubmitting}
                            className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-all disabled:bg-gray-400"
                        >
                            Reject Selected ({getCurrentSelectedRows().size})
                        </button>
                    </>
                )}
                {activeTab === "vacancy" && (
                    <ServerExportButton
                        fetchDataFunction={getVacanciesForExport}
                        filename="vacancies_report.csv"
                        label="Export All Vacancies"
                    />
                )}
                {activeTab === "permission" && (
                    <ServerExportButton
                        fetchDataFunction={getPermissionsForExport}
                        filename="permissions_report.csv"
                        label="Export All Permissions"
                    />
                )}
                {activeTab === "workFromHome" && ( // New export button for WFH
                    <ServerExportButton
                        fetchDataFunction={getWorkFromHomeForExport}
                        filename="work_from_home_report.csv"
                        label="Export All WFH Requests"
                    />
                )}
            </div>

            {/* DataTable for Vacancy */}
            {activeTab === "vacancy" && (
                <DataTable
                    data={transformedVacancyData}
                    totalCount={totalVacanciesCount}
                    onPageChange={handleVacancyDataTableChange}
                    itemsPerPage={vacancyItemsPerPage}
                    filterConfig={vacancyFilterConfig}
                    loading={loading.vacancies}
                    columnRenderers={vacancyColumnRenderers}
                    enableSelection={auth.role == 4}
                    rowIdKey="Actions"
                    selectedRows={vacancySelectedRows}
                    onSelectionChange={handleSelectionChange}
                    serverSide={true}
                    currentPage={vacancyCurrentPage}
                    currentFilters={vacancyFilters}
                    currentSearchText={vacancySearchText}
                    isRowSelectable={isRowSelectable}
                />
            )}

            {/* DataTable for Permissions */}
            {activeTab === "permission" && (
                <DataTable
                    data={transformedPermissionData}
                    totalCount={totalPermissionsCount}
                    onPageChange={handlePermissionDataTableChange}
                    itemsPerPage={permissionItemsPerPage}
                    filterConfig={permissionFilterConfig}
                    loading={loading.permissions}
                    columnRenderers={permissionColumnRenderers}
                    serverSide={true}
                    enableSelection={auth.role == 4}
                    rowIdKey="Actions"
                    selectedRows={permissionSelectedRows}
                    onSelectionChange={handleSelectionChange}
                    currentPage={permissionCurrentPage}
                    currentFilters={permissionFilters}
                    currentSearchText={permissionSearchText}
                    isRowSelectable={isRowSelectable}
                />
            )}

            {/* DataTable for Work From Home (NEW) */}
            {activeTab === "workFromHome" && (
                <DataTable
                    data={transformedWorkFromHomeData}
                    totalCount={totalWorkFromHomeCount}
                    onPageChange={handleWorkFromHomeDataTableChange}
                    itemsPerPage={workFromHomeItemsPerPage}
                    filterConfig={workFromHomeFilterConfig}
                    loading={loading.workFromHome}
                    columnRenderers={workFromHomeColumnRenderers}
                    serverSide={true}
                    enableSelection={auth.role == 4}
                    rowIdKey="Actions"
                    selectedRows={workFromHomeSelectedRows}
                    onSelectionChange={handleSelectionChange}
                    currentPage={workFromHomeCurrentPage}
                    currentFilters={workFromHomeFilters}
                    currentSearchText={workFromHomeSearchText}
                    isRowSelectable={isRowSelectable}
                />
            )}
        </div>
    );
};

export default Calendar;