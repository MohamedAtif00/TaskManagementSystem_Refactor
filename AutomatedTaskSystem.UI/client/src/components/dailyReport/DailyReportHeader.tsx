interface Props {
    rows: DailyReportRow[];
    onExport: () => Promise<DailyReportRow[]>;
}

const DailyReportHeader = ({ rows, onExport }: Props) => {
    const handleExport = async () => {
        const exportRows = await onExport();
        if (exportRows.length === 0) return;

        const XLSX = await import("xlsx");
        const sheetRows = exportRows.map((item) => ({
            Date: item.date,
            Team: item.team,
            Semester: item.semester,
            Subjects: item.subjects,
            Grade: item.grade,
            "Task Name": item.taskName,
            "LO Code": item.loCode,
            "LO Type": item.loType,
            "Assigned To": item.assignedTo,
            Status: item.status,
            "Problem Type": item.problemType,
            Priority: item.priority,
            Notes: item.notes,
        }));

        const ws = XLSX.utils.json_to_sheet(sheetRows);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "LO_Report");
        XLSX.writeFile(
            wb,
            `LO_Report_${new Date().toISOString().slice(0, 19).replace(/:/g, "-")}.xlsx`
        );
    };

    return (
        <div className="flex flex-wrap justify-between items-center mb-6 gap-3">
            <h2 className="text-2xl font-bold m-0 bg-gradient-to-r from-blue-900 to-blue-600 bg-clip-text text-transparent">
                LO Daily Report
            </h2>
            <button
                type="button"
                onClick={handleExport}
                disabled={rows.length === 0}
                className="px-4 py-2 rounded-full bg-white shadow text-sm font-semibold hover:bg-slate-50 disabled:opacity-50"
            >
                Export Excel
            </button>
        </div>
    );
};

export default DailyReportHeader;
