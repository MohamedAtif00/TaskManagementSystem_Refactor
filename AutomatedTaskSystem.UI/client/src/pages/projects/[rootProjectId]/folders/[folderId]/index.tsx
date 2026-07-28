import { useCallback, useEffect, useMemo, useState } from "react";
import Head from "next/head";
import { useRouter } from "next/router";
import { useAppSelector } from "../../../../../app/hooks";
import API from "../../../../../lib/API";
import { parseYearTerm } from "../../../../../lib/curriculumHierarchy";
import { markProjectTreeSubjectGroup } from "../../../../../lib/projectTreeState";
import Loader from "../../../../../components/loader";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import ProgressDonut from "../../../../../components/charts/ProgressDonut";
import AddProject from "../../../../../components/pageComponent/projects/addProject";
import EditProject from "../../../../../components/pageComponent/projects/editProject";
import HoldProject from "../../../../../components/pageComponent/projects/holdProject";
import CloseProject from "../../../../../components/pageComponent/projects/closeProject";
import ActivateProject from "../../../../../components/pageComponent/projects/activeProject";
import RemoveProject from "../../../../../components/pageComponent/projects/removeProject";
import TableAction from "../../../../../components/TableComponents/TableActionButton";

type FolderDto = {
    id: number;
    name: string;
    path?: string;
};

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

