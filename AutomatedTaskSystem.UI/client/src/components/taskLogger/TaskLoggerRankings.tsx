interface Props {
    rankings: TaskLoggerMemberRank[];
    rankingRange: TaskLoggerRankingRange;
    onRangeChange: (range: TaskLoggerRankingRange) => void;
}

const RANK_STYLES = [
    { className: "rank-platinum bg-gradient-to-br from-indigo-100 to-indigo-200 border-l-[6px] border-violet-700 text-violet-950", title: "Platinum Champion", icon: "👑" },
    { className: "rank-gold bg-gradient-to-br from-yellow-100 to-amber-300 border-l-[6px] border-amber-700 text-amber-950", title: "Gold Medalist", icon: "🥇" },
    { className: "rank-silver bg-gradient-to-br from-slate-100 to-slate-300 border-l-[6px] border-slate-600 text-slate-900", title: "Silver Medalist", icon: "🥈" },
    { className: "rank-bronze bg-gradient-to-br from-orange-100 to-orange-400 border-l-[6px] border-orange-900 text-orange-950", title: "Bronze Medalist", icon: "🥉" },
];

const defaultStyle = {
    className: "bg-gradient-to-br from-amber-50 to-yellow-100 border-l-[6px] border-amber-700 text-amber-950 opacity-90",
    title: "Copper Participant",
    icon: "👤",
};

const TaskLoggerRankings = ({ rankings, rankingRange, onRangeChange }: Props) => {
    const ranges: { id: TaskLoggerRankingRange; label: string }[] = [
        { id: "all", label: "All Time" },
        { id: "week", label: "This Week" },
        { id: "month", label: "This Month" },
    ];

    return (
        <div className="bg-white rounded-xl shadow-md p-4 mb-6">
            <div className="flex flex-wrap justify-between items-center mb-3 gap-2">
                <h2 className="text-xl font-bold text-gray-800 m-0">Member Rankings (by Points)</h2>
                <div className="flex gap-2">
                    {ranges.map((r) => (
                        <button
                            key={r.id}
                            type="button"
                            onClick={() => onRangeChange(r.id)}
                            className={`px-3 py-1 rounded-lg text-sm ${
                                rankingRange === r.id
                                    ? "bg-indigo-600 text-white"
                                    : "bg-gray-200 text-gray-700"
                            }`}
                        >
                            {r.label}
                        </button>
                    ))}
                </div>
            </div>
            <div className="flex flex-wrap gap-4 justify-start items-stretch">
                {rankings.length === 0 ? (
                    <div className="text-center text-gray-400 w-full py-4">No data</div>
                ) : (
                    rankings.map((item, idx) => {
                        const style = RANK_STYLES[idx] ?? defaultStyle;
                        return (
                            <div
                                key={item.member}
                                className={`p-4 rounded-xl shadow-sm flex-1 min-w-[180px] ${style.className}`}
                            >
                                <div className="flex items-center gap-3">
                                    <span className="text-2xl">{style.icon}</span>
                                    <div>
                                        <span className="font-bold text-lg">{item.member}</span>
                                        <p className="text-xs opacity-75 m-0">{style.title}</p>
                                    </div>
                                </div>
                                <p className="mt-3 m-0">
                                    Points: <strong>{item.points.toFixed(1)}</strong>
                                </p>
                                <p className="text-sm opacity-75 m-0">
                                    Rank: {item.rank} / {rankings.length}
                                </p>
                            </div>
                        );
                    })
                )}
            </div>
        </div>
    );
};

export default TaskLoggerRankings;
