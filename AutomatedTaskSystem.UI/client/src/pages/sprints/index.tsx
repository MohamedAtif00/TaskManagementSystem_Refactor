import { useEffect, useState } from "react";
import API from "../../lib/API";
import Link from "next/link"; // Ensure Link is imported
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task"; // Adjust path as needed
import TrashIcon from "../../assets/Icons/Trash";
import ArchiveIcon from "../../assets/Icons/Archive";
import RotatingArrowsIcon from "../../assets/Icons/RotatingArrows";
import Head from "next/head";
import Loader from "../../components/loader"; // Adjust path as needed
import CreateSprint from "../../components/sprintComponents/createSprint"; // Adjust path as needed
import { format } from "date-fns";
import SprintEye from "../../assets/Icons/SprintEye";
import SprintChart from "../../assets/Icons/SprintChart";
import { PieChart } from '@mui/x-charts/PieChart';
import { Box, Typography } from "@mui/material";
import { useRouter } from "next/router";
import { GetAllSprintsResponse } from "../../lib/API/Sprints.d";



const Sprints = () => {
    const [sprints, setSprints] = useState<GetAllSprintsResponse[]>();
    const [activeTab, setActiveTab] = useState<'active' | 'archived'>('active');
    const router = useRouter();

    // Fetch Sprints based on active tab
    const fetchSprints = (archived: boolean) => {
        API.SPRINTS.GET_ALL_SPRINTS(archived).then((res) => {
            if (res && !res.error && res.data) {
                setSprints(res.data);
            } else {
                console.error("Error fetching sprints:", (res) ? res.message : "Unknown error");
                setSprints([]); // Set to empty array on error to stop loading
            }
        });
    };

    // Fetch Sprints when tab changes
    useEffect(() => {
        setSprints(undefined); // Reset to show loading
        fetchSprints(activeTab === 'archived');
    }, [activeTab]);

    // Handle archive/unarchive action
    const handleArchiveToggle = async (sprintId: number, currentlyArchived: boolean) => {
        const newArchivedState = !currentlyArchived;
        const result = await API.SPRINTS.ARCHIVE_SPRINT(sprintId, newArchivedState);

        if (result && !result.error) {
            // Refresh the sprint list
            fetchSprints(activeTab === 'archived');
        } else {
            console.error("Error archiving sprint:", result?.message || "Unknown error");
            alert(`Failed to ${newArchivedState ? 'archive' : 'unarchive'} sprint`);
        }
    };

    const handleRoutingToSprintDetail = (sprintId: number) => {
        window.location.href = `/sprints/${sprintId}`;
    }

    const handleRoutingToSprintCharts = (sprintChartId: number) => {
        router.push(`/sprints/charts/${sprintChartId}`);
    }


    // Columns for Sprints with Actions column
    const sprintColumns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 90 },
        { field: "col1", headerName: "Name", width: 300 },
        { field: "col2", headerName: "Start Date", width: 100 },
        { field: "col3", headerName: "End Date", width: 100 },
        { field: "col4", headerName: "Number of LO", width: 120 },
        {
            field: "col5",
            headerName: "Progress",
            width: 100 ,
            renderCell: (params: GridRenderCellParams) => {
    const sprint = sprints?.find(s => s.id === params.row.id);
    const percentage = sprint?.completePercintag || 0;

    // Define colors based on your thresholds
    const getStatusColor = (perc: number) => {
        if (perc > 90) return '#22c55e'; // Green
        if (perc > 70) return '#f59e0b'; // Amber
        return '#ef4444';                // Red
    };

    const verticalCenter = '38%';

    const progressData = [
        { label: 'Done', value: percentage, color: getStatusColor(percentage) },
        { label: 'Pending', value: 100 - percentage, color: '#e5e7eb' },
    ];

    return (
        /* 1. Relative container limited to the chart's width/height */
        <Box 
            className="relative flex items-center justify-center" 
            sx={{ width: 70, height: 70, margin: 'auto' }}
        >
            {/* 2. The Donut Chart */}
            <PieChart
                series={[
                    {
                        innerRadius: 12, // Adjusted for 70px scale
                        outerRadius: 22,
                        data: progressData,
                        cx: '50%',
                        cy: verticalCenter,
                        // Using standard 0 to 360 for a clean circular fill
                        startAngle: -130,
                        endAngle: 230,
                        paddingAngle: 0,
                    },
                ]}
                hideLegend
                width={70}
                height={70}
                slotProps={{ tooltip: { trigger: 'none' } }}
                margin={{ top: 0, bottom: 0, left: 0, right: 0 }}
            />

            {/* 3. The Centered Text Overlay */}
            <Box
                className="absolute inset-0 flex items-center justify-center"
                sx={{ 
                    pointerEvents: 'none',
                    left: '50%',
                    top: verticalCenter, // Match the chart's cy
                    transform: 'translate(-50%, -50%)', // Keeps it perfectly centered on the point
                    width: '100%',
                }}
            >
                <Typography 
                    sx={{ 
                        fontSize: '10px', 
                        fontWeight: 'bold',
                        lineHeight: 1 
                    }}
                >
                    {/* Using toFixed(0) to show 98% instead of 98.34... */}
                    {percentage.toFixed(0)}%
                </Typography>
            </Box>
        </Box>
    );
},
        },
        {
            field: "actions",
            headerName: "Actions",
            width: 150,
            sortable: false,
            renderCell: (params: GridRenderCellParams) => {
                const sprint = sprints?.find(s => s.id === params.row.id);
                const isArchived = sprint?.isArchived || false;

                return (
                    <div className="flex items-center justify-center h-full">
                        <button
                            onClick={(e) => {
                                e.stopPropagation();
                                handleRoutingToSprintDetail(params.row.id);
                            }}
                            className="p-2 rounded-md hover:bg-gray-100 transition-colors group/btn"
                            title={activeTab === 'archived' ? 'Unarchive Sprint' : 'Archive Sprint'}
                        >

                            <SprintEye  />
                            
                        </button>
                        <button
                            onClick={(e) => {
                                e.stopPropagation();
                                handleRoutingToSprintCharts(params.row.id);
                            }}
                            className="p-2 rounded-md hover:bg-gray-100 transition-colors group/btn"
                            title={activeTab === 'archived' ? 'Unarchive Sprint' : 'Archive Sprint'}
                        >
                            <SprintChart></SprintChart>
                        </button>
                        <button
                            onClick={(e) => {
                                e.stopPropagation();
                                handleArchiveToggle(params.row.id, isArchived);
                            }}
                            className="p-2 rounded-md hover:bg-gray-100 transition-colors group/btn"
                            title={activeTab === 'archived' ? 'Unarchive Sprint' : 'Archive Sprint'}
                        >
                            {activeTab === 'archived' ? (
                                <RotatingArrowsIcon className="w-5 h-5 fill-green-600 group-hover/btn:fill-green-700" />
                            ) : (
                                <TrashIcon className="w-5 h-5 fill-red-600 group-hover/btn:fill-red-700" />
                            )}
                        </button>
                    </div>
                );
            },
        },
    ];

    if (sprints === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    return (
        <>
            <Head>
                <title>ATS - Sprints</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <TaskIcon className="stroke-black" />
                        <h1 className="font-bold text-2xl">Sprints</h1>
                    </div>
                    {/* Fixed: Link wrapping a button for navigation */}
                    <Link
                        href={{
                            pathname: "/sprints",
                            query: {
                                form: "create-sprint",
                            },
                        }}
                    >
                        <button className="px-4 py-1 rounded bg-blue-600 text-white">
                            Add Sprint
                        </button>
                    </Link>
                </div>

                {/* Tabs for Active and Archived Sprints */}
                <div className="flex gap-4 mt-4 mb-2 px-1">
                    <button
                        onClick={() => setActiveTab('active')}
                        className={`px-6 py-2 rounded-md font-semibold transition-colors ${
                            activeTab === 'active'
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                        }`}
                    >
                        Active Sprints
                    </button>
                    <button
                        onClick={() => setActiveTab('archived')}
                        className={`px-6 py-2 rounded-md font-semibold transition-colors ${
                            activeTab === 'archived'
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                        }`}
                    >
                        Archived Sprints
                    </button>
                </div>
                <div className="pb-4 mt-4">
                    <DataGrid
                        className="bg-white relative h-full"
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                        rows={sprints.map((s) => {
                            return {
                                id: s.id,
                                col0: s.id,
                                col1: s.name,
                                col2: format(new Date(s.startDate), 'yyyy-MM-dd'),
                                col3: format(new Date(s.endDate), 'yyyy-MM-dd'),
                                col4: s.loNumber,
                                col5: s.completePercintag,
                                actions: s.id, // Pass the id for the actions column
                            };
                        })}
                        columns={sprintColumns}
                        onRowClick={(params) => {
                            // Navigate to sprint detail page when clicking on the row (but not the actions button)
                            window.location.href = `/sprints/${params.id}`;
                        }}
                        sx={{
                            '& .MuiDataGrid-row': {
                                cursor: 'pointer',
                                '&:hover': {
                                    backgroundColor: 'rgba(0, 0, 0, 0.04)',
                                },
                            },
                        }}
                    />
                </div>
                <CreateSprint />
            </div>
        </>
    );
};

export default Sprints;