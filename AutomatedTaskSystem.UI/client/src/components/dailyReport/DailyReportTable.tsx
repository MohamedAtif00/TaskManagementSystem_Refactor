import { STATUS_COLORS } from "./constants";

const PRIORITY_STYLES: Record<string, string> = {
    High: "bg-red-100 text-red-700",
    Medium: "bg-amber-100 text-amber-700",
    Low: "bg-sky-100 text-sky-700",
    None: "bg-slate-100 text-slate-500",
};

interface Props {
    rows: DailyReportRow[];
}

const DailyReportTable = ({ rows }: Props) => {
    if (rows.length === 0) {
        return (
            <div className="bg-white rounded-3xl p-12 text-center text-slate-500 italic shadow-sm">
                No data available for the selected filters.
            </div>
        );
    }

    return (
        <div className="overflow-x-auto bg-white rounded-3xl shadow-sm">
            <table className="w-full border-collapse text-xs min-w-[1500px]">
                <thead>
                    <tr>
                        {[
                            "Date",
                            "Team",
                            "Semester",
                            "Subjects",
                            "Grade",
                            "Task Name",
                            "LO Code",
                            "LO Type",
                            "Assigned To",
                            "Status",
                            "Problem Type",
                            "Priority",
                            "Notes",
                        ].map((h) => (
                            <th
                                key={h}
                                className="bg-slate-100 px-2 py-3 text-center font-bold text-slate-800 border-b-2 border-slate-300"
                            >
                                {h}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {rows.map((row) => (
                        <tr key={row.taskId} className="border-b border-slate-200">
                            <td className="px-2 py-2 text-center">{row.date}</td>
                            <td className="px-2 py-2 text-center">{row.team}</td>
                            <td className="px-2 py-2 text-center">{row.semester}</td>
                            <td className="px-2 py-2 text-center">{row.subjects}</td>
                            <td className="px-2 py-2 text-center">{row.grade}</td>
                            <td className="px-2 py-2 text-center">{row.taskName}</td>
                            <td className="px-2 py-2 text-center">{row.loCode}</td>
                            <td className="px-2 py-2 text-center">{row.loType}</td>
                            <td className="px-2 py-2 text-center">
                                {row.status === "Rollback" ? row.assignedTo || "-" : "-"}
                            </td>
                            <td className="px-2 py-2 text-center">
                                <span
                                    className="inline-block px-2 py-1 rounded-full text-xs font-semibold"
                                    style={{
                                        backgroundColor:
                                            (STATUS_COLORS[row.status as keyof typeof STATUS_COLORS] ?? "#94a3b8") + "33",
                                        color: STATUS_COLORS[row.status as keyof typeof STATUS_COLORS] ?? "#64748b",
                                    }}
                                >
                                    {row.status}
                                </span>
                            </td>
                            <td className="px-2 py-2 text-center">{row.problemType}</td>
                            <td className="px-2 py-2 text-center">
                                <span
                                    className={`inline-block px-2 py-1 rounded-full text-xs font-semibold ${
                                        PRIORITY_STYLES[row.priority] ?? PRIORITY_STYLES.None
                                    }`}
                                >
                                    {row.priority || "None"}
                                </span>
                            </td>
                            <td className="px-2 py-2 text-center min-w-[140px] max-w-xs">
                                {row.notes || "-"}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default DailyReportTable;
