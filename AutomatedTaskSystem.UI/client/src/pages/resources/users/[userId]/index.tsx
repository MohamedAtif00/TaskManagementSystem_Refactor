import {
    Box,
    Typography,
    Chip,
    Grid,
    Paper,
    MenuItem,
    Menu
} from "@mui/material";
import Head from "next/head";
import { useRouter } from "next/router";
import { CSSProperties, useEffect, useState, useMemo, useCallback } from "react";
import API from "../../../../lib/API";
import UserProfileIcone from "../../../../assets/Icons/UserProfile";
import Tab from "../../../../components/Tab/Tab";
import PERMISSION, { IPermission, PermissionRequestStatus, PermissionType } from "../../../../lib/API/Permission";
import ExportButton from "../../../../components/button/ExportButton";
import React from "react";
import EditUser from "../../../../components/pageComponent/users/editUser";
import LEAVE, { IGetLeaveRequest, LeaveRequestStatus, LeaveRequestType } from "../../../../lib/API/Leave";
import DataTable from "../../../../components/table/tablePagination";
import { format } from "date-fns";
import { Pencil } from "lucide-react";
import { IGetWorkFromHomeRequest, WorkFromHomeStatus } from "../../../../lib/API/workFromHome";

const buttonStyle: CSSProperties = {
    paddingLeft: "55px",
    paddingRight: "55px",
    paddingTop: "7px",
    paddingBottom: "7px",
    textTransform: "none",
    borderRadius: "8px",
};

// Interface for Filter Configuration items used by DataTable
interface FilterConfigItem {
    key: string;
    label: string;
    type: "text" | "select" | "date" | "number";
    options?: { value: string | number; label: string }[];
}

// --- IMPORTANT: Update the IVacation interface to include WorkFromHome properties ---
// Assuming IVacation is imported from a shared types file or defined globally
interface IVacation {
    annual: number;
    sick: number;
    emergency: number;
    annual_MAX: number;
    emergency_MAX: number;
    // You might also need to track permissions here if not already on the user object
    // permission: number;
    // permission_MAX: number;
}

// --- IMPORTANT: Update the IUser interface to include WorkFromHome properties ---
// Assuming this IUser reflects your backend User model for profile display
interface IUser {
    id: number; // userId from router.query will be number
    name: string;
    code?: string; // Corresponds to hrCode usually
    group?: { id: number; name: string };
    email?: string | null;
    hrCode?: string;
    phone?: string;
    role?: number;
    title?: string;
    isArchived?: boolean; // Renamed from archived
    accountType?: number; // 0 for Internal, 1 for External
    vacation?: IVacation; // Existing vacation properties
    permission?: number; // Current permissions used
    permission_MAX?: number; // Max permissions allowed
    // NEW: Work From Home properties
    workFromHome_Used?: number; // Days already used/approved for WFH
    workFromHome_MAX?: number;  // Max allowed WFH days
}

// Interface for user changes data
interface IUserChange {
    id: number;
    userId: number;
    changedByUserId: number;
    changedByUserName: string;
    action: string;
    changes: string;
    changedAt: string;
}

