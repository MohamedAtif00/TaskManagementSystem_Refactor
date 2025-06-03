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
import DataTable from "../../../../components/table/tablePagination"; // Import DataTable
import { format } from "date-fns";
import { Pencil } from "lucide-react";
// import MoreVertIcon from "@mui/icons-material/MoreVert";

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

// Define interface for user changes data
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
    const [view, setView] = useState<"vacancies" | "updates" | "permission">("vacancies");

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

    // State for User Changes (Updates) DataTable
    const [userChangesData, setUserChangesData] = useState<IUserChange[]>([]);
    const [allUserChangesData, setAllUserChangesData] = useState<IUserChange[]>([]); // For client-side filtering
    const [userChangesLoading, setUserChangesLoading] = useState(true);

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const formatDate = useCallback((dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        return format(new Date(dateString), "dd MMM yyyy, hh:mm a");
    }, []);

    const formatDateOnly = useCallback((dateString: string | null | undefined) => {
        if (!dateString) return "N/A";
        return format(new Date(dateString), "dd MMM yyyy");
    }, []);

    useEffect(() => {
        if (userId) {
            API.RESOURCES.USERS.GET_ONE(Number(userId)).then((res) => {
                if (res && !res.error) {
                    setUser(res.data);
                    setUserVacationInfo(res.data.vacation ?? null);
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
            PERMISSION.GET_ALL_BY_USER(Number(userId), params).then((res) => {
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

    // Fetch User Changes History (Updates) - Assumes client-side pagination/filtering for this one
    useEffect(() => {
        if (userId && view === "updates") {
            setUserChangesLoading(true);
            API.RESOURCES.USERS.GET_USER_CHANGES(Number(userId)).then((res) => {
                if (res && !res.error) {
                    const sortedUserChanges = (res.data ?? []).sort((a, b) => {
                        return new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime();
                    });
                    setAllUserChangesData(sortedUserChanges); // Store all for client-side filtering
                    setUserChangesData(sortedUserChanges); // Initially display all
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

    const handleUserChangesTableChange = useCallback((page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => {
        // For client-side, DataTable handles pagination. We just update our state if needed for other purposes.
        // Filtering and search are also handled by DataTable internally when serverSide=false.
        // This callback is mostly for server-side, but good to have the structure.
        // For client-side, the `data` prop to DataTable will be `allUserChangesData`
        // and `totalCount` will be `allUserChangesData.length`.
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

    const transformedUserChangesData = useMemo(() => allUserChangesData.map(change => ({
        "ID": change.id,
        "Changed At": formatDate(change.changedAt),
        "Action": change.action,
        "Changes": change.changes,
        "Changed By": change.changedByUserName,
    })), [allUserChangesData, formatDate]);


  const getStatusColor = (status: PermissionRequestStatus): string => {
  switch (status) {
    case PermissionRequestStatus.Pending: return '#FFA500';
    case PermissionRequestStatus.Approved: return '#4CAF50';
    case PermissionRequestStatus.Rejected: return '#F44336';
    default: return '#9E9E9E';
  }
};

const getStatusChipColor = (status: PermissionRequestStatus): 'warning' | 'success' | 'error' | 'default' => {
  switch (status) {
    case PermissionRequestStatus.Pending: return 'warning';
    case PermissionRequestStatus.Approved: return 'success';
    case PermissionRequestStatus.Rejected: return 'error';
    default: return 'default';
  }
};

    const commonStatusRenderer = useCallback((value: string) => {
        let colorClass = '';
        let bgColorClass = '';
        switch (value?.toLowerCase()) {
            case 'approved':
            case 'accepted':
                colorClass = 'text-green-700';
                bgColorClass = 'bg-green-100';
                break;
            case 'rejected':
            case 'cancelled':
                colorClass = 'text-red-700';
                bgColorClass = 'bg-red-100';
                break;
            case 'pending':
                colorClass = 'text-yellow-700';
                bgColorClass = 'bg-yellow-100';
                break;
            default:
                colorClass = 'text-gray-700';
                bgColorClass = 'bg-gray-100';
        }
        return <Chip label={value} size="small" className={`${colorClass} ${bgColorClass}`} />;
    }, []);

    const vacancyColumnRenderers = useMemo(() => ({ "Status": commonStatusRenderer }), [commonStatusRenderer]);
    const permissionColumnRenderers = useMemo(() => ({ "Status": commonStatusRenderer }), [commonStatusRenderer]);
    const userChangesColumnRenderers = useMemo(() => ({
        "Action": (value: string) => commonStatusRenderer(value),
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
                pathname: `/resources/users/${user.id}`,
                query: {
                form: "edit-user",
                userId: user.id,
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

    const userChangesFilterConfig: FilterConfigItem[] = useMemo(() => [
        { key: 'action', label: 'Action', type: 'select', options: [
            {value: "Created", label: "Created"}, 
            {value: "Updated", label: "Updated"},
            {value: "Deleted", label: "Deleted"}
        ]},
        { key: 'changedByUserName', label: 'Changed By', type: 'text' },
        { key: 'changedAt', label: 'Changed After', type: 'date' }, // Note: DataTable date filter is exact match. For range, you'd need two date filters or custom logic.
    ], []);

    const handleTabChange = (newView: "vacancies" | "updates" | "permission") => {
        setView(newView);
        // Reset pagination and filters for the new tab to ensure fresh data load
        if (newView === "vacancies") {
            setVacancyPage(1);
            setVacancyDtFilters({});
            setVacancySearch("");
        } else if (newView === "permission") {
            setPermissionPage(1);
            setPermissionDtFilters({});
            setPermissionSearch("");
        } else if (newView === "updates") {
            // For client-side, DataTable's internal state will reset on data/config change.
            // No explicit reset needed here for page/filters if DataTable handles it.
        }
    };

    if (!user) return <Typography>Loading...</Typography>;

    return (
        <div className="w-full flex align-middle justify-center">
            <div className="w-10/12 ">  
                <Box sx={{paddingTop:2,paddingX:4, borderRadius: 1, marginBottom: 2 }}>
                <Paper sx={{ p: 3 }} elevation={2} className="flex ">
                    
                    <UserProfileIcone/>
                    <Typography variant="h5" fontWeight="bold" gutterBottom>
                        User Profile
                    </Typography>
                    </Paper>
                </Box>

                {/* Add background color to only the header */}
                <Box sx={{ p: 4, borderRadius: 2, display: 'flex', flexDirection: 'column', gap: 4 }}>
                    
                    {/* User Profile Paper */}
                    <Paper elevation={3} sx={{ p: 3 }} className="flex flex-col md:flex-row justify-between gap-4 bg-white shadow-md rounded-xl">
                    
                    {/* User Info Section */}
                    <Box className="space-y-2   rounded-lg border border-gray-200 w-full md:w-2/3">
                          <Grid sx={{ display: "flex" }} className="justify-between w-full">
                                <Typography variant="body1" className="flex items-center gap-2 font-bold text-lg">
                                    {user.name}
                                </Typography>

                                </Grid>

                        {user.code && (
                            <Typography className="flex items-center gap-2  text-[#22648C] text-[14px]">
                              {user.code}
                            </Typography>
                        )}

                        <Grid container className="gap-x-20 gap-y-4">
                            {/* Column 1 */}
                            <Grid item xs={5}>
                                <div className="flex flex-col gap-4">
                                <Typography className="flex items-center  text-gray-700">
                                    <span className="font-bold w-fit">Department:</span> 
                                    <span className="text-[#5570FF]">{user.group?.name}</span>
                                </Typography>
                                
                                <Typography className="flex items-center gap-2 text-gray-700">
                                    <span className="font-bold w-fit ">Type:</span> 
                                    <span className="text-[#5570FF]">
                                    {user.accountType === 0 ? "External" : "Internal"}
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
                                    user.role === 2 ? "Team Leader" : "Member"}
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

                    {/* Leave Info Section */}
                    <div className="flex justify-between  items-end   pt-4  w-full md:w-1/3 "
                    // style={{borderTop:"1px solid #D1D5DB"}}
                    >

                        {[
                        { label: "Annual", value: userVacationInfo?.annual, total: userVacationInfo?.annual_MAX },
                        { label: "Sick", value: userVacationInfo?.sick , total: userVacationInfo?.sick }, // Assuming sick leave doesn't have a MAX in the same way
                        { label: "Emergency", value: userVacationInfo?.emergency, total: userVacationInfo?.emergency_MAX },
                        ].map((leave, index) => (
                        <div
                            key={leave.label}
                            className={`flex flex-col items-center px-2 w-full ${
                            index !== 2 ? 'border-r border-gray-300' : ''
                            }`}
                            style={{
                                borderRight: index !== 2 ? '1px solid #D1D5DB' : 'none', // gray-300 in Tailwind is #D1D5DB
                            }}
                        >
                            <Typography variant="body1" className="font-bold text-gray-800">
                                {leave.label}
                            </Typography>

                            {leave.label !== "Sick" ? <Typography variant="body1" component="p" className="text-gray-600 font-light">
                                <span className="text-blue-500 font-bold text-lg">{leave.value ?? 0}</span> / {leave.total ?? 0}
                            </Typography>:<>
                            <Typography variant="body1" component="p" className="text-gray-600 font-light">
                                <span className="text-blue-500 font-bold text-lg">{leave.value ?? 0}</span>
                            </Typography>
                            </>}
                        </div>
                        ))}
                    </div>
                    <div className="">

                        <button
                            onClick={handleEdit}
                            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors text-sm font-medium"
                        >Edit
                        </button>
                    </div>

                    {/* <Menu
                        anchorEl={anchorEl}
                        open={open}
                        onClose={handleClose}
                        anchorOrigin={{
                        vertical: "bottom",
                        horizontal: "right",
                        }}
                        transformOrigin={{
                        vertical: "top",
                        horizontal: "right",
                        }}
                    >
                        <MenuItem onClick={handleEdit}>Edit</MenuItem>
                    </Menu> */}
                    </Paper>
                    <div className="relative mt-20">

                        <div className="flex gap-2 items-end h-9 absolute " style={{top:-37}}>
                            <Tab
                                label="Vacancies"
                                active={view === "vacancies"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> handleTabChange("vacancies")}
                            />
                            <Tab
                                label="Permission"
                                active={view === "permission"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> handleTabChange("permission")}
                            />
                            <Tab
                                label="Updates"
                                active={view === "updates"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> handleTabChange("updates")}
                            />
                        </div>

                        {view === "vacancies" ? (
                            <Paper sx={{ p: 3, mt:0 }} elevation={2}>
                                <DataTable
                                    data={transformedVacancyData}
                                    totalCount={totalVacancies}
                                    onPageChange={handleVacancyTableChange}
                                    itemsPerPage={vacancyPageSize}
                                    loading={vacancyLoading}
                                    filterConfig={vacancyFilterConfig}
                                    columnRenderers={vacancyColumnRenderers}
                                    serverSide={true}
                                />
                            </Paper>
                        ) : (view === "permission" ? (
                            <>
                                <Paper sx={{ p: 3, mt:0 }} elevation={2}>
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
                                        <Box sx={{ml: 2}}>
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
                                />
                                </Paper>
                            </>
                        ) : ( 
                            <>
                                <Paper sx={{ p: 3, mt:0 }} elevation={2}>
                                     <DataTable
                                        data={transformedUserChangesData} // Use allUserChangesData for client-side
                                        totalCount={allUserChangesData.length} // Total count is length of all data for client-side
                                        onPageChange={handleUserChangesTableChange} // May not be strictly needed for client-side if DataTable handles it
                                        itemsPerPage={10} // Default items per page
                                        loading={userChangesLoading}
                                        filterConfig={userChangesFilterConfig}
                                        columnRenderers={userChangesColumnRenderers}
                                        serverSide={false} // Client-side pagination and filtering
                                    />
                                </Paper>
                            </>
                        ))}
                    </div>
                </Box>
            </div>
            <EditUser />
            
        </div>
    );
};

export default UserProfile;