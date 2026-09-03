import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
} from "chart.js";
import { Doughnut } from "react-chartjs-2";

ChartJS.register(ArcElement, Tooltip);

export type DonutSegment = {
    label: string;
    color: string;
    value: number;
};

export const DonutChart = ({
    title,
    segments,
    centerValue,
    centerSubLabel,
}: {
    title: string;
    segments: DonutSegment[];
    centerValue: number | string;
    centerSubLabel: string;
}) => {
    const total = segments.reduce((sum, s) => sum + s.value, 0);
    const hasData = total > 0;

    return (
        <div className="bg-white rounded-xl p-6 flex flex-col border border-gray-100 h-full">
            <h3 className="text-base font-bold text-[#29313D] mb-3">{title}</h3>

            <div className="flex flex-wrap items-center justify-center gap-x-4 gap-y-1 mb-2">
                {segments.map((segment) => (
                    <div key={segment.label} className="flex items-center gap-1.5">
                        <span
                            className="w-2.5 h-2.5 rounded-full shrink-0"
                            style={{ backgroundColor: segment.color }}
                        />
                        <span className="text-xs text-gray-500">{segment.label}</span>
                    </div>
                ))}
            </div>

            <div className="relative flex items-center justify-center h-[200px] flex-1">
                <Doughnut
                    data={{
                        labels: segments.map((s) => s.label),
                        datasets: [
                            {
                                data: hasData
                                    ? segments.map((s) => s.value)
                                    : [1],
                                backgroundColor: hasData
                                    ? segments.map((s) => s.color)
                                    : ["#E5E7EB"],
                                borderWidth: 0,
                            },
                        ],
                    }}
                    options={{
                        cutout: "72%",
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                enabled: hasData,
                                callbacks: {
                                    label: (ctx) => {
                                        const value = ctx.raw as number;
                                        const pct =
                                            total > 0
                                                ? Math.round((value / total) * 100)
                                                : 0;
                                        return `${ctx.label}: ${value} (${pct}%)`;
                                    },
                                },
                            },
                        },
                    }}
                />
                <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div className="text-center">
                        <div className="text-3xl font-bold text-[#29313D] leading-none">
                            {centerValue}
                        </div>
                        <div className="text-[11px] font-semibold uppercase tracking-wider text-[#9CA3AF] mt-1">
                            {centerSubLabel}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export const StatCard = ({
    label,
    value,
    badge,
}: {
    label: string;
    value: number | string;
    badge?: string;
}) => (
    <div className="bg-[#3B82F6] rounded-xl p-5 text-white flex flex-col gap-1">
        <span className="text-xs font-semibold uppercase tracking-wide opacity-90">
            {label}
        </span>
        <div className="flex items-end justify-between gap-2">
            <span className="text-3xl font-bold">{value}</span>
            {badge && (
                <span className="text-xs bg-white text-[#22D3EE] font-bold px-2 py-0.5 rounded-full mb-1">
                    {badge}
                </span>
            )}
        </div>
    </div>
);

export type SubjectOverviewItem = {
    id: number;
    name: string;
    activeTasks: number;
};

export const SubjectsOverviewCard = ({
    subjects,
}: {
    subjects: SubjectOverviewItem[];
}) => (
    <div className="bg-white rounded-xl p-6 border border-gray-100 h-full">
        <div className="flex items-center justify-between mb-4">
            <h3 className="text-base font-bold text-[#29313D]">Subjects Overview</h3>
            <span className="text-xs text-gray-400">Per active tasks</span>
        </div>
        <div className="flex flex-col gap-3">
            {subjects.length === 0 ? (
                <p className="text-sm text-gray-400">No active subjects</p>
            ) : (
                subjects.map((subject) => (
                    <div
                        key={subject.id}
                        className="flex items-center justify-between gap-3"
                    >
                        <span className="text-sm font-medium text-gray-800 truncate">
                            {subject.name}
                        </span>
                        <span className="text-sm font-semibold text-blue-500 shrink-0">
                            {subject.activeTasks}
                        </span>
                    </div>
                ))
            )}
        </div>
    </div>
);

export const DashboardHeader = ({
    title,
    subtitle,
}: {
    title: string;
    subtitle: string;
}) => (
    <div className="mb-8">
        <h1 className="text-2xl font-bold">{title}</h1>
        <p className="text-gray-400 text-sm mt-1">{subtitle}</p>
    </div>
);

export const StatusPill = ({
    status,
}: {
    status: "on_track" | "completed" | "at_risk";
}) => {
    const styles = {
        on_track: "bg-blue-50 text-blue-600",
        completed: "bg-green-50 text-green-600",
        at_risk: "bg-red-50 text-red-600",
    };
    const labels = {
        on_track: "ON TRACK",
        completed: "COMPLETED",
        at_risk: "At Risk",
    };
    return (
        <span className={`text-xs font-semibold px-2 py-0.5 rounded ${styles[status]}`}>
            {labels[status]}
        </span>
    );
};

export const ProgressBar = ({
    percent,
    status,
}: {
    percent: number;
    status: "on_track" | "completed" | "at_risk";
}) => {
    const barColor =
        status === "completed"
            ? "bg-green-500"
            : status === "at_risk"
            ? "bg-red-500"
            : "bg-blue-500";
    return (
        <div className="w-full bg-gray-100 rounded-full h-1.5">
            <div
                className={`h-1.5 rounded-full ${barColor}`}
                style={{ width: `${Math.min(100, percent)}%` }}
            />
        </div>
    );
};
