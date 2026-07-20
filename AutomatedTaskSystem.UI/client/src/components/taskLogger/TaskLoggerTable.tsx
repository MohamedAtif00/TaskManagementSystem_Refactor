import { STATUS_COLORS } from "./constants";

interface Props {
    rows: TaskLoggerRow[];
    dupMode: boolean;
    dupOnly: boolean;
    duplicateColorMap: Map<number, string>;
    onDetectDuplicates: () => void;
    onShowDuplicatesOnly: () => void;
    totalLabelCount: number;
}

const TaskLoggerTable = ({
    rows,
    dupMode,
    dupOnly,
    duplicateColorMap,
    onDetectDuplicates,
    onShowDuplicatesOnly,
    totalLabelCount,
}: Props) => {
    const statusClass = (status: string) => {
        if (status === "Approved") return "bg-green-100 text-green-700";
        if (status === "Hold") return "bg-yellow-100 text-yellow-700";
        if (status === "Rollback") return "bg-red-100 text-red-700";
        return "bg-gray-100 text-gray-700";
    };

    return (
        <div className="bg-white rounded-xl shadow-md p-4 mb-6">
            <div className="flex justify-between items-center mb-3 flex-wrap gap-2">
                <h2 className="text-xl font-bold text-gray-800 m-0">Tasks Log</h2>
                <div className="flex gap-2 flex-wrap items-center">
                    <button
                        type="button"
                        onClick={onDetectDuplicates}
                        className={`px-4 py-1 rounded-lg shadow text-sm text-white ${
                            dupMode ? "bg-gray-500" : "bg-amber-500 hover:bg-amber-600"
                        }`}
                    >
                        {dupMode ? "Hide Duplicates" : "Detect Duplicate LO Codes"}
                    </button>
                    <button
                        type="button"
                        onClick={onShowDuplicatesOnly}
                        disabled={!dupMode}
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
                            {["Date", "Member", "LO Code", "Subject", "Task", "Time", "Expected", "Points", "Status", "Note"].map(
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
                                    <td className="p-2 font-mono text-xs">{row.loCode}</td>
                                    <td className="p-2">{row.subject}</td>
                                    <td className="p-2">{row.taskName}</td>
                                    <td className="p-2 font-bold">{row.actualMinutes}</td>
                                    <td className="p-2">{row.expectedMinutes}</td>
                                    <td className="p-2 font-bold" style={{ color: "#4f46e5" }}>
                                        {Number(row.points).toFixed(1)}
                                    </td>
                                    <td className="p-2">
                                        <span
                                            className={`px-2 py-1 rounded-full text-xs ${statusClass(row.status)}`}
                                            style={
                                                row.status in STATUS_COLORS
                                                    ? undefined
                                                    : undefined
                                            }
                                        >
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