const UserProfile = () => {
    const router = useRouter();
    const { userId } = router.query;

    const [user, setUser] = useState<IUser | null>(null);
    const [userVacationInfo, setUserVacationInfo] = useState<IVacation | null>(null);
    // NEW: Add Work From Home info state
    const [userWorkFromHomeInfo, setUserWorkFromHomeInfo] = useState<{ used: number; max: number } | null>(null);

    // NEW: Add "workFromHome" to the view options
    const [view, setView] = useState<"vacancies" | "updates" | "permission" | "workFromHome">("vacancies");

    // State for Vacancies DataTable
    const [vacancyData, setVacancyData] = useState<IGetLeaveRequest[]>([]);
    const [totalVacancies, setTotalVacancies] = useState(0);
    const [vacancyPage, setVacancyPage] = useState(1);
    const [vacancyPageSize, setVacancyPageSize] = useState(10);
    const [vacancyDtFilters, setVacancyDtFilters] = useState<Record<string, any>>({});
    const [vacancySearch, setVacancySearch] = useState("");
    const [vacancyLoading, setVacancyLoading] = useState(true);

    // State for Permissions DataTable
    const [permissionData, setPermissionData] = useState<IPermission[]>([]);
    const [totalPermissions, setTotalPermissions] = useState(0);
    const [permissionPage, setPermissionPage] = useState(1);
    const [permissionPageSize, setPermissionPageSize] = useState(10);
    const [permissionDtFilters, setPermissionDtFilters] = useState<Record<string, any>>({});
    const [permissionSearch, setPermissionSearch] = useState("");
    const [permissionLoading, setPermissionLoading] = useState(true);

    // NEW: State for Work From Home DataTable
    const [workFromHomeData, setWorkFromHomeData] = useState<IGetWorkFromHomeRequest[]>([]);
    const [totalWorkFromHome, setTotalWorkFromHome] = useState(0);
    const [workFromHomePage, setWorkFromHomePage] = useState(1);
    const [workFromHomePageSize, setWorkFromHomePageSize] = useState(10);
    const [workFromHomeDtFilters, setWorkFromHomeDtFilters] = useState<Record<string, any>>({});
    const [workFromHomeSearch, setWorkFromHomeSearch] = useState("");
    const [workFromHomeLoading, setWorkFromHomeLoading] = useState(true);

    // State for User Changes (Updates) DataTable
    const [userChangesData, setUserChangesData] = useState<IUserChange[]>([]);
    const [allUserChangesData, setAllUserChangesData] = useState<IUserChange[]>([]);
    const [userChangesLoading, setUserChangesLoading] = useState(true);

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const formatDate = useCallback((dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        return format(new Date(dateString), "dd MMM, yyyy, hh:mm a");
    }, []);

    const formatDateOnly = useCallback((dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        return format(new Date(dateString), "dd MMM, yyyy");
    }, []);

    // Fetch User Profile Info
    useEffect(() => {
        if (userId) {
            API.RESOURCES.USERS.GET_ONE(Number(userId)).then((res) => {
                if (res && !res.error && res.data) {
                    setUser(res.data);
                    setUserVacationInfo(res.data.vacation ?? null);
                    // NEW: Set Work From Home info
                    setUserWorkFromHomeInfo({
                        used: res.data.workFromHome?? 0,
                        max: res.data.workFromHome_MAX ?? 0,
                    });
                }
            });
        }
    }, [userId]);

    // Fetch Vacancies
    useEffect(() => {
        if (userId && view === "vacancies") {
            setVacancyLoading(true);
            const params = {
                page: vacancyPage,
                pageSize: vacancyPageSize,
                searchTerm: vacancySearch || undefined,
                ...vacancyDtFilters,
            };
            API.LEAVE.GET_ALL_BY_USER(Number(userId), params).then((res) => {
                if (res && !res.error && res.data) {
                    setVacancyData(res.data.items ?? []);
                    setTotalVacancies(res.data.totalCount ?? 0);
                } else {
                    setVacancyData([]);
                    setTotalVacancies(0);
                }
                setVacancyLoading(false);
            });
        }
    }, [userId, view, vacancyPage, vacancyPageSize, vacancyDtFilters, vacancySearch]);

    // Fetch Permissions
    useEffect(() => {
        if (userId && view === "permission") {
            setPermissionLoading(true);
            const params = {
                page: permissionPage,
                pageSize: permissionPageSize,
                searchTerm: permissionSearch || undefined,
                ...permissionDtFilters,
            };
            API.PERMISSION.GET_ALL_BY_USER(Number(userId), params).then((res) => {
                if (res && !res.error && res.data) {
                    setPermissionData(res.data.items ?? []);
                    setTotalPermissions(res.data.totalCount ?? 0);
                } else {
                    setPermissionData([]);
                    setTotalPermissions(0);
                }
                setPermissionLoading(false);
            });
        }
    }, [userId, view, permissionPage, permissionPageSize, permissionDtFilters, permissionSearch]);

    // NEW: Fetch Work From Home requests
    useEffect(() => {
        if (userId && view === "workFromHome") {
            setWorkFromHomeLoading(true);
            const params = {
                page: workFromHomePage,
                pageSize: workFromHomePageSize,
                searchTerm: workFromHomeSearch || undefined,
                ...workFromHomeDtFilters,
            };
            API.WORK_FROM_HOME.GET_ALL_BY_USER(Number(userId), params).then((res) => {
                if (res && !res.error && res.data) {
                    setWorkFromHomeData(res.data.items ?? []);
                    setTotalWorkFromHome(res.data.totalCount ?? 0);
                } else {
                    setWorkFromHomeData([]);
                    setTotalWorkFromHome(0);
                }
                setWorkFromHomeLoading(false);
            });
        }
    }, [userId, view, workFromHomePage, workFromHomePageSize, workFromHomeDtFilters, workFromHomeSearch]);


    // Fetch User Changes History (Updates)
    useEffect(() => {
        if (userId && view === "updates") {
            setUserChangesLoading(true);
            API.RESOURCES.USERS.GET_USER_CHANGES(Number(userId)).then((res) => {
                if (res && !res.error) {
                    const sortedUserChanges = (res.data ?? []).sort((a, b) => {
                        return new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime();
                    });
                    setAllUserChangesData(sortedUserChanges);
                    setUserChangesData(sortedUserChanges);
                }
                setUserChangesLoading(false);
            });
        }
    }, [userId, view]);


    const handleVacancyTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => {
        setVacancyPage(page);
        setVacancyPageSize(itemsPerPage);
        setVacancyDtFilters(filters);
        setVacancySearch(searchText);
    }, []);

    const handlePermissionTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => {
        setPermissionPage(page);
        setPermissionPageSize(itemsPerPage);
        setPermissionDtFilters(filters);
        setPermissionSearch(searchText);
    }, []);

    // NEW: Handle Work From Home DataTable changes
    const handleWorkFromHomeTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => {
        setWorkFromHomePage(page);
        setWorkFromHomePageSize(itemsPerPage);
        setWorkFromHomeDtFilters(filters);
        setWorkFromHomeSearch(searchText);
    }, []);

    const handleUserChangesTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => {
        // This is for client-side DataTable, so data changes are handled by DataTable itself.
        // No need to update state here for re-fetch, but you could apply client-side filters if needed.
    }, []);


    const transformedVacancyData = useMemo(() => vacancyData.map(v => ({
        "ID": v.id,
        "Request Date": formatDateOnly(v.dateCreated),
        "Vacancy Type": v.type,
        "Status": v.status,
        "Start Date": formatDateOnly(v.startDate),
        "End Date": formatDateOnly(v.endDate),
        "Duration": `${v.duration} day(s)`,
    })), [vacancyData, formatDateOnly]);

    const transformedPermissionData = useMemo(() => permissionData.map(p => ({
        "ID": p.id,
        "Request Date": formatDateOnly(p.dateCreated),
        "Permission Type": p.type,
        "Status": p.status,
        "Permission Date": formatDateOnly(p.permissionDate),
        "From": p.fromTime,
        "To": p.toTime,
        "Duration": p.duration,
    })), [permissionData, formatDateOnly]);

    // NEW: Transformed data for Work From Home DataTable
    const transformedWorkFromHomeData = useMemo(() => workFromHomeData.map(wfh => ({
        "ID": wfh.id,
        "Request Date": formatDateOnly(wfh.dateCreated),
        "Work From Home Date": formatDateOnly(wfh.date),
        "Status": wfh.status,
        "Note for Manager": wfh.noteForManager || "N/A", // Include this if applicable
    })), [workFromHomeData, formatDateOnly]);


    const transformedUserChangesData = useMemo(() => allUserChangesData.map(change => ({
        "ID": change.id,
        "Changed At": formatDate(change.changedAt),
        "Action": change.action,
        "Changes": change.changes,
        "Changed By": change.changedByUserName,
    })), [allUserChangesData, formatDate]);


    const getStatusColor = (status: LeaveRequestStatus | PermissionRequestStatus | WorkFromHomeStatus): string => {
        switch (status) {
            case LeaveRequestStatus.Pending:
            case PermissionRequestStatus.Pending:
            case WorkFromHomeStatus.Pending:
                return '#FFA500'; // Orange
            case LeaveRequestStatus.Approved:
            case PermissionRequestStatus.Approved:
            case WorkFromHomeStatus.Approved:
                return '#4CAF50'; // Green
            case LeaveRequestStatus.Rejected:
            case PermissionRequestStatus.Rejected:
            case WorkFromHomeStatus.Rejected:
                return '#F44336'; // Red
            case LeaveRequestStatus.Cancelled:
            case PermissionRequestStatus.Cancelled:
            case WorkFromHomeStatus.Cancelled:
                return '#9E9E9E'; // Grey
            default: return '#9E9E9E';
        }
    };

    const commonStatusRenderer = useCallback((value: string) => {
        let colorClass = '';
        let bgColorClass = '';
        // Map string value back to enum for consistent coloring
        const statusEnum: LeaveRequestStatus | PermissionRequestStatus | WorkFromHomeStatus =
            value === 'Approved' ? LeaveRequestStatus.Approved as any : // Cast to any to satisfy all enum types
            value === 'Rejected' ? LeaveRequestStatus.Rejected as any :
            value === 'Pending' ? LeaveRequestStatus.Pending as any :
            value === 'Cancelled' ? LeaveRequestStatus.Cancelled as any :
            LeaveRequestStatus.Pending as any; // Default or handle 'all' etc.

        switch (statusEnum) {
            case LeaveRequestStatus.Approved:
            case PermissionRequestStatus.Approved:
            case WorkFromHomeStatus.Approved: // NEW
                colorClass = 'text-green-700';
                bgColorClass = 'bg-green-100';
                break;
            case LeaveRequestStatus.Rejected:
            case PermissionRequestStatus.Rejected:
            case WorkFromHomeStatus.Rejected: // NEW
                colorClass = 'text-red-700';
                bgColorClass = 'bg-red-100';
                break;
            case LeaveRequestStatus.Pending:
            case PermissionRequestStatus.Pending:
            case WorkFromHomeStatus.Pending: // NEW
                colorClass = 'text-yellow-700';
                bgColorClass = 'bg-yellow-100';
                break;
            case LeaveRequestStatus.Cancelled:
            case PermissionRequestStatus.Cancelled:
            case WorkFromHomeStatus.Cancelled: // NEW
                colorClass = 'text-gray-700';
                bgColorClass = 'bg-gray-100';
                break;
            default:
                colorClass = 'text-gray-700';
                bgColorClass = 'bg-gray-100';
        }
        return <Chip label={value} size="small" className={`${colorClass} ${bgColorClass}`} />;
    }, []);

    const vacancyColumnRenderers = useMemo(() => ({ "Status": commonStatusRenderer }), [commonStatusRenderer]);
    const permissionColumnRenderers = useMemo(() => ({ "Status": commonStatusRenderer }), [commonStatusRenderer]);
    // NEW: Work From Home Column Renderers
    const workFromHomeColumnRenderers = useMemo(() => ({ "Status": commonStatusRenderer }), [commonStatusRenderer]);

    const userChangesColumnRenderers = useMemo(() => ({
        "Action": (value: string) => commonStatusRenderer(value), // Reusing for 'Action' status
        "Changes": (value: string) => value.split(';').map((item, idx) => <div key={idx}>{item.trim()}</div>)
    }), [commonStatusRenderer]);

    const handleMenuClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleEdit = () => {
        router.push({
            pathname: `/resources/users/${user?.id}`,
            query: {
                form: "edit-user",
                userId: user?.id,
            },
        });
        handleClose();
    };

    const handleDelete = () => {
        console.log("Delete user", user);
        handleClose();
    };

    const vacancyFilterConfig: FilterConfigItem[] = useMemo(() => [
        { key: 'status', label: 'Status', type: 'select', options: Object.values(LeaveRequestStatus).map(s => ({ value: s, label: s })) },
        { key: 'type', label: 'Type', type: 'select', options: Object.values(LeaveRequestType).map(t => ({ value: t, label: t })) },
        { key: 'startDate', label: 'Start Date After', type: 'date' },
        { key: 'endDate', label: 'End Date Before', type: 'date' },
    ], []);

    const permissionFilterConfig: FilterConfigItem[] = useMemo(() => [
        { key: 'status', label: 'Status', type: 'select', options: Object.values(PermissionRequestStatus).map(s => ({ value: s, label: s })) },
        { key: 'type', label: 'Type', type: 'select', options: Object.values(PermissionType).map(t => ({ value: t, label: t })) },
        { key: 'permissionDate', label: 'Permission Date', type: 'date' },
    ], []);

    // NEW: Work From Home Filter Config
    const workFromHomeFilterConfig: FilterConfigItem[] = useMemo(() => [
        { key: 'status', label: 'Status', type: 'select', options: Object.values(WorkFromHomeStatus).map(s => ({ value: s, label: s })) },
        { key: 'date', label: 'Work From Home Date', type: 'date' }, // Assuming single date filter
    ], []);

    const userChangesFilterConfig: FilterConfigItem[] = useMemo(() => [
        {
            key: 'action', label: 'Action', type: 'select', options: [
                { value: "Created", label: "Created" },
                { value: "Updated", label: "Updated" },
                { value: "Deleted", label: "Deleted" }
            ]
        },
        { key: 'changedByUserName', label: 'Changed By', type: 'text' },
        { key: 'changedAt', label: 'Changed After', type: 'date' },
    ], []);

    // NEW: Add "workFromHome" to handleTabChange
    const handleTabChange = (newView: "vacancies" | "updates" | "permission" | "workFromHome") => {
        setView(newView);
        // Reset pagination and filters for the new tab
        if (newView === "vacancies") {
            setVacancyPage(1);
            setVacancyDtFilters({});
            setVacancySearch("");
        } else if (newView === "permission") {
            setPermissionPage(1);
            setPermissionDtFilters({});
            setPermissionSearch("");
        } else if (newView === "workFromHome") { // NEW: Reset WFH states
            setWorkFromHomePage(1);
            setWorkFromHomeDtFilters({});
            setWorkFromHomeSearch("");
        } else if (newView === "updates") {
            // No explicit reset needed here for page/filters for client-side updates table
        }
    };

    if (!user) return <Typography>Loading...</Typography>;

    return (
        <div className="w-full flex align-middle justify-center">
            <div className="w-10/12 ">
                <Box sx={{ paddingTop: 2, paddingX: 4, borderRadius: 1, marginBottom: 2 }}>
                    <Paper sx={{ p: 3 }} elevation={2} className="flex ">
                        <UserProfileIcone />
                        <Typography variant="h5" fontWeight="bold" gutterBottom>
                            User Profile
                        </Typography>
                    </Paper>
                </Box>

                <Box sx={{ p: 4, borderRadius: 2, display: 'flex', flexDirection: 'column', gap: 4 }}>

                    {/* User Profile Paper */}
                    <Paper elevation={3} sx={{ p: 3 }} className="flex flex-col md:flex-row justify-between gap-4 bg-white shadow-md rounded-xl">

                        {/* User Info Section */}
                        <Box className="space-y-2 rounded-lg border border-gray-200 w-full md:w-2/3">
                            <Grid sx={{ display: "flex" }} className="justify-between w-full">
                                <Typography variant="body1" className="flex items-center gap-2 font-bold text-lg">
                                    {user.name}
                                </Typography>
                            </Grid>

                            {user.hrCode && ( // Changed from user.code to user.hrCode
                                <Typography className="flex items-center gap-2 text-[#22648C] text-[14px]">
                                    {user.hrCode}
                                </Typography>
                            )}

                            <Grid container className="gap-x-20 gap-y-4">
                                {/* Column 1 */}
                                <Grid item xs={5}>
                                    <div className="flex flex-col gap-4">
                                        <Typography className="flex items-center text-gray-700">
                                            <span className="font-bold w-fit">Department:</span>
                                            <span className="text-[#5570FF]">{user.group?.name}</span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit ">Type:</span>
                                            <span className="text-[#5570FF]">
                                                {user.accountType === 0 ? "Internal" : "External"} {/* Corrected mapping */}
                                            </span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">Status:</span>
                                            <span className="text-[#5570FF]">
                                                {user.isArchived ? "Archived" : "Active"}
                                            </span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">HR Code:</span>
                                            <span className="text-[#5570FF]">
                                                {user.hrCode}
                                            </span>
                                        </Typography>
                                    </div>
                                </Grid>

                                {/* Column 2 */}
                                <Grid item xs={5}>
                                    <div className="flex flex-col gap-4">
                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">Role:</span>
                                            <span className="text-[#5570FF]">
                                                {user.role === 0 ? "Project Manager" :
                                                    user.role === 1 ? "Section Head" :
                                                        user.role === 2 ? "Team Leader" :
                                                            user.role === 3 ? "Member" : "Owner"} {/* Added Owner */}
                                            </span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">Email:</span>
                                            <span className="text-[#5570FF]">{user.email}</span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">Phone:</span>
                                            <span className="text-[#5570FF]">{user.phone}</span>
                                        </Typography>

                                        <Typography className="flex items-center gap-2 text-gray-700">
                                            <span className="font-bold w-fit">Title:</span>
                                            <span className="text-[#5570FF]">{user.title}</span>
                                        </Typography>
                                    </div>
                                </Grid>
                            </Grid>
                        </Box>

                        {/* Leave/Permission/Work From Home Info Section */}
                        <div className="flex justify-between items-end pt-4 w-full md:w-1/3">
                            {[
                                { label: "Annual", value: userVacationInfo?.annual, total: userVacationInfo?.annual_MAX },
                                { label: "Sick", value: userVacationInfo?.sick, total: undefined }, // Sick usually doesn't have a max
                                { label: "Emergency", value: userVacationInfo?.emergency, total: userVacationInfo?.emergency_MAX },
                                { label: "Permissions", value: user.permission, total: user.permission_MAX }, // Directly from user object
                                { label: "Work From Home", value: userWorkFromHomeInfo?.used, total: userWorkFromHomeInfo?.max }, // NEW: WFH balance
                            ].map((item, index) => (
                                <div
                                    key={item.label}
                                    className={`flex flex-col items-center px-2 w-full ${
                                        index < 4 ? 'border-r border-gray-300' : '' // Adjust border for 5 items
                                    }`}
                                    style={{
                                        borderRight: index < 4 ? '1px solid #D1D5DB' : 'none',
                                    }}
                                >
                                    <Typography variant="body1" className="font-bold text-gray-800">
                                        {item.label}
                                    </Typography>
                                    <Typography variant="body1" component="p" className="text-gray-600 font-light">
                                        <span className="text-blue-500 font-bold text-lg">{item.value ?? 0}</span>
                                        {item.total !== undefined ? ` / ${item.total ?? 0}` : ''}
                                    </Typography>
                                </div>
                            ))}
                        </div>

                        {/* Edit Button */}
                        <div className="flex justify-center items-center">
                            <button
                                onClick={handleEdit}
                                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors text-sm font-medium"
                            >Edit
                            </button>
                        </div>
                    </Paper>

                    {/* Tabs for Vacancies, Permissions, Updates, and Work From Home */}
                    <div className="relative mt-20">
                        <div className="flex gap-2 items-end h-9 absolute " style={{ top: -37 }}>
                            <Tab
                                label="Vacancies"
                                active={view === "vacancies"}
                                style={{ borderTopLeftRadius: 16, borderTopRightRadius: 16 }}
                                onClick={() => handleTabChange("vacancies")}
                            />
                            <Tab
                                label="Permission"
                                active={view === "permission"}
                                style={{ borderTopLeftRadius: 16, borderTopRightRadius: 16 }}
                                onClick={() => handleTabChange("permission")}
                            />
                            <Tab
                                label="Work From Home" // NEW: Work From Home Tab
                                active={view === "workFromHome"}
                                style={{ borderTopLeftRadius: 16, borderTopRightRadius: 16 }}
                                onClick={() => handleTabChange("workFromHome")}
                            />
                            <Tab
                                label="Updates"
                                active={view === "updates"}
                                style={{ borderTopLeftRadius: 16, borderTopRightRadius: 16 }}
                                onClick={() => handleTabChange("updates")}
                            />
                        </div>

                        {/* DataTable for Vacancies */}
                        {view === "vacancies" ? (
                            <Paper sx={{ p: 3, mt: 0 }} elevation={2}>
                                <DataTable
                                    data={transformedVacancyData}
                                    totalCount={totalVacancies}
                                    onPageChange={handleVacancyTableChange}
                                    itemsPerPage={vacancyPageSize}
                                    loading={vacancyLoading}
                                    filterConfig={vacancyFilterConfig}
                                    columnRenderers={vacancyColumnRenderers}
                                    serverSide={true}
                                     showSearchInput={false}
                                />
                            </Paper>
                        ) : view === "permission" ? (
                            <Paper sx={{ p: 3, mt: 0 }} elevation={2}>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                                    <Typography variant="h6" fontWeight="bold">
                                        Permissions
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                        <Typography variant="body1" sx={{ mr: 1 }}>
                                            Remaining:
                                        </Typography>
                                        <Typography variant="body1" fontWeight="bold" color="primary">
                                            {user.permission ?? 0} / {user.permission_MAX ?? 0}
                                        </Typography>
                                        <Box sx={{ ml: 2 }}>
                                            <ExportButton data={transformedPermissionData} filename={`user_${userId}_permissions.csv`}></ExportButton>
                                        </Box>
                                    </Box>
                                </Box>
                                <DataTable
                                    data={transformedPermissionData}
                                    totalCount={totalPermissions}
                                    onPageChange={handlePermissionTableChange}
                                    itemsPerPage={permissionPageSize}
                                    loading={permissionLoading}
                                    filterConfig={permissionFilterConfig}
                                    columnRenderers={permissionColumnRenderers}
                                    serverSide={true}
                                    showSearchInput={false}
                                />
                            </Paper>
                        ) : view === "workFromHome" ? ( // NEW: DataTable for Work From Home
                            <Paper sx={{ p: 3, mt: 0 }} elevation={2}>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                                    <Typography variant="h6" fontWeight="bold">
                                        Work From Home Requests
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                        <Typography variant="body1" sx={{ mr: 1 }}>
                                            Remaining:
                                        </Typography>
                                        <Typography variant="body1" fontWeight="bold" color="primary">
                                            {userWorkFromHomeInfo?.used ?? 0} / {userWorkFromHomeInfo?.max ?? 0}
                                        </Typography>
                                        <Box sx={{ ml: 2 }}>
                                            {/* You might want a specific export button for WFH data if needed */}
                                            {/* <ExportButton data={transformedWorkFromHomeData} filename={`user_${userId}_work_from_home.csv`}></ExportButton> */}
                                        </Box>
                                    </Box>
                                </Box>
                                <DataTable
                                    data={transformedWorkFromHomeData}
                                    totalCount={totalWorkFromHome}
                                    onPageChange={handleWorkFromHomeTableChange}
                                    itemsPerPage={workFromHomePageSize}
                                    loading={workFromHomeLoading}
                                    filterConfig={workFromHomeFilterConfig}
                                    columnRenderers={workFromHomeColumnRenderers}
                                    serverSide={true}
                                     showSearchInput={false}
                                />
                            </Paper>
                        ) : (
                            // DataTable for User Changes (Updates)
                            <Paper sx={{ p: 3, mt: 0 }} elevation={2}>
                                <DataTable
                                    data={transformedUserChangesData}
                                    totalCount={allUserChangesData.length}
                                    onPageChange={handleUserChangesTableChange}
                                    itemsPerPage={10}
                                    loading={userChangesLoading}
                                    filterConfig={userChangesFilterConfig}
                                    columnRenderers={userChangesColumnRenderers}
                                    serverSide={false} // Client-side pagination and filtering for updates
                                     showSearchInput={false}
                                />
                            </Paper>
                        )}
                    </div>
                </Box>
            </div>
            <EditUser />
        </div>
    );
};

export default UserProfile;