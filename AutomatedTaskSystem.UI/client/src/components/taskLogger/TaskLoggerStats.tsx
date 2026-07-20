interface Props {
    summary: TaskLoggerSummary;
}

const cards = [
    { label: "Total Tasks", key: "totalTasks" as const, border: "border-blue-500", iconColor: "text-blue-500" },
    { label: "Total Time (min)", key: "totalTimeMinutes" as const, border: "border-green-500", iconColor: "text-green-500" },
    { label: "Rollback Tasks", key: "rollbackTasks" as const, border: "border-red-500", iconColor: "text-red-500" },
    { label: "Total Points", key: "totalPoints" as const, border: "border-purple-500", iconColor: "text-purple-500" },
];

const TaskLoggerStats = ({ summary }: Props) => {
    const format = (key: typeof cards[number]["key"], value: number) => {
        if (key === "totalPoints") return value.toFixed(1);
        if (key === "totalTimeMinutes") return Math.round(value).toString();
        return String(value);
    };

    return (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-6">
            {cards.map((card) => (
                <div
                    key={card.label}
                    className={`bg-white rounded-xl shadow p-3 flex justify-between border-b-4 ${card.border}`}
                >
                    <div className={`text-2xl ${card.iconColor}`}>●</div>
                    <div className="text-right">
                        <p className="text-gray-500 text-xs m-0">{card.label}</p>
                        <p className="text-xl font-bold m-0">{format(card.key, summary[card.key] ?? 0)}</p>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default TaskLoggerStats;
