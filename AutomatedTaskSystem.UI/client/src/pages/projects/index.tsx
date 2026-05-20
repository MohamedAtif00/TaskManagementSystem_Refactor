import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import Loader from "../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../components/curriculum/HierarchyListPage";
import { hierarchyActionsColumn } from "../../components/curriculum/hierarchyColumns";
import AddRootProject from "../../components/pageComponent/projects/addRootProject";
import EditRootProject from "../../components/pageComponent/projects/editRootProject";

type RootProjectRow = { id: number; name: string; description: string };

const projectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Name", width: 280 },
    { field: "col2", headerName: "Description", width: 360 },
    hierarchyActionsColumn,
];

const ProjectsIndex = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const [roots, setRoots] = useState<RootProjectRow[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    useEffect(() => {
        API.PROJECTS.ROOT.LIST().then((res) => {
            if (res && typeof res === "object" && "error" in res && !res.error && "data" in res) {
                setRoots((res as { data: RootProjectRow[] }).data);
            }
            setLoading(false);
        });
    }, [router.asPath]);

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    return (
        <>
            <HierarchyListPage
                title="Projects"
                pageTitle="Projects"
                addLabel="Add Project"
                addHref={{ pathname: "/projects", query: { form: "add-root" } }}
                rows={roots.map((r) => ({
                    id: r.id,
                    col0: r.id,
                    col1: r.name,
                    col2: r.description,
                }))}
                columns={projectColumns}
                rowHref={(id) => `/projects/${id}`}
                editHref={(id) => ({
                    pathname: "/projects",
                    query: { form: "edit-root", rootProjectId: id },
                })}
            />
            <AddRootProject />
            <EditRootProject />
        </>
    );
};

export default ProjectsIndex;
