import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../../../../../../app/hooks";
import API from "../../../../../../../lib/API";
import { useRouter } from "next/router";
import ProjectIcon from "../../../../../../../assets/Icons/Project";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import AddProject from "../../../../../../../components/pageComponent/projects/addProject";
import EditProject from "../../../../../../../components/pageComponent/projects/editProject";
import RemoveProject from "../../../../../../../components/pageComponent/projects/removeProject";
import TableAction from "../../../../../../../components/TableComponents/TableActionButton";
import Head from "next/head";
import Loader from "../../../../../../../components/loader";
import { load } from "../../../../../../../slices/projectSlice";
import HoldProject from "../../../../../../../components/pageComponent/projects/holdProject";
import CloseProject from "../../../../../../../components/pageComponent/projects/closeProject";
import ActivateProject from "../../../../../../../components/pageComponent/projects/activeProject";
import CurriculumBreadcrumb from "../../../../../../../components/curriculum/CurriculumBreadcrumb";
import EditTerm from "../../../../../../../components/pageComponent/projects/editTerm";
import { formatDisplayDate } from "../../../../../../../lib/formatDate";
import RightArrowIcon from "../../../../../../../assets/Icons/RightArrow";

type IdName = { id: number; name: string };

const subjectColumns: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Name", width: 220 },
    { field: "col2", headerName: "Description", width: 360 },
    {
        field: "col3",
        headerName: "Actions",
        width: 260,
        renderCell: (c) => (
            <div className="flex justify-end gap-4" onClick={(e) => e.stopPropagation()}>
                <TableAction text="View" url={{ pathname: `/subjects/${c.id}` }} type="eye" />
                <TableAction
                    text="Edit"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "edit-project", subjectId: c.id },
                    }}
                    type="edit"
                />
                <TableAction
                    text="Archive"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "remove-project", subjectId: c.id },
                    }}
                    type="archive"
                />
                <TableAction
                    text="Hold"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "hold-project", subjectId: c.id },
                    }}
                    type="pause"
                />
                <TableAction
                    text="Close"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "close-project", subjectId: c.id },
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

