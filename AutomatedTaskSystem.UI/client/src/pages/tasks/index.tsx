import { useEffect, useState } from "react";
import API from "../../lib/API";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import TaskIcon from "../../assets/Icons/Task";
import Head from "next/head";
import Loader from "../../components/loader";
import { useRouter } from "next/router";

const projectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 70 },
    {
        field: "col1",
        headerName: "Name",
        width: 300,
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", width: 300 },
    { field: "col3", headerName: "Year", width: 100 },
    { field: "col4", headerName: "Term", width: 100 },
    { field: "col5", headerName: "Tasks Num", width: 100 }, // New column for task count
];

const sprintColumns: GridColDef[] = [
    { field: "id", headerName: "ID", width: 70 },
    { field: "name", headerName: "Name", width: 300 },
    { field: "startDate", headerName: "Start Date", width: 150 },
    { field: "endDate", headerName: "End Date", width: 150 },
    { field: "status", headerName: "Status", width: 120 },
];

const Projects = () => {
    const router = useRouter();
    const [view, setView] = useState<"projects" | "sprints">("projects");
    const [projects, setProjects] = useState<IProject[]>();
    const [sprints, setSprints] = useState<any[]>();

    useEffect(() => {
        API.TASKS.PROJECTS().then((res) => {
            if (res && !res.error) setProjects(res.data);
        });
    }, []);

    // Fetch sprints when switching to sprints view
    useEffect(() => {
        if (view === "sprints" && !sprints) {
            API.SPRINTS.GET_ALL_SPRINTS().then((res: any) => {
                if (res && !res.error) setSprints(res.data);
            });
        }
    }, [view, sprints]);

    const renderLoading = () => (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Loading</title>
            </Head>
            <Loader />
        </div>
    );

    if (view === "projects" && projects === undefined) {
        return renderLoading();
    }

    return (
        <>
            <Head>
                <title>
                    ATS - {view === "projects" ? "Projects" : "Sprints"}
                </title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <TaskIcon className="stroke-black" />
                        <h1 className="font-bold text-2xl ">
                            {view === "projects" ? "Projects" : "Sprints"}
                        </h1>
                    </div>
                    <div className="flex gap-4">
                        <button
                            onClick={() => setView("projects")}
                            className={`px-4 py-2 rounded-md font-semibold transition-colors ${
                                view === "projects"
                                    ? "bg-blue-600 text-white"
                                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                            }`}
                        >
                            Projects
                        </button>
                        <button
                            onClick={() => setView("sprints")}
                            className={`px-4 py-2 rounded-md font-semibold transition-colors ${
                                view === "sprints"
                                    ? "bg-blue-600 text-white"
                                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                            }`}
                        >
                            Sprints
                        </button>
                    </div>
                </div>
                {view === "projects" && projects && (
                    <div className="pb-4 mt-4">
                        <DataGrid
                            className="bg-white relative h-full"
                            initialState={{
                                sorting: {
                                    sortModel: [{ field: "col1", sort: "asc" }],
                                },
                            }}
                            slots={{
                                row: (r) => {
                                    return (
                                        <Link
                                            href={{
                                                pathname: `/tasks/${r.rowId}`,
                                            }}
                                        >
                                            <div
                                                key={r.rowId}
                                                style={{ height: r.rowHeight }}
                                                className="group hover:bg-slate-50 flex border-solid border-b border-slate-200 cursor-pointer"
                                            >
                                                {r.visibleColumns.map(
                                                    (c: any) => {
                                                        if (c.field === "col1")
                                                            return (
                                                                <div
                                                                    key={
                                                                        c.headerName
                                                                    }
                                                                    style={{
                                                                        minWidth:
                                                                            c.width,
                                                                        maxWidth:
                                                                            c.width,
                                                                    }}
                                                                    className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-base flex items-center group-hover:text-blue-700"
                                                                >
                                                                    {
                                                                        r.row[
                                                                            c
                                                                                .field
                                                                        ]
                                                                    }
                                                                </div>
                                                            );

                                                        return (
                                                            <div
                                                                key={
                                                                    c.headerName
                                                                }
                                                                style={{
                                                                    minWidth:
                                                                        c.width,
                                                                    maxWidth:
                                                                        c.width,
                                                                }}
                                                                className="px-[0.625rem] group-hover:pl-4 transition-all ease-in text-sm flex items-center group-hover:text-blue-700"
                                                            >
                                                                {
                                                                    r.row[
                                                                        c.field
                                                                    ]
                                                                }
                                                            </div>
                                                        );
                                                    }
                                                )}
                                            </div>
                                        </Link>
                                    );
                                },
                            }}
                            rows={projects.map((p: any) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year.name,
                                    col4: p.term ? "Term 2" : "Term 1",
                                    col5: p.count || 0, // Add the task count here
                                };
                            })}
                            columns={projectColumns}
                            autoHeight
                        />
                    </div>
                )}

                {view === "sprints" && (
                    <div className="pb-4 mt-4">
                        {sprints === undefined ? (
                            renderLoading()
                        ) : (
                            <>
                                <h2 className="text-xl font-semibold mb-2 px-1">
                                    Select a Sprint
                                </h2>
                                <DataGrid
                                    className="bg-white relative h-full"
                                    rows={sprints.map((s: any) => ({
                                        id: s.id,
                                        name: s.name,
                                        startDate: new Date(
                                            s.startDate
                                        ).toLocaleDateString(),
                                        endDate: new Date(
                                            s.endDate
                                        ).toLocaleDateString(),
                                        status: s.status,
                                    }))}
                                    columns={sprintColumns}
                                    onRowClick={(params) =>
                                        router.push(`/tasks/sprint/${params.id}`)
                                    }
                                    getRowClassName={(params) =>
                                        `cursor-pointer hover:bg-gray-50`
                                    }
                                    autoHeight
                                />
                            </>
                        )}
                    </div>
                )}
            </div>
        </>
    );
};



export default Projects;
