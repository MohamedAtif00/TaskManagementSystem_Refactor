interface Props {
    rows: TaskLoggerRow[];
    onExport: () => Promise<TaskLoggerRow[]>;
}

const TaskLoggerHeader = ({ rows, onExport }: Props) => {
    const handleExport = async () => {
        const exportRows = await onExport();
        if (exportRows.length === 0) return;

        const XLSX = await import("xlsx");
        const sheetRows = exportRows.map((item) => ({
            Date: item.date,
            Member: item.member,
            "LO Code": item.loCode,
            Subject: item.subject,
            Task: item.taskName,
            "Time (min)": item.actualMinutes,
            Expected: item.expectedMinutes,
            Points: item.points,
            Status: item.status,
            Notes: item.notes,
        }));

        const ws = XLSX.utils.json_to_sheet(sheetRows);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Task_Logger");
        XLSX.writeFile(
            wb,
            `Task_Logger_${new Date().toISOString().slice(0, 19).replace(/:/g, "-")}.xlsx`
        );
    };

    return (
        <div className="flex flex-wrap justify-between items-center mb-6 gap-3">
            <div>
                <h2 className="text-2xl font-bold m-0 text-indigo-800">Team Task Logger</h2>
                <p className="text-xs text-slate-400 m-0 mt-1">Member rankings &amp; duplicate LO grouping</p>
            </div>
            <button
                type="button"
                onClick={handleExport}
                disabled={rows.length === 0}
                className="px-4 py-2 rounded-lg bg-green-600 text-white text-sm font-semibold hover:bg-green-700 disabled:opacity-50"
            >
                Export Excel
            </button>
        </div>
    );
};

export default TaskLoggerHeader;