const columnsForDisabled: GridColDef[] = [
    { field: "col0", headerName: "ID", width: 90 },
    { field: "col1", headerName: "Name", width: 220 },
    { field: "col2", headerName: "Description", width: 360 },
    {
        field: "col3",
        headerName: "Actions",
        width: 220,
        renderCell: (c) => (
            <div className="flex justify-end gap-4" onClick={(e) => e.stopPropagation()}>
                <TableAction text="View" url={{ pathname: `/subjects/${c.id}` }} type="eye" />
                <TableAction
                    text="Edit"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "edit-project", subjectId: c.id },
                    }}
                    type="edit"
                />
                <TableAction
                    text="Archive"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "remove-project", subjectId: c.id },
                    }}
                    type="archive"
                />
                <TableAction
                    text="Activate"
                    url={{
                        pathname: c.row.basePath,
                        query: { form: "activate-project", subjectId: c.id },
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

const TermSubjectsPage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const dispatch = useAppDispatch();
    const [view, setView] = useState<"active" | "hold" | "closed">("active");
    const [subjects, setSubjects] = useState<IProject[]>();
    const [rootName, setRootName] = useState("");
    const [yearName, setYearName] = useState("");
    const [termName, setTermName] = useState("");
    const [termStartDate, setTermStartDate] = useState<string | null>(null);
    const [termEndDate, setTermEndDate] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    const rawRoot = router.query.rootProjectId;
    const rawYear = router.query.yearId;
    const rawTerm = router.query.termId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const yearId = rawYear ? Number(Array.isArray(rawYear) ? rawYear[0] : rawYear) : NaN;
    const termId = rawTerm ? Number(Array.isArray(rawTerm) ? rawTerm[0] : rawTerm) : NaN;

    const basePath =
        !Number.isNaN(rootProjectId) && !Number.isNaN(yearId) && !Number.isNaN(termId)
            ? `/projects/${rootProjectId}/years/${yearId}/terms/${termId}`
            : "/projects";

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    useEffect(() => {
        if (!router.isReady || Number.isNaN(termId)) return;
        setLoading(true);
        (async () => {
            const rootsRes = await API.PROJECTS.ROOT.LIST();
            if (rootsRes && typeof rootsRes === "object" && "error" in rootsRes && !rootsRes.error && "data" in rootsRes) {
                const r = (rootsRes as { data: IdName[] }).data.find((x) => x.id === rootProjectId);
                if (r) setRootName(r.name);
            }
            const yearsRes = await API.PROJECTS.ROOT.YEARS(rootProjectId);
            if (yearsRes && typeof yearsRes === "object" && "error" in yearsRes && !yearsRes.error && "data" in yearsRes) {
                const y = (yearsRes as { data: IdName[] }).data.find((x) => x.id === yearId);
                if (y) setYearName(y.name);
            }
            const termRes = await API.PROJECTS.ROOT.GET_TERM(termId);
            if (termRes && typeof termRes === "object" && "error" in termRes && !termRes.error && "data" in termRes) {
                const t = (termRes as {
                    data: { name: string; startDate?: string | null; endDate?: string | null };
                }).data;
                setTermName(t.name);
                setTermStartDate(t.startDate ?? null);
                setTermEndDate(t.endDate ?? null);
            }
            const all = await API.PROJECTS.GET_BY_TERM(termId);
            if (all && !all.error) {
                setSubjects(all.data);
                dispatch(load(all.data));
            }
            setLoading(false);
        })();
    }, [router.isReady, rootProjectId, yearId, termId, router.asPath, dispatch]);

    const mapRows = (list: IProject[]) =>
        list.map((p) => ({
            id: p.id,
            col0: p.id,
            col1: p.name,
            col2: p.description,
            basePath,
        }));

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (!router.isReady || loading || subjects === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    const filtered =
        view === "active"
            ? subjects.filter((p) => p.status === 0 || p.status === 3)
            : view === "closed"
            ? subjects.filter((p) => p.status === 1)
            : subjects.filter((p) => p.status === 2);

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <Head>
                <title>ATS - Subjects — {termName}</title>
            </Head>
            <div className="pt-4 px-2">
                <CurriculumBreadcrumb
                    items={[
                        { label: "Projects", href: "/projects" },
                        { label: rootName, href: `/projects/${rootProjectId}` },
                        {
                            label: yearName,
                            href: `/projects/${rootProjectId}/years/${yearId}`,
                        },
                        { label: termName },
                    ]}
                />
            </div>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 min-h-20 sticky top-0 left-0 right-0 flex items-center justify-between gap-4 py-3">
                <div className="flex gap-3 items-center min-w-0">
                    <Link
                        href={`/projects/${rootProjectId}/years/${yearId}`}
                        className="shrink-0 p-1 rounded hover:bg-slate-100"
                        aria-label="Go back"
                    >
                        <RightArrowIcon className="w-6 h-6 stroke-black" />
                    </Link>
                    <div className="w-6 h-6 shrink-0">
                        <ProjectIcon />
                    </div>
                    <div className="min-w-0">
                        <h1 className="font-bold text-2xl truncate">Subjects</h1>
                        <p className="text-sm text-slate-600 truncate">
                            {termName}
                            {(termStartDate || termEndDate) && (
                                <>
                                    {" "}
                                    · {formatDisplayDate(termStartDate)} – {formatDisplayDate(termEndDate)}
                                </>
                            )}
                        </p>
                    </div>
                </div>
                <div className="flex gap-2 shrink-0">
                    <Link
                        href={{
                            pathname: basePath,
                            query: { form: "edit-term", termId: String(termId) },
                        }}
                    >
                        <button type="button" className="px-4 py-1 rounded border border-solid border-gray-400">
                            Edit Term
                        </button>
                    </Link>
                    <Link
                        href={{
                            pathname: basePath,
                            query: { form: "add-project", termId: String(termId) },
                        }}
                    >
                        <button type="button" className="px-4 py-1 rounded bg-blue-600 text-white">
                            Add Subject
                        </button>
                    </Link>
                </div>
            </div>
            <div className="pb-4 mt-4">
                <div className="flex gap-2 items-end h-9">
                    <Tab label="Active" active={view === "active"} onClick={() => setView("active")} />
                    <Tab label="On Hold" active={view === "hold"} onClick={() => setView("hold")} />
                    <Tab label="Closed" active={view === "closed"} onClick={() => setView("closed")} />
                </div>
                <DataGrid
                    className="bg-white relative h-full"
                    rows={mapRows(filtered)}
                    columns={view === "active" ? subjectColumns : columnsForDisabled}
                    initialState={{
                        sorting: { sortModel: [{ field: "col1", sort: "asc" }] },
                    }}
                />
            </div>
            <AddProject />
            <EditProject />
            <HoldProject />
            <ActivateProject />
            <CloseProject />
            <RemoveProject />
            <EditTerm />
        </div>
    );
};

export default TermSubjectsPage;
