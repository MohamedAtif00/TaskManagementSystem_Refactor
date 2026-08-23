import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Loader from "../../../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../../../components/curriculum/HierarchyListPage";

type IdName = { id: number; name: string };

const projectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Project", width: 400 },
];

const TaskProjectsPage = () => {
    const router = useRouter();
    const raw = router.query.rootProjectId;
    const rootProjectId = raw ? Number(Array.isArray(raw) ? raw[0] : raw) : NaN;

    const [rootName, setRootName] = useState("");
    const [projects, setProjects] = useState<IdName[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!router.isReady || Number.isNaN(rootProjectId)) return;
        setLoading(true);
        API.PROJECTS.ROOT.LIST().then((rootsRes) => {
            if (
                rootsRes &&
                typeof rootsRes === "object" &&
                "error" in rootsRes &&
                !rootsRes.error &&
                "data" in rootsRes
            ) {
                const r = (rootsRes as { data: IdName[] }).data.find(
                    (x) => x.id === rootProjectId
                );
                if (r) setRootName(r.name);
            }
        });
        API.PROJECTS.ROOT.YEARS(rootProjectId).then((projectsRes) => {
            if (
                projectsRes &&
                typeof projectsRes === "object" &&
                "error" in projectsRes &&
                !projectsRes.error &&
                "data" in projectsRes
            ) {
                setProjects((projectsRes as { data: IdName[] }).data);
            } else setProjects([]);
            setLoading(false);
        });
    }, [router.isReady, rootProjectId, router.asPath]);

    if (!router.isReady || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId)) {
        return <div className="p-6">Invalid year.</div>;
    }

    return (
        <HierarchyListPage
            title="Projects"
            pageTitle={`${rootName} — Projects`}
            icon="task"
            showAdd={false}
            breadcrumbs={[
                { label: "Tasks", href: "/tasks" },
                { label: rootName || `Year #${rootProjectId}` },
            ]}
            rows={projects.map((project) => ({
                id: project.id,
                col0: project.id,
                col1: project.name,
            }))}
            columns={projectColumns}
            rowHref={(projectId) => `/tasks/browse/${rootProjectId}/years/${projectId}`}
        />
    );
};

export default TaskProjectsPage;
