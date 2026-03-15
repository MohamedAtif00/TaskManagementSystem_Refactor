import { useState, useMemo, useEffect } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { XMarkIcon, ArrowDownTrayIcon, FunnelIcon, TableCellsIcon } from "@heroicons/react/24/outline";
import * as XLSX from "xlsx";

interface TaskInfo {
    id: number;
    name: string;
    status: number; // 0=Backlog, 1=Todo, 2=Doing, 3=Done, 4=Done(alt)
    learningObjective: { id: number; name: string };
    doneAt: string | null;
    startedAt: string | null;
    createdAt: string;
    isRollback: boolean;
    [key: string]: any;
}

type DateFieldKey = "doneAt" | "startedAt" | "createdAt";

interface ExportFilters {
    taskName: string;
    loName: string;
    status: number;
    dateFrom: string;
    dateTo: string;
    dateField: DateFieldKey;
    includeRollback: boolean;
    includeCompleteDone: boolean;
}

interface Props {
    isOpen: boolean;
    onClose: () => void;
    tasks: TaskInfo[];
    projectName: string;
    initialFilters?: Partial<ExportFilters>;
    onApplyFilters?: (filters: ExportFilters) => void;
}

const STATUS_LABELS: Record<number, string> = {
    0: "Backlog",
    1: "To Do",
    2: "Doing",
    3: "Done",
    4: "Rollback",
};

const STATUS_OPTIONS = [
    { value: -1, label: "All Statuses" },
    { value: 0, label: "Backlog" },
    { value: 1, label: "To Do" },
    { value: 2, label: "Doing" },
    { value: 3, label: "Done" },
];

// Column config
const COLUMN_OPTIONS = [
    { key: "loName", label: "LO Name" },
    { key: "taskName", label: "Task Name" },
    { key: "status", label: "Status" },
    { key: "taskType", label: "Task Type" },
    { key: "doneDate", label: "Done Date" },
    { key: "startedDate", label: "Started Date" },
    { key: "createdDate", label: "Created Date" },
];

// Helper to get status label with rollback indication
const getStatusLabel = (task: TaskInfo): string => {
    const baseStatus = STATUS_LABELS[task.status] ?? "";
    return baseStatus;
};

// Helper to get task type
const getTaskType = (task: TaskInfo): string => {
    return task.isRollback ? "Rollback" : "Regular";
};

const formatDate = (dateStr?: string | null) => {
    if (!dateStr) return "";
    const d = new Date(dateStr);
    if (!isNaN(d.getTime())) {
        return d.toLocaleDateString("en-GB");
    }
    return dateStr;
};

const parseLoCode = (loName: string) => {
    if (!loName) return "";
    const match = loName.trim().match(/^([^\s-]+)/);
    return match ? match[1] : loName;
};

