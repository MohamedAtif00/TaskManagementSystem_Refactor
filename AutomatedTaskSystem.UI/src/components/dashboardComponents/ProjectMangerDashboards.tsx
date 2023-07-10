import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
} from "chart.js";
import { useEffect, useState } from "react";
import { Pie, Bar } from "react-chartjs-2";
import API from "../../lib/API";
import Loader from "../loader";
import Head from "next/head";
import DashboardCard from "./card";

const ProjectManagerDashboard = () => {
    const [dashboard, setDashboard] = useState<ProjectManagerDashboard>();

    useEffect(() => {
        API.DASHBOARDS.GET_PM_DB().then(
            (res) => res && !res.error && setDashboard(res.data)
        );
    }, []);

    let done = 0,
        idle = 0,
        running = 0;

    ChartJS.register(
        CategoryScale,
        LinearScale,
        BarElement,
        ArcElement,
        Title,
        Tooltip,
        Legend
    );

    if (dashboard === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    dashboard.projectsReport.forEach((p) => {
        done += p.doneLearningObjectives;
        idle += p.idleLearningObjectives;
        running += p.runningLearningObjectives;
    });

    return (
        <div className="grid grid-cols-12 gap-8 py-8 px-4 w-full">
            <DashboardCard label="Users" value={dashboard.numberOfUsers} />
            <DashboardCard label="Projects" value={dashboard.numberOfProject} />
            <DashboardCard label="Schemas" value={dashboard.numberOfSchemas} />
            <DashboardCard
                label="Active Tasks"
                value={dashboard.numberOfActiveTasks}
            />
            <div className="col-span-4 p-4 bg-white rounded">
                <Pie
                    options={{
                        plugins: {
                            title: {
                                display: true,
                                text: "Learning Objective Statuses",
                                font: {
                                    size: 20,
                                },
                            },
                        },
                    }}
                    data={{
                        labels: ["Idle", "Running", "Done"],
                        datasets: [
                            {
                                label: "Projects",
                                data: [idle, running, done],
                                backgroundColor: [
                                    "#f59e0b",
                                    "#2563eb",
                                    "#22c55e",
                                ],
                                borderWidth: 2,
                            },
                        ],
                    }}
                />
                <div className="text-sm font-bold flex gap-4 justify-center">
                    <div className="flex gap-1">
                        <div className="flex items-center">
                            <div
                                style={{ backgroundColor: "#f59e0b" }}
                                className="p-1 rounded-full"
                            ></div>
                        </div>
                        <div
                            style={{
                                color: "#f59e0b",
                            }}
                        >
                            {idle}
                        </div>
                    </div>
                    <div className="flex gap-1">
                        <div className="flex items-center">
                            <div
                                style={{ backgroundColor: "#2563eb" }}
                                className="p-1 rounded-full"
                            ></div>
                        </div>
                        <div
                            style={{
                                color: "#2563eb",
                            }}
                        >
                            {running}
                        </div>
                    </div>
                    <div className="flex gap-1">
                        <div className="flex items-center">
                            <div
                                style={{ backgroundColor: "#22c55e" }}
                                className="p-1 rounded-full"
                            ></div>
                        </div>
                        <div className="text-[#22c55e]">{done}</div>
                    </div>
                </div>
            </div>
            <div className="bg-white col-span-8">
                <Bar
                    data={{
                        labels: dashboard.groupsCount.map((g) => g.name),
                        datasets: [
                            {
                                data: dashboard.groupsCount.map(
                                    (g) => g.usersCount
                                ),
                                backgroundColor: "rgb(53, 162, 235)",
                            },
                        ],
                    }}
                    height={500}
                    width={1000}
                    options={{
                        responsive: true,
                        plugins: {
                            legend: {
                                display: false,
                            },
                            title: {
                                display: true,
                                text: "User count in Group",
                                font: {
                                    size: 20,
                                },
                            },
                        },
                    }}
                />
            </div>
            <div className="bg-white col-span-12 p-4 overflow-x-auto flex justify-center">
                <div className="w-[1000px] overflow-x-auto">
                    <Bar
                        style={{
                            width: 1000,
                        }}
                        data={{
                            labels: dashboard.projectsReport.map((r) => r.name),
                            datasets: [
                                {
                                    label: "Idle",
                                    data: dashboard.projectsReport.map(
                                        (r) => r.idleLearningObjectives
                                    ),
                                    backgroundColor: "#f59e0b",
                                },
                                {
                                    label: "Running",
                                    data: dashboard.projectsReport.map(
                                        (r) => r.runningLearningObjectives
                                    ),
                                    backgroundColor: "rgb(255, 99, 150)",
                                },
                                {
                                    label: "Done",
                                    data: dashboard.projectsReport.map(
                                        (r) => r.doneLearningObjectives
                                    ),
                                    backgroundColor: "rgb(53, 162, 235)",
                                },
                            ],
                        }}
                        height={500}
                        width={1000}
                        options={{
                            plugins: {
                                title: {
                                    display: true,
                                    text: "Learning Objective statuses in Project",
                                    font: {
                                        size: 20,
                                    },
                                },
                            },
                            interaction: {
                                mode: "index" as const,
                                intersect: false,
                            },
                            scales: {
                                x: {
                                    stacked: true,
                                },
                                y: {
                                    stacked: true,
                                },
                            },
                        }}
                    />
                </div>
            </div>
        </div>
    );
};

export default ProjectManagerDashboard;
