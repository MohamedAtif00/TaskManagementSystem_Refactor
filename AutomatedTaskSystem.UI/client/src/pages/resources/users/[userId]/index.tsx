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

const UserProfile = () => {
    const router = useRouter();
    const { userId } = router.query;

    const [user, setUser] = useState<any>(null); // Replace `any` with your type
    const [vacancies, setVacancies] = useState<IVacation | null>(null);
    const [view,setView] = useState<"vacancies"|"updates">("vacancies")

    useEffect(() => {
    debugger
    console.log(router.query);
    
        if (userId) {
            API.RESOURCES.USERS.GET_ONE(Number( userId)).then((res) => {
                if (res && !res.error) {
                    setUser(res.data);
                    setVacancies(res.data.vacation??null);
                }
            });
        }
    }, [router.isReady]);
    
    
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
                        <Typography variant="p"  className="flex items-center gap-2 font-bold text-lg">
                        {user.name}
                        </Typography>
                        {user.code && (
                            <Typography className="flex items-center gap-2  text-[#22648C] text-[14px]">
                              {user.code}
                            </Typography>
                        )}
                        <Grid className="flex justify-between">
                            <Grid className="w-1/2">

                            <Typography className="flex items-center gap-2 text-gray-700">
                                <span className="font-bold"> Department:</span> 
                                <span className="text-[#5570FF]">{user.group?.name}</span>
                            </Typography>
                            <Typography className="flex items-center gap-2 text-gray-700">
                                <span className="font-bold"> Role: </span>
                                <span className="text-[#5570FF]">{user.role === 0 ?    
                                    "Project Manager": user.role === 1?
                                    "Section Head":user.role ===2 ?"Team Leader"
                                    :"Member"}
                                </span>
                            </Typography>
                            

                            </Grid>
                            <Grid className="w-full ms-10">

                                <Typography className="flex  gap-2 text-gray-700 ">
                                    <span className="font-bold">Type:</span> 
                                    <span className="text-[#5570FF]">
                                        {user.accountType === 0 ? "External":"Enternal"}
                                    </span>
                                </Typography>
                                <Typography className="flex  gap-2 text-gray-700 ">
                                    <span className="font-bold"> Email: </span>
                                    <span className="text-[#5570FF]">{user.email}
                                    </span>
                                </Typography>

                            </Grid>
                            
                        </Grid>
                    </Box>

                    {/* Leave Info Section */}
                    <div className="flex justify-between  items-end border-t-2 border-gray-300 pt-4  w-full md:w-1/3 "
                    style={{borderTop:"1px solid #D1D5DB"}}
                    >

                        {[
                        { label: "Annual", value: vacancies?.annual, total: 20 },
                        { label: "Sick", value: vacancies?.sick, total: 5 },
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
                            <Typography variant="p" className="font-bold text-gray-800">
                                {leave.label}
                            </Typography>
                            <Typography variant="p" className="text-gray-600 font-light">
                            <span className="text-blue-500 font-bold text-lg">{leave.value}</span> / {leave.total}
                            </Typography>
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
                                        {}
                                    </TableBody>
                                    </Table>
                                </TableContainer>
                                </Paper>
                            </>
                            ):( <>
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
                                    <Button fullWidth variant="contained" color="primary" sx={buttonStyle }>
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
                                    </Table>
                                </TableContainer>
                                </Paper>
                            </>)}

                    </div>
                </Box>

            </div>
        </div>
    );
};

export default UserProfile;
