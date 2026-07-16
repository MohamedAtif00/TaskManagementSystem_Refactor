import { STATUS_COLORS } from "./constants";

interface Props {
    summary: DailyReportSummary | null;
}

const STAT_CARDS = [
    { label: "Total LOs", icon: "📌", borderColor: "#3b82f6" },
    { label: "Approved", icon: "✔️", borderColor: STATUS_COLORS.Approved },
    { label: "Hold", icon: "⏸", borderColor: STATUS_COLORS.Hold },
    { label: "Rollback", icon: "🔄", borderColor: STATUS_COLORS.Rollback },
    { label: "Active Teams", icon: "🏆", borderColor: "#8b5cf6" },
] as const;

const DailyReportStats = ({ summary }: Props) => {
    if (!summary) return null;

    const values = [
        summary.total,
        summary.approved,
        summary.hold,
        summary.rollback,
        summary.activeTeams,
    ];

    return (
        <div className="flex flex-wrap gap-4 mb-6 justify-between">
            {STAT_CARDS.map((card, i) => (
                <div
                    key={card.label}
                    className="flex-[1_1_160px] bg-[#f8fafc] rounded-[20px] py-3 px-5 text-center shadow-[0_1px_2px_rgba(0,0,0,0.05)] border-l-[5px] border-solid"
                    style={{ borderLeftColor: card.borderColor }}
                >
                    <h4 className="m-0 mb-2 text-[0.85rem] font-normal text-[#334155]">
                        {card.icon} {card.label}
                    </h4>
                    <div className="text-[2rem] font-extrabold text-[#0f172a] leading-none">
                        {values[i]}
                    </div>
                </div>
            ))}

            {summary.topTeams.length > 0 && (
                <div className="flex-[2] min-w-[180px] bg-[#f1f5f9] rounded-[20px] py-2 px-3 text-center">
                    <strong className="text-sm text-[#334155]">🏷️ Most Active Teams</strong>
                    <div className="mt-1">
                        {summary.topTeams.map((team) => (
                            <span
                                key={team.team}
                                className="inline-block bg-[#e2e8f0] rounded-[20px] px-3 py-1 text-xs font-semibold m-0.5"
                            >
                                {team.team}: {team.count}
                            </span>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
};

export default DailyReportStats;
