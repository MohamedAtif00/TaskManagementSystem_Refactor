import {
    Box,
    Typography,
    Chip,
    Grid,
    Paper,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    TextField,
    Button,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow
} from "@mui/material";
import Head from "next/head";
import { useRouter } from "next/router";
import { CSSProperties, useEffect, useState } from "react";
import API from "../../../../lib/API";
import UserProfileIcone from "../../../../assets/Icons/UserProfile";
import PersonIcon from "../../../../assets/Icons/Person";
import Tab from "../../../../components/Tab/Tab";

const buttonStyle: CSSProperties = {
    paddingLeft: "55px",
    paddingRight: "55px",
    paddingTop: "7px",
    paddingBottom: "7px",
    textTransform: "none",
    borderRadius: "8px",
};

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
    const [vacancies, setVacancies] = useState<IVacation | null>(null);
    const [vacanciesList, setVacanciesList] = useState<IGetVacation[]>([]);
    const [userChangesList, setUserChangesList] = useState<IUserChange[]>([]); 
    const [view, setView] = useState<"vacancies" | "updates" | "permission">("vacancies");
    const [isLoading, setIsLoading] = useState<boolean>(true);

    useEffect(() => {
        console.log(router.query);
        
        if (userId) {
            API.RESOURCES.USERS.GET_ONE(Number(userId)).then((res) => {
                if (res && !res.error) {
                    setUser(res.data);
                    setVacancies(res.data.vacation ?? null);
                }
            });
        }
    }, [router.isReady]);
    
    useEffect(() => {
        if (userId) {
            API.LEAVE.GET_ALL_BY_USER(Number(userId)).then((res) => {
                console.log(res);
                
                if (res && !res.error) {
                    setVacanciesList(res.data ?? []);
                    console.log(res.data, 'inside effect');
                }
            });
        }
    }, [router.isReady]);

    // New effect to fetch user changes history
    useEffect(() => {
        if (userId) {
            setIsLoading(true);
            API.RESOURCES.USERS.GET_USER_CHANGES(Number(userId)).then((res) => {
                if (res && !res.error) {
                    setUserChangesList(res.data ?? []);
                }
                setIsLoading(false);
            });
        }
    }, [router.isReady]);
    
    if (!user) return <Typography>Loading...</Typography>;

    // Function to format date for display
    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('en-GB', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

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
                        <Typography variant="body1"  className="flex items-center gap-2 font-bold text-lg">
                        {user.name}
                        </Typography>
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
                                    {user.accountType === 0 ? "External" : "Internal"}
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
                    <div className="flex justify-between  items-end border-t-2 border-gray-300 pt-4  w-full md:w-1/3 "
                    style={{borderTop:"1px solid #D1D5DB"}}
                    >

                        {[
                        { label: "Annual", value: vacancies?.annual, total: 20 },
                        { label: "Sick", value: vacancies?.sick },
                        { label: "Emergency", value: vacancies?.emergency, total: 5 },
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

                            {leave.label != "Sick"? <Typography variant="body1" component="p" className="text-gray-600 font-light">
                                <span className="text-blue-500 font-bold text-lg">{leave.value}</span> / {leave.total}
                            </Typography>:<>
                            <Typography variant="body1" component="p" className="text-gray-600 font-light">
                                <span className="text-blue-500 font-bold text-lg">{leave.value}</span> 
                            </Typography>
                            </>}
                        </div>
                        ))}
                    </div>
                    </Paper>
                    <div className="relative mt-20">

                        <div className="flex gap-2 items-end h-9 absolute " style={{top:-52}}>
                            <Tab
                                label="Vacancies"
                                active={view === "vacancies"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> setView("vacancies")}
                            />
                            <Tab
                                label="Permission"
                                active={view === "permission"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> setView("permission")}
                            />
                            <Tab
                                label="Updates"
                                active={view === "updates"}
                                style={{borderTopLeftRadius:16,borderTopRightRadius:16}}
                                onClick={()=> setView("updates")}
                            />
                        </div>

                        {/* Filters Paper */}
                        {view === "vacancies" ? (
                            <>
                                <Paper sx={{ p: 3, paddingTop: 0 }} elevation={2}>
                                <Grid container spacing={2} justifyContent={"flex-end"}>
                                    <Grid item xs={12} sm={3}>
                                    <TextField
                                        fullWidth
                                        label="Vacancy From"
                                        type="date"
                                        InputLabelProps={{ shrink: true }}
                                    />
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <TextField
                                        fullWidth
                                        label="Vacancy To"
                                        type="date"
                                        InputLabelProps={{ shrink: true }}
                                    />
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <FormControl fullWidth>
                                        <InputLabel>Status</InputLabel>
                                        <Select label="Status">
                                        <MenuItem value="">All</MenuItem>
                                        <MenuItem value="accepted">Accepted</MenuItem>
                                        <MenuItem value="rejected">Rejected</MenuItem>
                                        </Select>
                                    </FormControl>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                        <FormControl fullWidth >
                                            <InputLabel>Type</InputLabel>
                                            <Select label="Type">
                                            <MenuItem value="">All</MenuItem>
                                            <MenuItem value="Annual">Annual</MenuItem>
                                            <MenuItem value="Sick">Sick</MenuItem>
                                            <MenuItem value="Emergency">Emergency</MenuItem>
                                            </Select>
                                        </FormControl>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="success"  sx={buttonStyle}>
                                        Export CSV
                                    </Button>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="primary"  sx={buttonStyle }>
                                        Apply Filter
                                    </Button>
                                    </Grid>
                                </Grid>
                                </Paper>

                                <Paper sx={{ p: 3 }} elevation={2}>
                                <TableContainer component={Paper}>
                                    <Table>
                                    <TableHead>
                                        <TableRow>
                                        <TableCell>ID</TableCell>
                                        <TableCell>Request Date</TableCell>
                                        <TableCell>Vacancy Type</TableCell>
                                        <TableCell>Status</TableCell>
                                        <TableCell>Vacancy Date</TableCell>
                                        <TableCell align="center">Actions</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {vacanciesList.map((vacancy) => (
                                        <TableRow key={vacancy.id}>
                                            <TableCell>{vacancy.id}</TableCell>
                                            <TableCell>{new Date(vacancy.dateCreated).toLocaleDateString('en-GB') || "N/A"}</TableCell>
                                            <TableCell>{vacancy.type}</TableCell>
                                            <TableCell>{vacancy.status}</TableCell>
                                            
                                            <TableCell>
                                            {new Date(vacancy.startDate).toLocaleDateString('en-GB')}
                                            {/* Shows as "05/05/2025" */}
                                            </TableCell>
                                            <TableCell align="center">
                                            {/* Replace with icons or buttons as needed */}
                                            Edit | Delete
                                            </TableCell>
                                        </TableRow>
                                        ))}
                                    </TableBody>
                                    </Table>
                                </TableContainer>
                                </Paper>
                            </>
                        ) : (view === "permission" ? (
                            <>
                                <Paper sx={{ p: 3, paddingTop: 0 }} elevation={2}>
                                <Grid container spacing={2} justifyContent={"flex-end"}>
                                    <Grid item xs={12} sm={3}>
                                    <FormControl fullWidth>
                                        <InputLabel>Year</InputLabel>
                                        <Select label="Year">
                                        <MenuItem value="2025">2025</MenuItem>
                                        <MenuItem value="2024">2024</MenuItem>
                                        </Select>
                                    </FormControl>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <FormControl fullWidth>
                                        <InputLabel>Month</InputLabel>
                                        <Select label="Month">
                                        <MenuItem value="October">October</MenuItem>
                                        <MenuItem value="November">November</MenuItem>
                                        </Select>
                                    </FormControl>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="success" sx={buttonStyle}>
                                        Export CSV
                                    </Button>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="primary" sx={buttonStyle}>
                                        Apply Filter
                                    </Button>
                                    </Grid>
                                </Grid>
                                </Paper>

                                <Paper sx={{ p: 3, mt: 2 }} elevation={2}>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                                    <Typography variant="h6" fontWeight="bold">
                                    Permissions
                                    </Typography>
                                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                    <Typography variant="body1" sx={{ mr: 1 }}>
                                        Remaining:
                                    </Typography>
                                    <Typography variant="body1" fontWeight="bold" color="primary">
                                        1 / 2
                                    </Typography>
                                    </Box>
                                </Box>

                                <Grid container spacing={2}>
                                    {/* Pending Permission Card */}
                                    <Grid item xs={12} sm={6}>
                                    <Paper elevation={1} sx={{ p: 2, borderLeft: '4px solid #FFA500' }}>
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                        <Typography variant="subtitle1" fontWeight="bold">
                                            Morning Permission
                                        </Typography>
                                        <Chip label="Pending" color="warning" size="small" />
                                        </Box>
                                        <Typography variant="body2" color="text.secondary">
                                        12/12/2024
                                        </Typography>
                                    </Paper>
                                    </Grid>

                                    {/* Accepted Permission Card */}
                                    <Grid item xs={12} sm={6}>
                                    <Paper elevation={1} sx={{ p: 2, borderLeft: '4px solid #4CAF50' }}>
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                        <Typography variant="subtitle1" fontWeight="bold">
                                            Morning Permission
                                        </Typography>
                                        <Chip label="Accepted" color="success" size="small" />
                                        </Box>
                                        <Typography variant="body2" color="text.secondary">
                                        12/12/2024
                                        </Typography>
                                    </Paper>
                                    </Grid>
                                </Grid>
                                </Paper>
                            </>
                        ) : ( 
                            <>
                                <Paper sx={{ p: 3, paddingTop: 0 }} elevation={2}>
                                <Grid container spacing={2} justifyContent={"flex-end"}>
                                    <Grid item xs={12} sm={6}>
                                    <TextField
                                        fullWidth
                                        label="Date From"
                                        type="date"
                                        InputLabelProps={{ shrink: true }}
                                    />
                                    </Grid>
                                    <Grid item xs={12} sm={6}>
                                    <TextField
                                        fullWidth
                                        label="Date To"
                                        type="date"
                                        InputLabelProps={{ shrink: true }}
                                    />
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="success" sx={buttonStyle}>
                                        Export CSV
                                    </Button>
                                    </Grid>
                                    <Grid item xs={12} sm={3}>
                                    <Button fullWidth variant="contained" color="primary" sx={buttonStyle}>
                                        Apply Filter
                                    </Button>
                                    </Grid>
                                </Grid>
                                </Paper>

                                <Paper sx={{ p: 3 }} elevation={2}>
                                <TableContainer component={Paper}>
                                    <Table>
                                    <TableHead>
                                        <TableRow>
                                        <TableCell>ID</TableCell>
                                        <TableCell>Changed At</TableCell>
                                        <TableCell>Action</TableCell>
                                        <TableCell>Changes</TableCell>
                                        <TableCell>Changed By</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {isLoading ? (
                                            <TableRow>
                                                <TableCell colSpan={5} align="center">Loading user changes...</TableCell>
                                            </TableRow>
                                        ) : userChangesList.length === 0 ? (
                                            <TableRow>
                                                <TableCell colSpan={5} align="center">No changes history available</TableCell>
                                            </TableRow>
                                        ) : (
                                            userChangesList.map((change) => (
                                                <TableRow key={change.id}>
                                                    <TableCell>{change.id}</TableCell>
                                                    <TableCell>{formatDate(change.changedAt)}</TableCell>
                                                    <TableCell>
                                                        <Chip 
                                                            label={change.action} 
                                                            color={
                                                                change.action === "Updated" ? "primary" : 
                                                                change.action === "Created" ? "success" : 
                                                                change.action === "Deleted" ? "error" : "default"
                                                            } 
                                                            size="small" 
                                                        />
                                                    </TableCell>
                                                    <TableCell>
                                                        {change.changes.split(';').map((changeItem, index) => (
                                                            <Typography key={index} variant="body2" sx={{ mb: 0.5 }}>
                                                                {changeItem.trim()}
                                                            </Typography>
                                                        ))}
                                                    </TableCell>
                                                    <TableCell>{change.changedByUserName}</TableCell>
                                                </TableRow>
                                            ))
                                        )}
                                    </TableBody>
                                    </Table>
                                </TableContainer>
                                </Paper>
                            </>
                        ))}
                    </div>
                </Box>
            </div>
        </div>
    );
};

export default UserProfile;