const LeafFolderSubjectsPage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rawFolder = router.query.folderId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const folderId = rawFolder ? Number(Array.isArray(rawFolder) ? rawFolder[0] : rawFolder) : NaN;

    const [loading, setLoading] = useState(true);
    const [folder, setFolder] = useState<FolderDto | null>(null);
    const [subjects, setSubjects] = useState<any[]>([]);
    const [view, setView] = useState<"active" | "hold" | "closed">("active");

    const loadPageData = useCallback(async () => {
        const [folderRes, subjectsRes] = await Promise.all([
            API.PROJECTS.ROOT.GET(folderId),
            API.PROJECTS.GET_BY_FOLDER(folderId, { includeInactive: true }),
        ]);

        if (folderRes && !folderRes.error && folderRes.data) {
            setFolder(folderRes.data as FolderDto);
        } else {
            setFolder(null);
        }

        if (subjectsRes && !subjectsRes.error && Array.isArray(subjectsRes.data)) {
            setSubjects(subjectsRes.data);
        } else {
            setSubjects([]);
        }
    }, [folderId]);

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
            return;
        }
        if (!router.isReady || Number.isNaN(folderId)) return;

        setLoading(true);
        (async () => {
            await loadPageData();
            setLoading(false);
        })();
    }, [auth.isAuth, auth.role, router, router.isReady, router.asPath, folderId, loadPageData]);

    useEffect(() => {
        if (Number.isNaN(rootProjectId) || Number.isNaN(folderId)) return;
        markProjectTreeSubjectGroup(rootProjectId, folderId);
    }, [rootProjectId, folderId]);

    const title = useMemo(() => {
        if (folder?.name) return `TMS - Subjects - ${folder.name}`;
        return "TMS - Subjects";
    }, [folder?.name]);

    const activeColumns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 70 },
        { field: "col1", headerName: "Name", flex: 1, minWidth: 170 },
        { field: "col2", headerName: "Description", flex: 1, minWidth: 220 },
        { field: "col3", headerName: "Year", width: 90 },
        { field: "col4", headerName: "Term", width: 90 },
        { field: "col5", headerName: "No of LO", width: 95 },
        {
            field: "col6",
            headerName: "Progress",
            width: 105,
            renderCell: (params) => (
                <div className="w-full flex items-center justify-center">
                    <ProgressDonut percentage={Number(params.row.progressPercent ?? 0)} size={56} />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
        {
            field: "col7",
            headerName: "Analytics",
            width: 90,
            renderCell: (c) => (
                <div onClick={(e) => e.stopPropagation()}>
                    <TableAction text="Analytics" url={{ pathname: `/subjects/charts/${c.id}` }} type="chart" />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
        {
            field: "colActions",
            headerName: "Actions",
            width: 250,
            renderCell: (c) => (
                <div className="flex justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                    <TableAction text="View" url={{ pathname: `/subjects/${c.id}`, query: { rootProjectId } }} type="eye" />
                    <TableAction
                        text="Edit"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "edit-project", subjectId: c.id },
                        }}
                        type="edit"
                    />
                    <TableAction
                        text="Archive"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "remove-project", subjectId: c.id },
                        }}
                        type="archive"
                    />
                    <TableAction
                        text="Hold"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "hold-project", subjectId: c.id },
                        }}
                        type="pause"
                    />
                    <TableAction
                        text="Close"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "close-project", subjectId: c.id },
                        }}
                        type="stop"
                    />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
    ];

    const disabledColumns: GridColDef[] = [
        { field: "col0", headerName: "ID", width: 70 },
        { field: "col1", headerName: "Name", flex: 1, minWidth: 170 },
        { field: "col2", headerName: "Description", flex: 1, minWidth: 220 },
        { field: "col3", headerName: "Year", width: 90 },
        { field: "col4", headerName: "Term", width: 90 },
        { field: "col5", headerName: "No of LO", width: 95 },
        {
            field: "col6",
            headerName: "Progress",
            width: 105,
            renderCell: (params) => (
                <div className="w-full flex items-center justify-center">
                    <ProgressDonut percentage={Number(params.row.progressPercent ?? 0)} size={56} />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
        {
            field: "col7",
            headerName: "Analytics",
            width: 90,
            renderCell: (c) => (
                <div onClick={(e) => e.stopPropagation()}>
                    <TableAction text="Analytics" url={{ pathname: `/subjects/charts/${c.id}` }} type="chart" />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
        {
            field: "colActions",
            headerName: "Actions",
            width: 220,
            renderCell: (c) => (
                <div className="flex justify-end gap-2" onClick={(e) => e.stopPropagation()}>
                    <TableAction text="View" url={{ pathname: `/subjects/${c.id}`, query: { rootProjectId } }} type="eye" />
                    <TableAction
                        text="Edit"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "edit-project", subjectId: c.id },
                        }}
                        type="edit"
                    />
                    <TableAction
                        text="Archive"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "remove-project", subjectId: c.id },
                        }}
                        type="archive"
                    />
                    <TableAction
                        text="Activate"
                        url={{
                            pathname: router.pathname,
                            query: { ...router.query, form: "activate-project", subjectId: c.id },
                        }}
                        type="resume"
                    />
                </div>
            ),
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
        },
    ];

    const rows = subjects.map((p) => {
        const { year, term } = parseYearTerm(p.folderPath);
        return {
            id: p.id,
            col0: p.id,
            col1: p.name,
            col2: p.description ?? "",
            col3: year,
            col4: term,
            col5: p.count ?? 0,
            col6: "",
            col7: "",
            progressPercent: p.progressPercent ?? 0,
            status: p.status,
        };
    });

    const filteredRows =
        view === "active"
            ? rows.filter((p) => p.status === 0 || p.status === 3)
            : view === "closed"
            ? rows.filter((p) => p.status === 1)
            : rows.filter((p) => p.status === 2);

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4) || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || Number.isNaN(folderId)) {
        return <div className="p-6">Invalid folder URL.</div>;
    }

    return (
        <div className="w-full max-h-screen overflow-y-auto">
            <Head>
                <title>{title}</title>
            </Head>
            <div className="bg-white border border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                <div>
                    <h1 className="font-bold text-3xl">Subjects</h1>
                    <p className="text-slate-500 text-sm mt-1">{folder?.path ?? folder?.name ?? "Folder"}</p>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        type="button"
                        className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                        onClick={() =>
                            router.push({
                                pathname: router.pathname,
                                query: { ...router.query, form: "add-project", folderId },
                            })
                        }
                    >
                        Add Subject
                    </button>
                    <button
                        type="button"
                        className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50"
                        onClick={() => router.push(`/projects/${rootProjectId}`)}
                    >
                        Back to Folders
                    </button>
                </div>
            </div>

            <div className="p-6">
                <div className="bg-white border border-slate-200 rounded-xl">
                    {subjects.length === 0 ? (
                        <div className="p-6 text-slate-500">No subjects in this folder.</div>
                    ) : (
                        <div className="pb-4 mt-4">
                            <div className="flex gap-2 items-end h-9 px-2">
                                <Tab label="Active" active={view === "active"} onClick={() => setView("active")} />
                                <Tab label="On Hold" active={view === "hold"} onClick={() => setView("hold")} />
                                <Tab label="Closed" active={view === "closed"} onClick={() => setView("closed")} />
                            </div>
                            <DataGrid
                                className="bg-white relative h-full"
                                rows={filteredRows}
                                columns={view === "active" ? activeColumns : disabledColumns}
                                initialState={{
                                    sorting: { sortModel: [{ field: "col1", sort: "asc" }] },
                                }}
                                autoHeight
                                sx={{
                                    width: "100%",
                                    minWidth: 0,
                                    "& .MuiDataGrid-main": { width: "100%" },
                                    "& .MuiDataGrid-virtualScroller": { overflowX: "auto" },
                                }}
                            />
                        </div>
                    )}
                </div>
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

export default LeafFolderSubjectsPage;
