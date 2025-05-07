import { Tab } from "@headlessui/react";
import { Box, Paper, Typography, Grid, TextField, FormControl, InputLabel, Select, MenuItem, Button, TableContainer, Table, TableHead, TableRow, TableCell, TableBody } from "@mui/material";
import SchemaIcon from "../../assets/Icons/Schema";
import UserProfileIcone from "../../assets/Icons/UserProfile";
import { CSSProperties, useState } from "react";


const buttonStyle: CSSProperties = {
    paddingLeft: "55px",
    paddingRight: "55px",
    paddingTop: "7px",
    paddingBottom: "7px",
    textTransform: "none",
    borderRadius: "8px",
  };

const tableContainerClassStyle = "bg-card p-6 rounded-lg shadow-sm overflow-x-auto"
const tableCellClassStyle = "text-left p-3"
const tableRowClassStyle = "p-3"




export default function Calendar(){

    const [vacancies, setVacancies] = useState<IVacation | null>(null);

    return(
        <>
       <div className="w-full flex align-middle justify-center">
            <div className="w-10/12 ">  
                <Box sx={{paddingTop:2,paddingX:4, borderRadius: 1, marginBottom: 2 }}>
                <Paper sx={{ p: 3 }} elevation={2} className="flex ">
                    
                    <UserProfileIcone/>
                    <Typography variant="h5" fontWeight="bold" gutterBottom>
                        Calendar
                    </Typography>
                    </Paper>
                </Box>

                {/* Add background color to only the header */}
                <Box sx={{ p: 4, borderRadius: 2, display: 'flex', flexDirection: 'column', gap: 4 }}>
                    {/* User Profile Paper */}
                    <Paper elevation={3} sx={{ p: 3 }} className="flex flex-col md:flex-row justify-between gap-4 bg-white shadow-md rounded-xl">
                    {/* User Info Section */}
                    <Box className="space-y-2   rounded-lg border border-gray-200 w-full md:w-2/3">
                        
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
                        {/* Filters Paper */}
                        
                            
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
                        {/* </Paper>

                        <Paper sx={{ p: 3 }} elevation={2}> */}
                            <TableContainer component={Paper} className={tableContainerClassStyle}>
                                <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell className={tableCellClassStyle}>ID</TableCell>
                                        <TableCell className={tableCellClassStyle}>Request Date</TableCell>
                                        <TableCell className={tableCellClassStyle}>Vacancy Type</TableCell>
                                        <TableCell className={tableCellClassStyle}>Status</TableCell>
                                        <TableCell className={tableCellClassStyle}>Vacancy Date</TableCell>
                                        <TableCell className={tableCellClassStyle} align="center">Actions</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {}
                                </TableBody>
                                </Table>
                            </TableContainer>
                        </Paper>
                        
                            

                    </div>
                </Box>

            </div>
        </div>
        </>
    )
}