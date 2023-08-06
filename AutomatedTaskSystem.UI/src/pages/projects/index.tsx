import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { useRouter } from "next/router";
import ProjectIcon from "../../assets/Icons/Project";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import AddProject from "../../components/pageComponent/projects/addProject";
import EditProject from "../../components/pageComponent/projects/editProject";
import RemoveProject from "../../components/pageComponent/projects/removeProject";
import TableAction from "../../components/TableComponents/TableActionButton";
import Head from "next/head";
import Loader from "../../components/loader";
import { load } from "../../slices/projectSlice";
import HoldProject from "../../components/pageComponent/projects/holdProject";
import CloseProject from "../../components/pageComponent/projects/closeProject";
import ActivateProject from "../../components/pageComponent/projects/activeProject";

const columnsForDisabled: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    {
        field: "col1",
        headerName: "Name",
        width: 200,
        sortComparator: (A, B) => {
            const a = A.toLowerCase(),
                b = B.toLowerCase();
            return a > b ? 1 : b > a ? -1 : 0;
        },
    },
    { field: "col2", headerName: "Description", width: 300 },
    { field: "col3", headerName: "Year", width: 100 },
    { field: "col4", headerName: "Term", width: 100 },
    {
        field: "col5",
        headerName: "Actions",
        width: 220,
        renderCell: (c) => (
            <div className="flex justify-end gap-4">
                <TableAction
                    text="View"
                    url={{
                        pathname: `/projects/${c.id}`,
                    }}
                    type="eye"
                />
                <TableAction
                    text="Edit"
                    url={{
                        pathname: "/projects",
                        query: {
                            form: "edit-project",
                            projectId: c.id,
                        },
                    }}
                    type="edit"
                />
                <TableAction
                    text="Archive"
                    url={{
                        pathname: `/projects`,
                        query: {
                            form: "remove-project",
                            projectId: c.id,
                        },
                    }}
                    type="archive"
                />
                <TableAction
                    text="Activate"
                    url={{
                        pathname: `/projects`,
                        query: {
                            form: "activate-project",
                            projectId: c.id,
                        },
                    }}
                    type="resume"
                />
            </div>
        ),
        filterable: false,
        disableColumnMenu: true,
        sortable: false,
    },
];

const columns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    {
        field: "col1",
        headerName: "Name",
        width: 200,
    },
    { field: "col2", headerName: "Description", width: 300 },
    { field: "col3", headerName: "Year", width: 100 },
    { field: "col4", headerName: "Term", width: 100 },
    {
        field: "col5",
        headerName: "Actions",
        width: 260,
        renderCell: (c) => (
            <div className="flex justify-end gap-4">
                <TableAction
                    text="View"
                    url={{
                        pathname: `/projects/${c.id}`,
                    }}
                    type="eye"
                />
                <TableAction
                    text="Edit"
                    url={{
                        pathname: "/projects",
                        query: {
                            form: "edit-project",
                            projectId: c.id,
                        },
                    }}
                    type="edit"
                />
                <TableAction
                    text="Archive"
                    url={{
                        pathname: `/projects`,
                        query: {
                            form: "remove-project",
                            projectId: c.id,
                        },
                    }}
                    type="archive"
                />
                <TableAction
                    text="Hold"
                    url={{
                        pathname: `/projects`,
                        query: {
                            form: "hold-project",
                            projectId: c.id,
                        },
                    }}
                    type="pause"
                />
                <TableAction
                    text="Close"
                    url={{
                        pathname: `/projects`,
                        query: {
                            form: "close-project",
                            projectId: c.id,
                        },
                    }}
                    type="stop"
                />
            </div>
        ),
        filterable: false,
        disableColumnMenu: true,
        sortable: false,
    },
];

const Tab = ({
    active,
    label,
    onClick,
}: {
    active?: boolean;
    label: string;
    onClick: () => void;
}) => (
    <div
        onClick={onClick}
        className={`border border-solid border-b-0 border-slate-400 rounded-t px-4 transition-all w-28 flex items-center justify-center ease-in ${
            active ? "py-1 bg-white font-bold" : "cursor-pointer py-0"
        }`}
    >
        {label}
    </div>
);

const Projects = () => {
    const projects = useAppSelector((s) => s.projectSlice);
    const dispatch = useAppDispatch();
    const [view, setView] = useState<"active" | "hold" | "closed">("active");
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();

    if (!auth.isAuth || auth.role != 1) router.replace("/");

    useEffect(() => {
        API.PROJECTS.GET_ALL().then((res) => {
            if (res && !res.error) {
                dispatch(load(res.data));
            }
        });
    }, [dispatch]);

    if (projects === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <Head>
                <title>
                    ATS -{" "}
                    {view === "hold"
                        ? "Projects (On Hold)"
                        : view === "closed"
                        ? "Projects (Closed)"
                        : "Projects"}
                </title>
            </Head>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex gap-2 items-center">
                    <div className="w-6 h-6">
                        <ProjectIcon />
                    </div>
                    <h1 className="font-bold text-2xl ">Project</h1>
                </div>
                <Link
                    href={{
                        pathname: "/projects",
                        query: {
                            form: "add-project",
                        },
                    }}
                >
                    <button className="px-4 py-1 rounded bg-blue-600 text-white">
                        Add Project
                    </button>
                </Link>
            </div>
            <div className="pb-4 mt-4">
                <div className="flex gap-2 items-end h-9">
                    <Tab
                        label="Active"
                        active={view === "active"}
                        onClick={() => setView("active")}
                    />
                    <Tab
                        label="On Hold"
                        active={view === "hold"}
                        onClick={() => setView("hold")}
                    />
                    <Tab
                        label="Closed"
                        active={view === "closed"}
                        onClick={() => setView("closed")}
                    />
                </div>
                {view === "active" ? (
                    <DataGrid
                        className="bg-white relative h-full"
                        rows={projects
                            .filter((p) => p.status === 1 || p.status === 4)
                            .map((p) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year.name,
                                    col4: p.term ? "Term 2" : "Term 1",
                                };
                            })}
                        columns={columns}
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                    />
                ) : view === "closed" ? (
                    <DataGrid
                        className="bg-white relative h-full"
                        rows={projects
                            .filter((p) => p.status === 2)
                            .map((p) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year.name,
                                    col4: p.term ? "Term 2" : "Term 1",
                                };
                            })}
                        columns={columnsForDisabled}
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                    />
                ) : (
                    <DataGrid
                        className="bg-white relative h-full"
                        rows={projects
                            .filter((p) => p.status === 3)
                            .map((p) => {
                                return {
                                    id: p.id,
                                    col0: p.id,
                                    col1: p.name,
                                    col2: p.description,
                                    col3: p.year.name,
                                    col4: p.term ? "Term 2" : "Term 1",
                                };
                            })}
                        columns={columnsForDisabled}
                        initialState={{
                            sorting: {
                                sortModel: [{ field: "col1", sort: "asc" }],
                            },
                        }}
                    />
                )}
            </div>
            <AddProject />
            <EditProject />
            <HoldProject />
            <ActivateProject />
            <CloseProject />
            <RemoveProject />
        </div>
    );
};

export default Projects;
