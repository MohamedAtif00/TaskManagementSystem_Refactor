import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import Head from "next/head";
import API from "../../../../../../../../lib/API";
import Loader from "../../../../../../../../components/loader";
import TaskIcon from "../../../../../../../../assets/Icons/Task";
import CurriculumBreadcrumb from "../../../../../../../../components/curriculum/CurriculumBreadcrumb";
import TaskSubjectsDataGrid from "../../../../../../../../components/tasks/TaskSubjectsDataGrid";
import HierarchyListPage from "../../../../../../../../components/curriculum/HierarchyListPage";
import { GridColDef } from "@mui/x-data-grid";

type IdName = { id: number; name: string };
type FolderDto = { id: number; name: string; parentFolderId?: number | null; path?: string };

const folderColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Folder", flex: 1, minWidth: 260 },
    { field: "col2", headerName: "Items", width: 120 },
];

const TaskTermSubjectsPage = () => {
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rawYear = router.query.yearId;
    const rawTerm = router.query.termId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const yearId = rawYear ? Number(Array.isArray(rawYear) ? rawYear[0] : rawYear) : NaN;
    const folderId = rawTerm ? Number(Array.isArray(rawTerm) ? rawTerm[0] : rawTerm) : NaN;

    const [rootName, setRootName] = useState("");
    const [yearName, setYearName] = useState("");
    const [folder, setFolder] = useState<FolderDto | null>(null);
    const [children, setChildren] = useState<FolderDto[]>([]);
    const [subjects, setSubjects] = useState<IProject[]>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!router.isReady || Number.isNaN(folderId)) return;
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
            const folderRes = await API.PROJECTS.ROOT.GET_TERM(folderId);
            if (
                folderRes &&
                typeof folderRes === "object" &&
                "error" in folderRes &&
                !folderRes.error &&
                "data" in folderRes
            ) {
                setFolder((folderRes as { data: FolderDto }).data);
            } else {
                setFolder(null);
            }

            const childrenRes = await API.PROJECTS.ROOT.TERMS(folderId);
            let nextChildren: FolderDto[] = [];
            if (
                childrenRes &&
                typeof childrenRes === "object" &&
                "error" in childrenRes &&
                !childrenRes.error &&
                "data" in childrenRes &&
                Array.isArray(childrenRes.data)
            ) {
                nextChildren = (childrenRes as { data: FolderDto[] }).data;
                setChildren(nextChildren);
            } else {
                setChildren([]);
            }

            if (nextChildren.length === 0) {
                const res = await API.PROJECTS.GET_BY_FOLDER(folderId);
                setSubjects(res && !res.error ? res.data : []);
            } else {
                setSubjects(undefined);
            }
            setLoading(false);
        })();
    }, [router.isReady, rootProjectId, yearId, folderId, router.asPath]);

    if (!router.isReady || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || Number.isNaN(yearId) || Number.isNaN(folderId)) {
        return <div className="p-6">Invalid URL.</div>;
    }

    const browseBase = `/tasks/browse/${rootProjectId}`;
    const yearHref = `${browseBase}/years/${yearId}`;
    const currentName = folder?.name ?? `Folder #${folderId}`;

    if (children.length > 0) {
        return (
            <HierarchyListPage
                title={currentName}
                pageTitle={`Tasks - ${currentName}`}
                icon="task"
                showAdd={false}
                breadcrumbs={[
                    { label: "Tasks", href: "/tasks" },
                    { label: rootName || `Project #${rootProjectId}`, href: browseBase },
                    { label: yearName || `Year #${yearId}`, href: yearHref },
                    { label: currentName },
                ]}
                rows={children.map((child) => ({
                    id: child.id,
                    col0: child.id,
                    col1: child.name,
                    col2: "",
                }))}
                columns={folderColumns}
                rowHref={(childId) =>
                    `/tasks/browse/${rootProjectId}/years/${yearId}/terms/${childId}`
                }
            />
        );
    }

    return (
        <div className="mx-auto relative max-h-screen w-full min-w-0 overflow-y-auto overflow-x-hidden px-2 sm:px-4 box-border">
            <Head>
                <title>TMS - Subjects — {currentName}</title>
            </Head>
            <div className="pt-4 px-2">
                <CurriculumBreadcrumb
                    items={[
                        { label: "Tasks", href: "/tasks" },
                        { label: rootName, href: browseBase },
                        { label: yearName, href: yearHref },
                        { label: currentName },
                    ]}
                />
            </div>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 min-h-20 sticky top-0 left-0 right-0 flex items-center justify-between gap-4 py-3">
                <div className="flex gap-2 items-center min-w-0">
                    <TaskIcon className="stroke-black shrink-0" />
                    <div className="min-w-0">
                        <h1 className="font-bold text-2xl truncate">Subjects</h1>
                        <p className="text-sm text-slate-600 truncate">{folder?.path ?? currentName}</p>
                    </div>
                </div>
            </div>
            <TaskSubjectsDataGrid subjects={subjects ?? []} />
        </div>
    );
};

export default TaskTermSubjectsPage;
