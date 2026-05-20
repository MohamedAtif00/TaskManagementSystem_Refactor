import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../../../lib/API";
import Loader from "../../../../../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../../../../../components/curriculum/HierarchyListPage";
import { formatDisplayDate } from "../../../../../../lib/formatDate";

type IdName = { id: number; name: string };
type TermRow = {
    id: number;
    name: string;
    startDate?: string | null;
    endDate?: string | null;
};

const termColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 70 },
    { field: "col1", headerName: "Term", width: 160 },
    { field: "col2", headerName: "Start", width: 120 },
    { field: "col3", headerName: "End", width: 120 },
];

const TaskTermsPage = () => {
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rawYear = router.query.yearId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const yearId = rawYear ? Number(Array.isArray(rawYear) ? rawYear[0] : rawYear) : NaN;

    const [rootName, setRootName] = useState("");
    const [yearName, setYearName] = useState("");
    const [terms, setTerms] = useState<TermRow[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!router.isReady || Number.isNaN(rootProjectId) || Number.isNaN(yearId)) return;
        setLoading(true);
        (async () => {
            const rootsRes = await API.PROJECTS.ROOT.LIST();
            if (
                rootsRes &&
                typeof rootsRes === "object" &&
                "error" in rootsRes &&
                !rootsRes.error &&
                "data" in rootsRes
            ) {
                const r = (rootsRes as { data: IdName[] }).data.find((x) => x.id === rootProjectId);
                if (r) setRootName(r.name);
            }
            const yearsRes = await API.PROJECTS.ROOT.YEARS(rootProjectId);
            if (
                yearsRes &&
                typeof yearsRes === "object" &&
                "error" in yearsRes &&
                !yearsRes.error &&
                "data" in yearsRes
            ) {
                const y = (yearsRes as { data: IdName[] }).data.find((x) => x.id === yearId);
                if (y) setYearName(y.name);
            }
            const termsRes = await API.PROJECTS.ROOT.TERMS(yearId);
            if (
                termsRes &&
                typeof termsRes === "object" &&
                "error" in termsRes &&
                !termsRes.error &&
                "data" in termsRes
            ) {
                setTerms((termsRes as { data: TermRow[] }).data);
            } else setTerms([]);
            setLoading(false);
        })();
    }, [router.isReady, rootProjectId, yearId, router.asPath]);

    if (!router.isReady || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || Number.isNaN(yearId)) {
        return <div className="p-6">Invalid URL.</div>;
    }

    const browseBase = `/tasks/browse/${rootProjectId}`;

    return (
        <HierarchyListPage
            title="Terms"
            pageTitle={`${yearName} — Terms`}
            icon="task"
            showAdd={false}
            breadcrumbs={[
                { label: "Tasks", href: "/tasks" },
                { label: rootName || `Project #${rootProjectId}`, href: browseBase },
                { label: yearName || `Year #${yearId}` },
            ]}
            rows={terms.map((t) => ({
                id: t.id,
                col0: t.id,
                col1: t.name,
                col2: formatDisplayDate(t.startDate),
                col3: formatDisplayDate(t.endDate),
            }))}
            columns={termColumns}
            rowHref={(termId) =>
                `/tasks/browse/${rootProjectId}/years/${yearId}/terms/${termId}`
            }
        />
    );
};

export default TaskTermsPage;
