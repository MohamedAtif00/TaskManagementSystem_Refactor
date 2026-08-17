import { useCallback, useEffect, useMemo, useState } from "react";
import API from "../../lib/API";
import Link from "next/link";
import { DataGrid, GridColDef, GridRenderCellParams } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task";
import TrashIcon from "../../assets/Icons/Trash";
import ArchiveIcon from "../../assets/Icons/Archive";
import RotatingArrowsIcon from "../../assets/Icons/RotatingArrows";
import Head from "next/head";
import Loader from "../../components/loader";
import CreateSprint from "../../components/sprintComponents/createSprint";
import ProgressDonut from "../../components/charts/ProgressDonut";
import { format } from "date-fns";
import SprintEye from "../../assets/Icons/SprintEye";
import PieChartIcon from "../../assets/Icons/PieChart";
import { useRouter } from "next/router";
import { GetAllSprintsResponse } from "../../lib/API/Sprints.d";
import { useAppSelector } from "../../app/hooks";
import CurriculumPathFilters from "../../components/curriculum/CurriculumPathFilters";
import { useCurriculumPathFilters } from "../../hooks/useCurriculumPathFilters";
import { sprintMatchesProjectFilter } from "../../lib/curriculumHierarchy";

const Sprints = () => {
    const [sprints, setSprints] = useState<GetAllSprintsResponse[]>([]);
    const [subjects, setSubjects] = useState<IProject[]>([]);
    const [fetchError, setFetchError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'active' | 'archived'>('active');
    const router = useRouter();
    const { role } = useAppSelector((s) => s.authSlice);
    const {
        filters,
        filterLabels,
        filterOptions,
        updateFilter,
    } = useCurriculumPathFilters(subjects);

    const hierarchyFilter = useMemo(
        () => ({
            yearName: filters[0] || undefined,
            projectName: filters[1] || undefined,
            termName: filters[2] || undefined,
            subjectGroupName: filters[3] || undefined,
        }),
        [filters]
    );

    const [confirmModal, setConfirmModal] = useState<{
    open: boolean;
    sprintId: number | null;
    isArchived: boolean;
    } >({ open: false, sprintId: null, isArchived: false });

    useEffect(() => {
        API.PROJECTS.GET_ALL_FOR_SPRINT().then((res) => {
            if (res && !res.error && Array.isArray(res.data)) {
                setSubjects(res.data);
            }
        });
    }, []);

    const fetchSprints = useCallback(async (archived: boolean, filter = hierarchyFilter) => {
        setFetchError(null);
        setIsLoading(true);
        try {
            const res = await API.SPRINTS.GET_ALL_SPRINTS(archived, filter);
            if (res && !res.error && res.data) {
                setSprints(res.data);
            } else {
                const message = res && "message" in res && res.message ? res.message : "Unknown error";
                console.error("Error fetching sprints:", message);
                setFetchError(message);
                setSprints([]);
            }
        } finally {
            setIsLoading(false);
        }
    }, [hierarchyFilter]);

    useEffect(() => {
        fetchSprints(activeTab === 'archived');
    }, [activeTab, fetchSprints]);

    const handleArchiveToggle = (sprintId: number, currentlyArchived: boolean) => {
        setConfirmModal({ open: true, sprintId, isArchived: currentlyArchived });
    };

    const confirmArchiveToggle = async () => {
        if (confirmModal.sprintId === null) return;
        const newArchivedState = !confirmModal.isArchived;
        const result = await API.SPRINTS.ARCHIVE_SPRINT(confirmModal.sprintId, newArchivedState);
        setConfirmModal({ open: false, sprintId: null, isArchived: false });
        if (result && !result.error) {
            fetchSprints(activeTab === 'archived');
        } else {
            console.error("Error archiving sprint:", result?.message || "Unknown error");
            alert(`Failed to ${newArchivedState ? 'archive' : 'unarchive'} sprint`);
        }
    };

    const handleRoutingToSprintDetail = (sprintId: number) => {
        router.push(`/sprints/${sprintId}`);
    }

    const handleRouteToSprintTask = (sprintId: number) => {
        router.push(`/tasks/sprint/${sprintId}/board`);
    }

    const handleRoutingToSprintCharts = (sprintChartId: number) => {
        router.push(`/sprints/charts/${sprintChartId}`);
    }

    const createSprintQuery = useMemo(() => {
        const query: Record<string, string> = { form: "create-sprint" };
        if (filters[0]) query.yearName = filters[0];
        if (filters[1]) query.projectName = filters[1];
        if (filters[2]) query.termName = filters[2];
        if (filters[3]) query.subjectGroupName = filters[3];
        return query;
    }, [filters]);

    const sprintFilterOptions = useMemo(() => {
        const options = filterOptions.map((opts) => [...opts]);
        while (options.length < 2) {
            options.push([]);
        }

        const fromSprints = sprints.flatMap((sprint) => sprint.projectNames ?? []).filter(Boolean);
        if (fromSprints.length === 0) {
            return options;
        }

        options[1] = Array.from(new Set([...options[1], ...fromSprints]))
            .sort((a, b) => a.localeCompare(b));
        return options;
    }, [filterOptions, sprints]);

    const visibleSprints = useMemo(
        () => sprints.filter((sprint) => sprintMatchesProjectFilter(sprint.projectNames, filters[1])),
        [filters, sprints]
    );

    const sprintColumns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 90 },
        { field: "col1", headerName: "Name", width: 260 },
        {
            field: "colProject",
            headerName: "Project",
            width: 180,
        },
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

            return <ProgressDonut percentage={percentage} size={70} />;
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
                        {
                            !isArchived && (
                                <>
                                    <button
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleRouteToSprintTask(params.row.id);
                                        }}
                                        className="p-2 rounded-md hover:bg-gray-100 transition-colors group/btn"
                                        title='Show Sprint Task Board'
                                    >

                                        <SprintEye  />
                                        
                                    </button>

                                </>
                            )
                        }
                        <button
                            onClick={(e) => {
                                e.stopPropagation();
                                handleRoutingToSprintCharts(params.row.id);
                            }}
                            className="p-2 rounded-md hover:bg-gray-100 transition-colors group/btn"
                            title='Show Sprint Charts'
                        >
                            <PieChartIcon></PieChartIcon>
                        </button>
                        {(role == 0 || role == 4) && (
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

                        )}
                    </div>
                );
            },
        },
    ];

    if (isLoading && sprints.length === 0 && !fetchError)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    return (
        <>
            <Head>
                <title>TMS - Sprints</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <TaskIcon className="stroke-black" />
                        <h1 className="font-bold text-2xl">Sprints</h1>
                    </div>
                    {(role === 0 || role === 4) &&
                        <Link
                            href={{
                                pathname: "/sprints",
                                query: createSprintQuery,
                            }}
                        >
                            <button className="px-4 py-1 rounded bg-blue-600 text-white">
                                Add Sprint
                            </button>
                        </Link>

                    }
                </div>

                <div className="flex gap-4 mt-4 mb-2 px-1">
                    {(role == 0 || role == 4) && (
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
                    )}
                    {(role == 0 || role == 4) && (
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

                    )}
                </div>

                <div className="mt-2 mb-4 rounded-lg border border-slate-200 bg-white p-4">
                    <CurriculumPathFilters
                        filterLabels={filterLabels}
                        filterOptions={sprintFilterOptions}
                        filters={filters}
                        onFilterChange={updateFilter}
                    />
                </div>

                <div className="pb-4 mt-4">
                    {fetchError && (
                        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-red-700">
                            Failed to load sprints: {fetchError}
                        </div>
                    )}
                    <DataGrid
                        className="bg-white relative h-full"
                        loading={isLoading}
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                        rows={visibleSprints.map((s) => {
                            return {
                                id: s.id,
                                col0: s.id,
                                col1: s.name,
                                colProject: (s.projectNames ?? []).join(", ") || "—",
                                col2: format(new Date(s.startDate), 'yyyy-MM-dd'),
                                col3: format(new Date(s.endDate), 'yyyy-MM-dd'),
                                col4: s.loNumber,
                                col5: s.completePercintag,
                                actions: s.id,
                            };
                        })}
                        columns={sprintColumns}
                        onRowClick={(params) => {
                            if(role === 0 || role === 4)
                            handleRoutingToSprintDetail(params.id as number);
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
                <CreateSprint onCreated={() => fetchSprints(activeTab === 'archived')} />
            </div>


            {confirmModal.open && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
                <div className="bg-white rounded-2xl shadow-xl p-8 max-w-sm w-full mx-4 flex flex-col items-center gap-4">
                <div className={`w-14 h-14 rounded-full flex items-center justify-center ${confirmModal.isArchived ? 'bg-blue-100' : 'bg-amber-100'}`}>
                    {confirmModal.isArchived
                    ? <RotatingArrowsIcon className="w-7 h-7 text-blue-600" />
                    : <ArchiveIcon className="w-7 h-7 text-amber-600" />
                    }
                </div>
                <h2 className="text-lg font-bold text-gray-800">
                    {confirmModal.isArchived ? 'Restore Sprint?' : 'Archive Sprint?'}
                </h2>
                <p className="text-sm text-gray-500 text-center">
                    {confirmModal.isArchived
                    ? 'This sprint will be moved back to active sprints and become editable again.'
                    : 'This sprint will be archived and hidden from the active list. You can restore it later.'}
                </p>
                <div className="flex gap-3 w-full mt-2">
                    <button
                    onClick={() => setConfirmModal({ open: false, sprintId: null, isArchived: false })}
                    className="flex-1 px-4 py-2 rounded-lg border border-gray-200 text-gray-700 font-medium hover:bg-gray-50 transition-colors"
                    >
                    Cancel
                    </button>
                    <button
                    onClick={confirmArchiveToggle}
                    className={`flex-1 px-4 py-2 rounded-lg font-medium text-white transition-colors ${
                        confirmModal.isArchived
                        ? 'bg-blue-600 hover:bg-blue-700'
                        : 'bg-amber-500 hover:bg-amber-600'
                    }`}
                    >
                    {confirmModal.isArchived ? 'Restore' : 'Archive'}
                    </button>
                </div>
                </div>
            </div>
            )}
        </>
    );
};

export default Sprints;
