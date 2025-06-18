import { useState, useEffect, useCallback, useMemo } from "react";
import { format } from "date-fns";
import LEAVE, { IGetAllLeavesRequestNoPagination, IGetLeaveRequestForCalander, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import PERMISSION, { IPermission, PermissionRequestStatus, PermissionType } from "../../lib/API/Permission";
import ExportButton from "../../components/button/ExportButton"; // This is likely for client-side export
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import DataTable from "../../components/table/tablePagination";
import ServerExportButton from "../../components/button/serverExportButtonProps";
import WORK_FROM_HOME, { WorkFromHomeStatus, IGetAllWorkFromHomeRequestNoPagination, IGetWorkFromHomeRequest, IDName } from "../../lib/API/workFromHome";

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


    const auth = useAppSelector((s) => s.authSlice);
    //#endregion

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

    const transformedVacancyData = useMemo(() => {
        const isPrivilegedUser = auth.role === 0 || auth.role === 2; // Project Manager or Team Leader
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
        const isPrivilegedUser = auth.role === 0 || auth.role === 2; // Project Manager or Team Leader
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
        const isPrivilegedUser = auth.role === 0 || auth.role === 2; // Project Manager or Team Leader
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
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
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
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
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
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
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

    const commonActionsRenderer = useCallback((id: number, path: string) => (
        <Link href={`/calendar/${path}/${id}`} className="text-gray-600 hover:text-gray-900 transition-colors " >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
            </svg>
        </Link>
    ), []);

    const vacancyColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            "Actions": (value: number) => commonActionsRenderer(value, 'vacancy')
        };
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);

    const permissionColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            "Actions": (value: number) => commonActionsRenderer(value, 'permission')
        };
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);

    // New: Column renderers for Work From Home requests
    const workFromHomeColumnRenderers = useMemo(() => {
        const renderers: Record<string, (value: any, row?: Record<string, any>) => React.ReactNode> = {
            "Final Status": commonStatusRenderer,
            "Actions": (value: number) => commonActionsRenderer(value, 'workFromHome') // New path for WFH details
        };
        if (auth.role === 0 || auth.role === 2) { // Project Manager or Team Leader
            renderers["My Status"] = commonStatusRenderer;
        }
        return renderers;
    }, [commonStatusRenderer, commonActionsRenderer, auth.role]);


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
                myStatus: (auth.role === 0 || auth.role === 2) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as LeaveRequestStatus : undefined,
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
                myStatus: (auth.role === 0 || auth.role === 2) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as PermissionRequestStatus : undefined,
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
                myStatus: (auth.role === 0 || auth.role === 2) && filters.myStatus && filters.myStatus !== "all" ? filters.myStatus as WorkFromHomeStatus : undefined,
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


    // Handle tab change (reset pagination/filters for the new tab)
    const handleTabChange = useCallback((tab: "vacancy" | "permission" | "workFromHome") => {
        setActiveTab(tab);
        // Reset to page 1 for the newly active tab
        if (tab === "vacancy") {
            setVacancyCurrentPage(1);
            setVacancyItemsPerPage(10);
            const baseVacancyFilters: Record<string, any> = { fromDate: undefined, toDate: undefined, status: "all", type: "all" };
            if (auth.role === 0 || auth.role === 2) {
                baseVacancyFilters.myStatus = "all";
            }
            setVacancyFilters(baseVacancyFilters);
            setVacancySearchText("");
        } else if (tab === "permission") {
            setPermissionCurrentPage(1);
            setPermissionItemsPerPage(10);
            const basePermissionFilters: Record<string, any> = { date: undefined, type: "all", status: "all" };
            if (auth.role === 0 || auth.role === 2) {
                basePermissionFilters.myStatus = "all";
            }
            setPermissionFilters(basePermissionFilters);
            setPermissionSearchText("");
        } else { // 'workFromHome' tab
            setWorkFromHomeCurrentPage(1);
            setWorkFromHomeItemsPerPage(10);
            const baseWorkFromHomeFilters: Record<string, any> = { fromDate: undefined, toDate: undefined, status: "all" }; // WFH doesn't have a 'type' like leave
            if (auth.role === 0 || auth.role === 2) {
                baseWorkFromHomeFilters.myStatus = "all";
            }
            setWorkFromHomeFilters(baseWorkFromHomeFilters);
            setWorkFromHomeSearchText("");
        }
    }, [auth.role]);

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
            myStatus: (auth.role === 0 || auth.role === 2) && vacancyFilters.myStatus && vacancyFilters.myStatus !== "all" ? vacancyFilters.myStatus as LeaveRequestStatus : undefined,
        };
        const response = await LEAVE.GET_ALL_FOR_EXPORT(params);

        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2;
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
            myStatus: (auth.role === 0 || auth.role === 2) && permissionFilters.myStatus && permissionFilters.myStatus !== "all" ? permissionFilters.myStatus as PermissionRequestStatus : undefined,
        };
        const response = await PERMISSION.GET_ALL_FOR_EXPORT(params);
        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2;
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
            myStatus: (auth.role === 0 || auth.role === 2) && workFromHomeFilters.myStatus && workFromHomeFilters.myStatus !== "all" ? workFromHomeFilters.myStatus as WorkFromHomeStatus : undefined,
        };
        const response = await WORK_FROM_HOME.GET_ALL_FOR_EXPORT(params);
        if (response && response.data && Array.isArray(response.data.items)) {
            const isPrivilegedUser = auth.role === 0 || auth.role === 2;
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
                    serverSide={true}
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
                />
            )}
        </div>
    );
};

export default Calendar;