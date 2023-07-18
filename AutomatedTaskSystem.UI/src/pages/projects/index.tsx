import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { load } from "../../slices/projectSlice";
import { useRouter } from "next/router";
import ProjectIcon from "../../assets/Icons/Project";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import AddProject from "../../components/pageComponent/projects/addProject";
import EditProject from "../../components/pageComponent/projects/editProject";
import RemoveProject from "../../components/pageComponent/projects/removeProject";
import TableAction from "../../components/TableComponents/TableActionButton";

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
        width: 170,
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
            </div>
        ),
        filterable: false,
        disableColumnMenu: true,
        sortable: false,
    },
];

const Projects = () => {
    const projects = useAppSelector((states) => states.projectSlice);
    const dispatch = useAppDispatch();
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

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
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
                <DataGrid
                    className="bg-white relative h-full"
                    rows={projects.map((p) => {
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
                />
            </div>
            <AddProject />
            <EditProject />
            <RemoveProject />
        </div>
    );
};

export default Projects;