export default function ExportTasksModal({
    isOpen,
    onClose,
    tasks,
    projectName,
    initialFilters,
    onApplyFilters,
}: Props) {
    // Filters
    const [taskNameFilter, setTaskNameFilter] = useState("");
    const [loNameFilter, setLoNameFilter] = useState("");
    const [statusFilter, setStatusFilter] = useState(-1);
    const [dateFrom, setDateFrom] = useState("");
    const [dateTo, setDateTo] = useState("");
    const [dateField, setDateField] = useState<DateFieldKey>("doneAt");

    // Done sub-type filters (only relevant when statusFilter === 3)
    const [includeRollback, setIncludeRollback] = useState(true);
    const [includeCompleteDone, setIncludeCompleteDone] = useState(true);

    // Columns
    const [selectedColumns, setSelectedColumns] = useState<Record<string, boolean>>({
        loCode: false,
        loName: true,
        taskName: true,
        status: true,
        taskType: false,
        doneDate: true,
        startedDate: false,
        createdDate: false,
    });

    const toggleColumn = (key: string) => {
        setSelectedColumns(prev => ({ ...prev, [key]: !prev[key] }));
    };

    // When the modal is (re)opened, hydrate filters from parent if provided
    useEffect(() => {
        if (!isOpen || !initialFilters) return;

        if (initialFilters.taskName !== undefined) setTaskNameFilter(initialFilters.taskName);
        if (initialFilters.loName !== undefined) setLoNameFilter(initialFilters.loName);
        if (initialFilters.status !== undefined) setStatusFilter(initialFilters.status);
        if (initialFilters.dateFrom !== undefined) setDateFrom(initialFilters.dateFrom);
        if (initialFilters.dateTo !== undefined) setDateTo(initialFilters.dateTo);
        if (initialFilters.dateField !== undefined) setDateField(initialFilters.dateField);
        if (initialFilters.includeRollback !== undefined) setIncludeRollback(initialFilters.includeRollback);
        if (initialFilters.includeCompleteDone !== undefined) setIncludeCompleteDone(initialFilters.includeCompleteDone);
    }, [isOpen, initialFilters]);

    // isDoneFilter: true when the current status filter targets "Done" tasks
    const isDoneFilter = statusFilter === 3;

    const filteredTasks = useMemo(() => {
        return tasks.filter(task => {
            // Task name filter
            if (taskNameFilter.trim()) {
                const norm = (s: string) => s.toLowerCase().replace(/\s+/g, "");
                if (!norm(task.name).includes(norm(taskNameFilter))) return false;
            }

            // LO name filter
            if (loNameFilter.trim()) {
                const norm = (s: string) => s.toLowerCase().replace(/\s+/g, "");
                if (!norm(task.learningObjective.name).includes(norm(loNameFilter))) return false;
            }

            // Status filter
            if (statusFilter !== -1) {
                if (statusFilter === 3) {
                    // Only allow Done / Rollback statuses
                    if (!(task.status === 3 || task.status === 4)) return false;

                    // Sub-type filter within Done: use status itself, not flags
                    if (!includeCompleteDone && task.status === 3) return false;
                    if (!includeRollback && task.status === 4) return false;
                } else {
                    if (task.status !== statusFilter) return false;
                }
            }

            // Date filter
            if (dateFrom || dateTo) {
                const rawDate = task[dateField];
                if (!rawDate) return false;
                const taskDate = new Date(rawDate);
                if (dateFrom && taskDate < new Date(dateFrom)) return false;
                if (dateTo) {
                    const toEnd = new Date(dateTo);
                    toEnd.setHours(23, 59, 59, 999);
                    if (taskDate > toEnd) return false;
                }
            }

            return true;
        });
    }, [tasks, taskNameFilter, loNameFilter, statusFilter, dateFrom, dateTo, dateField, includeRollback, includeCompleteDone]);

    const handleExport = () => {
        const rows = filteredTasks.map(task => {
            const row: Record<string, string> = {};
            if (selectedColumns.loCode) row["LO Code"] = parseLoCode(task.learningObjective.name);
            if (selectedColumns.loName) row["LO Name"] = task.learningObjective.name;
            if (selectedColumns.taskName) row["Task Name"] = task.name;
            if (selectedColumns.status) row["Status"] = getStatusLabel(task);
            if (selectedColumns.taskType) row["Task Type"] = getTaskType(task);
            if (selectedColumns.doneDate) row["Done Date"] = (task.status === 3 || task.status === 4) ? formatDate(task.doneAt) : "";
            if (selectedColumns.startedDate) row["Started Date"] = formatDate(task.startedAt);
            if (selectedColumns.createdDate) row["Created Date"] = formatDate(task.createdAt);
            return row;
        });

        const ws = XLSX.utils.json_to_sheet(rows);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Tasks");

        // Auto column widths
        const colWidths = Object.keys(rows[0] ?? {}).map(key => ({
            wch: Math.max(key.length, ...rows.map(r => (r[key] ?? "").length)) + 2,
        }));
        ws["!cols"] = colWidths;

        XLSX.writeFile(wb, `${projectName}_tasks_export.xlsx`);
        onClose();
    };

    const handleApplyFilters = () => {
        if (!onApplyFilters) return;

        onApplyFilters({
            taskName: taskNameFilter,
            loName: loNameFilter,
            status: statusFilter,
            dateFrom,
            dateTo,
            dateField,
            includeRollback,
            includeCompleteDone,
        });
        onClose();
    };

    const handleClearFilters = () => {
        setTaskNameFilter("");
        setLoNameFilter("");
        setStatusFilter(-1);
        setDateFrom("");
        setDateTo("");
        setIncludeRollback(true);
        setIncludeCompleteDone(true);
    };

    const activeColumnCount = Object.values(selectedColumns).filter(Boolean).length;

    return (
        <AnimatePresence>
            {isOpen && (
                <motion.div
                    key="export-modal-backdrop"
                    initial={{ backgroundColor: "#00000000" }}
                    animate={{ backgroundColor: "#00000066" }}
                    exit={{ backgroundColor: "#00000000" }}
                    className="fixed inset-0 z-50 flex items-center justify-center p-4"
                    onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}
                >
                    <motion.div
                        initial={{ scale: 0.92, opacity: 0, y: 16 }}
                        animate={{ scale: 1, opacity: 1, y: 0 }}
                        exit={{ scale: 0.92, opacity: 0, y: 16 }}
                        transition={{ type: "spring", stiffness: 300, damping: 28 }}
                        className="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col overflow-hidden"
                    >
                        {/* Header */}
                        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 shrink-0">
                            <div className="flex items-center gap-3">
                                <div className="w-9 h-9 rounded-lg bg-emerald-50 flex items-center justify-center">
                                    <ArrowDownTrayIcon className="w-5 h-5 text-emerald-600" />
                                </div>
                                <div>
                                    <h2 className="text-base font-semibold text-slate-800">Export Tasks</h2>
                                    <p className="text-xs text-slate-400">{projectName}</p>
                                </div>
                            </div>
                            <button
                                onClick={onClose}
                                className="p-1.5 rounded-lg hover:bg-slate-100 transition-colors"
                            >
                                <XMarkIcon className="w-5 h-5 text-slate-500" />
                            </button>
                        </div>

                        <div className="px-6 py-5 space-y-6 overflow-y-auto">
                            {/* Filters Section */}
                            <div>
                                <div className="flex items-center gap-2 mb-3">
                                    <FunnelIcon className="w-4 h-4 text-slate-400" />
                                    <span className="text-sm font-medium text-slate-700">Filters</span>
                                </div>
                                <div className="grid grid-cols-2 gap-3">
                                    {/* Task Name */}
                                    <div>
                                        <label className="block text-xs text-slate-500 mb-1">Task Name</label>
                                        <input
                                            type="text"
                                            placeholder="Search by task name..."
                                            value={taskNameFilter}
                                            onChange={e => setTaskNameFilter(e.target.value)}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 placeholder-slate-300 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition"
                                        />
                                    </div>

                                    {/* LO Name */}
                                    <div>
                                        <label className="block text-xs text-slate-500 mb-1">LO Name</label>
                                        <input
                                            type="text"
                                            placeholder="Search by LO name..."
                                            value={loNameFilter}
                                            onChange={e => setLoNameFilter(e.target.value)}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 placeholder-slate-300 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition"
                                        />
                                    </div>

                                    {/* Status */}
                                    <div className={isDoneFilter ? "" : "col-span-1"}>
                                        <label className="block text-xs text-slate-500 mb-1">Status</label>
                                        <select
                                            value={statusFilter}
                                            onChange={e => {
                                                setStatusFilter(Number(e.target.value));
                                                // Reset sub-type checkboxes when switching status
                                                setIncludeRollback(true);
                                                setIncludeCompleteDone(true);
                                            }}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition bg-white"
                                        >
                                            {STATUS_OPTIONS.map(opt => (
                                                <option key={opt.value} value={opt.value}>{opt.label}</option>
                                            ))}
                                        </select>
                                    </div>

                                    {/* Done sub-type checkboxes — only visible when Done is selected */}
                                    {isDoneFilter && (
                                        <div>
                                            <label className="block text-xs text-slate-500 mb-1">Done Type</label>
                                            <div className="flex flex-col gap-2 px-3 py-2 rounded-lg border border-slate-200 bg-slate-50 h-[38px] justify-center">
                                                <div className="flex items-center gap-4">
                                                    <label className="flex items-center gap-2 cursor-pointer select-none">
                                                        <input
                                                            type="checkbox"
                                                            checked={includeCompleteDone}
                                                            onChange={e => {
                                                                // Prevent unchecking both
                                                                if (!e.target.checked && !includeRollback) return;
                                                                setIncludeCompleteDone(e.target.checked);
                                                            }}
                                                            className="w-3.5 h-3.5 accent-emerald-600 cursor-pointer"
                                                        />
                                                        <span className="text-xs font-medium text-slate-700">Complete</span>
                                                    </label>
                                                    <label className="flex items-center gap-2 cursor-pointer select-none">
                                                        <input
                                                            type="checkbox"
                                                            checked={includeRollback}
                                                            onChange={e => {
                                                                // Prevent unchecking both
                                                                if (!e.target.checked && !includeCompleteDone) return;
                                                                setIncludeRollback(e.target.checked);
                                                            }}
                                                            className="w-3.5 h-3.5 accent-amber-500 cursor-pointer"
                                                        />
                                                        <span className="text-xs font-medium text-slate-700">Rollback</span>
                                                    </label>
                                                </div>
                                            </div>
                                        </div>
                                    )}

                                    {/* Date field selector */}
                                    <div>
                                        <label className="block text-xs text-slate-500 mb-1">Filter by Date Field</label>
                                        <select
                                            value={dateField}
                                            onChange={e => setDateField(e.target.value as DateFieldKey)}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition bg-white"
                                        >
                                            <option value="doneAt">Done Date</option>
                                            <option value="startedAt">Started Date</option>
                                            <option value="createdAt">Created Date</option>
                                        </select>
                                    </div>

                                    {/* Date From */}
                                    <div>
                                        <label className="block text-xs text-slate-500 mb-1">From Date</label>
                                        <input
                                            type="date"
                                            value={dateFrom}
                                            onChange={e => setDateFrom(e.target.value)}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition"
                                        />
                                    </div>

                                    {/* Date To */}
                                    <div>
                                        <label className="block text-xs text-slate-500 mb-1">To Date</label>
                                        <input
                                            type="date"
                                            value={dateTo}
                                            onChange={e => setDateTo(e.target.value)}
                                            className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-sky-200 focus:border-sky-400 transition"
                                        />
                                    </div>
                                </div>
                            </div>

                            {/* Columns Section */}
                            <div>
                                <div className="flex items-center gap-2 mb-3">
                                    <TableCellsIcon className="w-4 h-4 text-slate-400" />
                                    <span className="text-sm font-medium text-slate-700">Columns</span>
                                    <span className="ml-auto text-xs text-slate-400">{activeColumnCount} selected</span>
                                </div>
                                <div className="grid grid-cols-2 gap-2">
                                    {COLUMN_OPTIONS.map(col => (
                                        <label
                                            key={col.key}
                                            className={`flex items-center gap-2.5 px-3 py-2 rounded-lg border cursor-pointer transition-all select-none ${
                                                selectedColumns[col.key]
                                                    ? "border-sky-300 bg-sky-50 text-sky-800"
                                                    : "border-slate-200 bg-white text-slate-500 hover:border-slate-300"
                                            }`}
                                        >
                                            <input
                                                type="checkbox"
                                                checked={!!selectedColumns[col.key]}
                                                onChange={() => toggleColumn(col.key)}
                                                className="w-3.5 h-3.5 accent-sky-600 cursor-pointer"
                                            />
                                            <span className="text-sm font-medium">{col.label}</span>
                                        </label>
                                    ))}
                                </div>
                            </div>

                            {/* Preview count */}
                            <div className="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-slate-50 border border-slate-100">
                                <div className="text-sm text-slate-500">
                                    Exporting{" "}
                                    <span className="font-semibold text-slate-800">{filteredTasks.length}</span>
                                    {" "}of{" "}
                                    <span className="font-semibold text-slate-800">{tasks.length}</span>
                                    {" "}tasks
                                </div>
                                {(taskNameFilter || loNameFilter || statusFilter !== -1 || dateFrom || dateTo) && (
                                    <button
                                        onClick={handleClearFilters}
                                        className="ml-auto text-xs text-slate-400 hover:text-rose-500 transition-colors underline"
                                    >
                                        Clear filters
                                    </button>
                                )}
                            </div>
                        </div>

                        {/* Footer */}
                        <div className="px-6 pb-5 pt-3 flex justify-end gap-2 border-t border-slate-100 shrink-0 bg-white">
                            <button
                                onClick={onClose}
                                className="px-4 py-2 text-sm text-slate-600 rounded-lg border border-slate-200 hover:bg-slate-50 transition"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleApplyFilters}
                                className="px-4 py-2 text-sm font-medium text-sky-700 rounded-lg border border-sky-200 bg-sky-50 hover:bg-sky-100 transition flex items-center gap-2"
                            >
                                <FunnelIcon className="w-4 h-4" />
                                Apply Filters
                            </button>
                            <button
                                onClick={handleExport}
                                disabled={filteredTasks.length === 0 || activeColumnCount === 0}
                                className="px-5 py-2 text-sm font-medium text-white rounded-lg bg-emerald-500 hover:bg-emerald-600 transition disabled:opacity-40 disabled:cursor-not-allowed flex items-center gap-2"
                            >
                                <ArrowDownTrayIcon className="w-4 h-4" />
                                Export Excel
                            </button>
                        </div>
                    </motion.div>
                </motion.div>
            )}
        </AnimatePresence>
    );
}