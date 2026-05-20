import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { useAppSelector } from "../../../../../app/hooks";
import API from "../../../../../lib/API";
import Loader from "../../../../../components/loader";
import { GridColDef } from "@mui/x-data-grid";
import HierarchyListPage from "../../../../../components/curriculum/HierarchyListPage";
import { hierarchyActionsColumn } from "../../../../../components/curriculum/hierarchyColumns";
import { formatDisplayDate } from "../../../../../lib/formatDate";
import AddTerm from "../../../../../components/pageComponent/projects/addTerm";
import EditTerm from "../../../../../components/pageComponent/projects/editTerm";

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
    hierarchyActionsColumn,
];

const YearTermsPage = () => {
    const auth = useAppSelector((s) => s.authSlice);
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
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

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

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

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

    const basePath = `/projects/${rootProjectId}/years/${yearId}`;

    return (
        <>
            <HierarchyListPage
                title="Terms"
                pageTitle={`${yearName} — Terms`}
                backHref={`/projects/${rootProjectId}`}
                breadcrumbs={[
                    { label: "Projects", href: "/projects" },
                    { label: rootName || `Project #${rootProjectId}`, href: `/projects/${rootProjectId}` },
                    { label: yearName || `Year #${yearId}` },
                ]}
                addLabel="Add Term"
                addHref={{
                    pathname: basePath,
                    query: { form: "add-term" },
                }}
                rows={terms.map((t) => ({
                    id: t.id,
                    col0: t.id,
                    col1: t.name,
                    col2: formatDisplayDate(t.startDate),
                    col3: formatDisplayDate(t.endDate),
                }))}
                columns={termColumns}
                rowHref={(termId) => `${basePath}/terms/${termId}`}
                editHref={(id) => ({
                    pathname: basePath,
                    query: { form: "edit-term", termId: id },
                })}
            />
            <AddTerm />
            <EditTerm />
        </>
    );
};

export default YearTermsPage;
