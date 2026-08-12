import Head from "next/head";
import { useCallback, useEffect, useMemo, useState } from "react";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import API from "../../lib/API";
import { UserSessionRow } from "../../lib/API/sessions";
import { useAppSelector } from "../../app/hooks";
import { useRouter } from "next/router";
import { toast } from "react-toastify";

const REASON_OPTIONS = [
    { value: "", label: "All reasons" },
    { value: "0", label: "Manual logout" },
    { value: "1", label: "Logged out by system (session expired)" },
    { value: "2", label: "Logged out by system (inactivity)" },
    { value: "3", label: "Logged out by system (admin)" },
    { value: "4", label: "Replaced by new login" },
];

const formatDateTime = (value?: string | null) => {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return value;
    return d.toLocaleString();
};

const toUtcDateString = (iso: string) => {
    const login = new Date(iso);
    const y = login.getUTCFullYear();
    const m = String(login.getUTCMonth() + 1).padStart(2, "0");
    const d = String(login.getUTCDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
};

const isActiveSession = (row: UserSessionRow) => !row.logoutAt;

const SessionsPage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const [rows, setRows] = useState<UserSessionRow[]>([]);
    const [loading, setLoading] = useState(false);
    const [page, setPage] = useState(0);
    const [pageSize, setPageSize] = useState(25);
    const [totalCount, setTotalCount] = useState(0);
    const [from, setFrom] = useState("");
    const [to, setTo] = useState("");
    const [reason, setReason] = useState("");
    const [activeOnly, setActiveOnly] = useState(false);
    const [forcingUserId, setForcingUserId] = useState<number | null>(null);

    useEffect(() => {
        if (auth.isAuth && auth.role !== 4) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    const openUserDay = useCallback(
        (row: UserSessionRow) => {
            router.push({
                pathname: `/sessions/${row.userId}`,
                query: { date: toUtcDateString(row.loginAt) },
            });
        },
        [router]
    );

    const load = useCallback(async () => {
        setLoading(true);
        const res = await API.SESSIONS.GET({
            page: page + 1,
            pageSize: activeOnly ? 100 : pageSize,
            from: from || undefined,
            to: to || undefined,
            reason: reason === "" ? undefined : Number(reason),
        });
        setLoading(false);

        if (res && !res.error && res.data) {
            const items = activeOnly
                ? res.data.items.filter(isActiveSession)
                : res.data.items;
            setRows(items);
            setTotalCount(activeOnly ? items.length : res.data.totalCount);
        } else {
            setRows([]);
            setTotalCount(0);
        }
    }, [page, pageSize, from, to, reason, activeOnly]);

    const forceLogout = useCallback(
        async (userId: number, userName: string) => {
            if (userId === auth.id) {
                toast.warning("You cannot force logout yourself.");
                return;
            }
            if (!window.confirm(`Force logout ${userName}?`)) return;

            setForcingUserId(userId);
            const res = await API.SESSIONS.FORCE_LOGOUT(userId);
            setForcingUserId(null);

            if (res && !res.error) {
                toast.success(res.message || "User force-logged out");
                void load();
            } else {
                toast.error(res && "message" in res ? res.message : "Force logout failed");
            }
        },
        [auth.id, load]
    );

    const columns: GridColDef[] = useMemo(
        () => [
            {
                field: "userName",
                headerName: "User",
                width: 180,
                renderCell: (params) => (
                    <button
                        type="button"
                        className="text-blue-600 hover:underline font-medium text-left"
                        onClick={(e) => {
                            e.stopPropagation();
                            openUserDay(params.row as UserSessionRow);
                        }}
                    >
                        {params.value as string}
                    </button>
                ),
            },
            {
                field: "loginAt",
                headerName: "Login",
                width: 180,
                valueFormatter: (params) => formatDateTime(params.value as string),
            },
            {
                field: "logoutAt",
                headerName: "Logout",
                width: 180,
                valueFormatter: (params) => formatDateTime(params.value as string | null),
            },
            { field: "durationFormatted", headerName: "Duration", width: 120 },
            {
                field: "totalHours",
                headerName: "Total Hours",
                width: 120,
                valueFormatter: (params) => {
                    const value = params.value as number | null | undefined;
                    return value == null ? "—" : value.toFixed(2);
                },
            },
            {
                field: "reason",
                headerName: "Reason",
                width: 240,
                valueFormatter: (params) => (params.value as string) || "Active",
            },
            {
                field: "actions",
                headerName: "Actions",
                width: 140,
                sortable: false,
                filterable: false,
                disableColumnMenu: true,
                renderCell: (params) => {
                    const row = params.row as UserSessionRow;
                    if (!isActiveSession(row)) {
                        return <span className="text-slate-400 text-xs">—</span>;
                    }
                    return (
                        <button
                            type="button"
                            className="px-2 py-1 text-xs rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
                            disabled={forcingUserId === row.userId || row.userId === auth.id}
                            onClick={(e) => {
                                e.stopPropagation();
                                void forceLogout(row.userId, row.userName);
                            }}
                        >
                            {forcingUserId === row.userId ? "..." : "Force Logout"}
                        </button>
                    );
                },
            },
        ],
        [openUserDay, forceLogout, forcingUserId, auth.id]
    );

    useEffect(() => {
        if (auth.isAuth && auth.role === 4) {
            void load();
        }
    }, [auth.isAuth, auth.role, load]);

    if (!auth.isAuth || auth.role !== 4) {
        return null;
    }

    return (
        <>
            <Head>
                <title>TMS - Sessions</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 sticky top-0 left-0 right-0 py-4">
                    <h1 className="font-bold text-2xl mb-4">User Sessions</h1>
                    <div className="flex flex-wrap gap-3 items-end">
                        <label className="flex flex-col text-sm">
                            From
                            <input
                                type="date"
                                className="border border-gray-300 rounded px-2 py-1"
                                value={from}
                                onChange={(e) => {
                                    setPage(0);
                                    setFrom(e.target.value);
                                }}
                            />
                        </label>
                        <label className="flex flex-col text-sm">
                            To
                            <input
                                type="date"
                                className="border border-gray-300 rounded px-2 py-1"
                                value={to}
                                onChange={(e) => {
                                    setPage(0);
                                    setTo(e.target.value);
                                }}
                            />
                        </label>
                        <label className="flex flex-col text-sm">
                            Reason
                            <select
                                className="border border-gray-300 rounded px-2 py-1"
                                value={reason}
                                onChange={(e) => {
                                    setPage(0);
                                    setReason(e.target.value);
                                }}
                            >
                                {REASON_OPTIONS.map((opt) => (
                                    <option key={opt.value} value={opt.value}>
                                        {opt.label}
                                    </option>
                                ))}
                            </select>
                        </label>
                        <label className="flex items-center gap-2 text-sm pb-2 cursor-pointer select-none">
                            <input
                                type="checkbox"
                                checked={activeOnly}
                                onChange={(e) => {
                                    setPage(0);
                                    setActiveOnly(e.target.checked);
                                }}
                            />
                            Active only
                        </label>
                        <button
                            type="button"
                            className="px-4 py-1 rounded bg-blue-600 text-white"
                            onClick={() => void load()}
                        >
                            Refresh
                        </button>
                    </div>
                </div>
                <div className="pb-4 mt-4 bg-white">
                    <DataGrid
                        autoHeight
                        loading={loading}
                        rows={rows}
                        columns={columns}
                        getRowId={(row) => row.id}
                        pagination
                        paginationMode={activeOnly ? "client" : "server"}
                        rowCount={totalCount}
                        page={page}
                        pageSize={pageSize}
                        rowsPerPageOptions={[10, 25, 50, 100]}
                        onPageChange={(p) => setPage(p)}
                        onPageSizeChange={(ps) => {
                            setPage(0);
                            setPageSize(ps);
                        }}
                        onRowClick={(params) => openUserDay(params.row as UserSessionRow)}
                        disableSelectionOnClick
                        sx={{
                            "& .MuiDataGrid-row": { cursor: "pointer" },
                        }}
                    />
                </div>
            </div>
        </>
    );
};

export default SessionsPage;
