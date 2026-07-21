import Loader from "../loader";

interface Props {
    rows: TaskLoggerRow[];
    dupMode: boolean;
    dupOnly: boolean;
    dupLoading: boolean;
    duplicateColorMap: Map<number, string>;
    onDetectDuplicates: () => void;
    onShowDuplicatesOnly: () => void;
    totalLabelCount: number;
}

const TaskLoggerTable = ({
    rows,
    dupMode,
    dupOnly,
    dupLoading,
    duplicateColorMap,
    onDetectDuplicates,
    onShowDuplicatesOnly,
    totalLabelCount,
}: Props) => {
    const statusClass = (status: string) => {
        if (status === "Approved") return "bg-green-100 text-green-700";
        if (status === "Hold") return "bg-yellow-100 text-yellow-700";
        if (status === "Rollback") return "bg-red-100 text-red-700";
        if (status === "Red Flag") return "bg-rose-100 text-rose-700";
        return "bg-gray-100 text-gray-700";
    };

    return (
        <div className="bg-white rounded-xl shadow-md p-4 mb-6 relative">
            {dupLoading && (
                <div className="absolute inset-0 bg-white/70 z-10 flex items-center justify-center rounded-xl">
                    <Loader />
                </div>
            )}
            <div className="flex justify-between items-center mb-3 flex-wrap gap-2">
                <h2 className="text-xl font-bold text-gray-800 m-0">Tasks Log</h2>
                <div className="flex gap-2 flex-wrap items-center">
                    <button
                        type="button"
                        onClick={onDetectDuplicates}
                        disabled={dupLoading}
                        className={`px-4 py-1 rounded-lg shadow text-sm text-white disabled:opacity-60 ${
                            dupMode ? "bg-gray-500" : "bg-amber-500 hover:bg-amber-600"
                        }`}
                    >
                        {dupLoading
                            ? "Loading..."
                            : dupMode
                              ? "Hide Duplicates"
                              : "Detect Duplicate LO Names"}
                    </button>
                    <button
                        type="button"
                        onClick={onShowDuplicatesOnly}
                        disabled={!dupMode || dupLoading}
                        className={`px-4 py-1 rounded-lg shadow text-sm text-white disabled:opacity-40 ${
                            dupOnly ? "bg-green-600" : "bg-indigo-500 hover:bg-indigo-600"
                        }`}
                    >
                        {dupOnly ? "Show All Tasks" : "Show Duplicates Only"}
                    </button>
                    <div className="text-sm font-semibold text-gray-600 bg-gray-100 px-3 py-1 rounded-full">
                        Total: {totalLabelCount}
                    </div>
                </div>
            </div>
            <div className="overflow-x-auto">
                <table className="min-w-full border text-sm">
                    <thead className="bg-gray-100 border-b">
                        <tr>
                            {["Date", "Member", "LO Name", "Subject", "Task", "Time", "Expected", "Points", "Status", "Note"].map(
                                (h) => (
                                    <th key={h} className="p-2 text-left font-semibold whitespace-nowrap">
                                        {h}
                                    </th>
                                )
                            )}
                        </tr>
                    </thead>
                    <tbody>
                        {rows.length === 0 ? (
                            <tr>
                                <td colSpan={10} className="text-center p-4 text-gray-400">
                                    No tasks.
                                </td>
                            </tr>
                        ) : (
                            rows.map((row) => (
                                <tr
                                    key={row.taskId}
                                    className={`border-b hover:brightness-95 ${
                                        dupMode ? duplicateColorMap.get(row.taskId) ?? "" : ""
                                    }`}
                                >
                                    <td className="p-2 whitespace-nowrap">{row.date}</td>
                                    <td className="p-2">{row.member}</td>
                                    <td className="p-2">{row.loCode}</td>
                                    <td className="p-2">{row.subject}</td>
                                    <td className="p-2">{row.taskName}</td>
                                    <td className="p-2 font-bold">{row.actualMinutes}</td>
                                    <td className="p-2">{row.expectedMinutes}</td>
                                    <td className="p-2 font-bold" style={{ color: "#4f46e5" }}>
                                        {Math.round(row.points)}
                                    </td>
                                    <td className="p-2">
                                        <span className={`px-2 py-1 rounded-full text-xs ${statusClass(row.status)}`}>
                                            {row.status}
                                        </span>
                                    </td>
                                    <td className="p-2 max-w-xs truncate">{row.notes || "-"}</td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default TaskLoggerTable;
