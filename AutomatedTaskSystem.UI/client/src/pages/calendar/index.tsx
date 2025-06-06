import { useState, useEffect, useCallback, useMemo } from "react";
import { format } from "date-fns";
import LEAVE, { IGetAllLeavesRequestNoPagination, IGetLeaveRequestForCalander, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import PERMISSION, { IPermission, PermissionRequestStatus, PermissionType } from "../../lib/API/Permission";
import ExportButton from "../../components/button/ExportButton"; // This is likely for client-side export
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import DataTable from "../../components/table/tablePagination";
import ServerExportButton from "../../components/button/serverExportButtonProps";

// Assume these interfaces are imported or defined globally if they are not part of LEAVE/PERMISSION
interface IGetAllLeavesApiResponse {
    items: IGetLeaveRequestForCalander[];
    totalCount: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

interface IGetAllLeavesRequest {
    userId?: number;
    role?: number;
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    fromDate?: string;
    toDate?: string;
    status?: LeaveRequestStatus | "all";
    type?: LeaveRequestType | "all";
}

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
    [key: string]: string | number | boolean | undefined; // Add this line
}

// Interface for fetching all permissions without pagination, similar to IGetAllLeavesRequestNoPagination
interface IGetAllPermissionsRequestNoPagination {
    userId?: number; // Assuming permissions might also be filtered by user, though not explicitly in current filters
    role: number;
    searchTerm?: string;
    date?: string;
    type?: PermissionType | "all";
    status?: PermissionRequestStatus | "all";
    disablePagination: true; // Explicitly indicates no pagination
}

const Calendar = () => {
    const [activeTab, setActiveTab] = useState<"vacancy" | "permission">("vacancy");

    const [vacancies, setVacancies] = useState<IGetLeaveRequestForCalander[]>([]);
    const [permissions, setPermissions] = useState<IPermission[]>([]);
    const [totalVacanciesCount, setTotalVacanciesCount] = useState<number>(0);
    const [totalPermissionsCount, setTotalPermissionsCount] = useState<number>(0);

    const [loading, setLoading] = useState({
        vacancies: true,
        permissions: true
    });

    // Pagination & Filter state for Vacancies (to be passed to API)
    const [vacancyCurrentPage, setVacancyCurrentPage] = useState<number>(1);
    const [vacancyItemsPerPage, setVacancyItemsPerPage] = useState<number>(10);
    const [vacancyFilters, setVacancyFilters] = useState<Record<string, string | number | undefined>>({
        fromDate: undefined,
        toDate: undefined,
        status: "all",
        type: "all"
    });
    const [vacancySearchText, setVacancySearchText] = useState<string>("");

    // Pagination & Filter state for Permissions (to be passed to API)
    const [permissionCurrentPage, setPermissionCurrentPage] = useState<number>(1);
    const [permissionItemsPerPage, setPermissionItemsPerPage] = useState<number>(10);
    const [permissionFilters, setPermissionFilters] = useState<Record<string, string | number | undefined>>({
        date: undefined,
        type: "all",
        status: "all"
    });
    const [permissionSearchText, setPermissionSearchText] = useState<string>("");

    const auth = useAppSelector((s) => s.authSlice);

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

    const transformedVacancyData = useMemo(() => vacancies.map(vacancy => ({
        "User name": vacancy.user?.name ?? "N/A",
        "Request date": formatDateForDisplay(vacancy.dateCreated),
        "Vacancy type": vacancy.type,
        "Final Status": vacancy.status,
        "Vacancy date": formatDateForDisplay(vacancy.startDate),
        "Actions": vacancy.id
    })), [vacancies, formatDateForDisplay]);

    const transformedPermissionData = useMemo(() => permissions.map(permission => ({
        "User name": permission.user?.name ?? "N/A",
        "Request date": formatDateForDisplay(permission.dateCreated),
        "Permission Type": permission.type,
        "Final Status": permission.status,
        "Permission date": formatDateForDisplay(permission.permissionDate),
        "Actions": permission.id
    })), [permissions, formatDateForDisplay]);

    const vacancyFilterConfig = useMemo(() => [
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
            label: "Status",
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
    ], []);

    const permissionFilterConfig = useMemo(() => [
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
            label: "Status",
            type: "select" as const,
            options: [
                { value: "all", label: "All" },
                { value: PermissionRequestStatus.Pending, label: "Pending" },
                { value: PermissionRequestStatus.Approved, label: "Approved" },
                { value: PermissionRequestStatus.Rejected, label: "Rejected" },

            ]
        }
    ], []);

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

    const vacancyColumnRenderers = useMemo(() => ({
        "Final Status": commonStatusRenderer,
        "Actions": (value: number) => commonActionsRenderer(value, 'vacancy')
    }), [commonStatusRenderer, commonActionsRenderer]);

    const permissionColumnRenderers = useMemo(() => ({
        "Final Status": commonStatusRenderer,
        "Actions": (value: number) => commonActionsRenderer(value, 'permission')
    }), [commonStatusRenderer, commonActionsRenderer]);

    const fetchVacancies = useCallback(async (page: number, itemsPerPage: number, filters: Record<string, string | number | undefined>, searchText: string) => {
        setLoading(prev => ({ ...prev, vacancies: true }));
        try {
            const params: IGetAllLeavesRequest = {
                userId: auth.id,
                role: auth.role,
                page: page,
                pageSize: itemsPerPage,
                searchTerm: searchText === "" ? undefined : searchText,
                fromDate: filters.fromDate as string | undefined,
                toDate: filters.toDate as string | undefined,
                status: filters.status === "all" ? undefined : filters.status as LeaveRequestStatus,
                type: filters.type === "all" ? undefined : filters.type as LeaveRequestType,
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


    // Handle tab change (reset pagination/filters for the new tab)
    const handleTabChange = useCallback((tab: "vacancy" | "permission") => {
        setActiveTab(tab);
        // Reset to page 1 for the newly active tab
        if (tab === "vacancy") {
            setVacancyCurrentPage(1);
            setVacancyItemsPerPage(10); // Reset items per page too
            setVacancyFilters({ fromDate: undefined, toDate: undefined, status: "all", type: "all" });
            setVacancySearchText("");
        } else {
            setPermissionCurrentPage(1);
            setPermissionItemsPerPage(10); // Reset items per page too
            setPermissionFilters({ date: undefined, type: "all", status: "all" });
            setPermissionSearchText("");
        }
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
            disablePagination: true
        };
        const response = await LEAVE.GET_ALL_FOR_EXPORT(params);
        if (response && response.data && Array.isArray(response.data.items)) {
            return response.data.items.map(item => ({
                "Id": item.id,
                "User Name": item.user?.name ?? "N/A",
                "Request Date": formatDateForDisplay(item.dateCreated),
                "Start Date": formatDateForDisplay(item.startDate),
                "End Date": formatDateForDisplay(item.endDate),
                "Duration": item.duration,
                "Type": item.type,
                "Status": item.status,
                "Reason": item.reason,
            }));
        }
        return [];
    }, [auth.id, auth.role, vacancySearchText, vacancyFilters, formatDateForDisplay]);

    // Assuming IPermission is correctly imported or defined:
// import { IPermission } from "../../lib/API/Permission";

    const getPermissionsForExport = useCallback(async () => {
        const params: IGetAllPermissionsRequestNoPagination = {
            role: auth.role,
            searchTerm: permissionSearchText === "" ? undefined : permissionSearchText,
            date: permissionFilters.date as string | undefined,
            type: permissionFilters.type === "all" ? undefined : permissionFilters.type as PermissionType,
            status: permissionFilters.status === "all" ? undefined : permissionFilters.status as PermissionRequestStatus,
            disablePagination: true
        };
        const response = await PERMISSION.GET_ALL_FOR_EXPORT(params); // Assuming a similar export endpoint for permissions
        if (response && response.data && Array.isArray(response.data.items)) {
            return response.data.items.map((item: IPermission) => ({ // <-- Add type annotation here
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
            }));
        }
        return [];
    }, [auth.role, permissionSearchText, permissionFilters, formatDateForDisplay]);

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
                </div>
            </div>

            <div className="flex justify-end gap-3 mb-4">
                {/* Client-side export for the currently displayed data */}
                <ExportButton data={activeTab === "vacancy" ? transformedVacancyData : transformedPermissionData} />

                {/* Server-side export button for Vacancies */}
                {activeTab === "vacancy" && (
                    <ServerExportButton
                        fetchDataFunction={getVacanciesForExport}
                        filename="vacancies_report.csv"
                        label="Export All Vacancies"
                    />
                )}

                {/* Server-side export button for Permissions */}
                {activeTab === "permission" && (
                    <ServerExportButton
                        fetchDataFunction={getPermissionsForExport}
                        filename="permissions_report.csv"
                        label="Export All Permissions"
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
        </div>
    );
};

export default Calendar;