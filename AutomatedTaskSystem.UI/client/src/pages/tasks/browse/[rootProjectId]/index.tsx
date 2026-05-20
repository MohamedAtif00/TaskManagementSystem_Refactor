import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Loader from "../../../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../../../components/curriculum/HierarchyListPage";

type IdName = { id: number; name: string };

const yearColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Year", width: 400 },
];

const TaskYearsPage = () => {
    const router = useRouter();
    const raw = router.query.rootProjectId;
    const rootProjectId = raw ? Number(Array.isArray(raw) ? raw[0] : raw) : NaN;

    const [rootName, setRootName] = useState("");
    const [years, setYears] = useState<IdName[]>([]);
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
        API.PROJECTS.ROOT.YEARS(rootProjectId).then((yearsRes) => {
            if (
                yearsRes &&
                typeof yearsRes === "object" &&
                "error" in yearsRes &&
                !yearsRes.error &&
                "data" in yearsRes
            ) {
                setYears((yearsRes as { data: IdName[] }).data);
            } else setYears([]);
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
        return <div className="p-6">Invalid project.</div>;
    }

    return (
        <HierarchyListPage
            title="Years"
            pageTitle={`${rootName} — Years`}
            icon="task"
            showAdd={false}
            breadcrumbs={[
                { label: "Tasks", href: "/tasks" },
                { label: rootName || `Project #${rootProjectId}` },
            ]}
            rows={years.map((y) => ({
                id: y.id,
                col0: y.id,
                col1: y.name,
            }))}
            columns={yearColumns}
            rowHref={(yearId) => `/tasks/browse/${rootProjectId}/years/${yearId}`}
        />
    );
};

export default TaskYearsPage;
