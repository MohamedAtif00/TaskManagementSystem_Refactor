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

const Sprints = () => {
    const [sprints, setSprints] = useState<ISprint[]>();
    const [activeTab, setActiveTab] = useState<'active' | 'archived'>('active');

    // Fetch Sprints based on active tab
    const fetchSprints = (archived: boolean) => {
        API.SPRINTS.GET_ALL_SPRINTS(archived).then((res) => {
            if (res && !res.error && res.data) {
                setSprints(res.data);
            } else {
                console.error("Error fetching sprints:", res?.message || "Unknown error");
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

    // Columns for Sprints with Actions column
    const sprintColumns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 90 },
        { field: "col1", headerName: "Sprint Name", width: 300 },
        { field: "col2", headerName: "Start Date", width: 200 },
        { field: "col3", headerName: "End Date", width: 200 },
        {
            field: "actions",
            headerName: "Actions",
            width: 100,
            sortable: false,
            renderCell: (params: GridRenderCellParams) => {
                const sprint = sprints?.find(s => s.id === params.row.id);
                const isArchived = sprint?.isArchived || false;

                return (
                    <div className="flex items-center justify-center h-full">
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