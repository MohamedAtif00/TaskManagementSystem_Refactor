import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend,
    CategoryScale,
    LinearScale,
    BarElement,
} from "chart.js";
import { Doughnut, Bar } from "react-chartjs-2";
import { STATUS_COLORS } from "./constants";

ChartJS.register(ArcElement, Tooltip, Legend, CategoryScale, LinearScale, BarElement);

interface Props {
    charts: DailyReportChartData | null;
}

const DailyReportCharts = ({ charts }: Props) => {
    if (!charts) return null;

    const statusData = {
        labels: ["Approved", "Hold", "Rollback"],
        datasets: [
            {
                data: [charts.approved, charts.hold, charts.rollback],
                backgroundColor: [
                    STATUS_COLORS.Approved,
                    STATUS_COLORS.Hold,
                    STATUS_COLORS.Rollback,
                ],
                borderRadius: 8,
            },
        ],
    };

    const problemTypes = charts.problemTypes ?? [];
    const problemLabels =
        problemTypes.length > 0
            ? problemTypes.map((p) => p.problemType)
            : ["No rollback issues"];
    const problemValues =
        problemTypes.length > 0
            ? problemTypes.map((p) => p.count)
            : [0];

    const problemData = {
        labels: problemLabels,
        datasets: [
            {
                label: "Rollback issues",
                data: problemValues,
                backgroundColor: STATUS_COLORS.Rollback,
                borderRadius: 8,
            },
        ],
    };

    return (
        <div className="flex flex-wrap gap-2 mt-4">
            <div className="flex-1 min-w-fit bg-white/90 rounded-2xl p-2 h-[300px] flex justify-center">
                <Doughnut
                    data={statusData}
                    options={{  
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: { legend: { position: "bottom" } },
                    }}
                />
                <p className="text-center text-xs text-slate-500 mt-1">Status Distribution</p>
            </div>
            <div className="flex-1 min-w-[220px] bg-white/90 rounded-2xl p-2">
                <Bar
                    data={problemData}
                    options={{
                        responsive: true,
                        maintainAspectRatio: true,
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: { stepSize: 1, precision: 0 },
                            },
                        },
                    }}
                />
                <p className="text-center text-xs text-slate-500 mt-1">
                    Rollback Issues by Problem Type
                </p>
            </div>
        </div>
    );
};

export default DailyReportCharts;
