import { useEffect, useState } from "react";
import API from "../../lib/API";
import Loader from "../loader";
import Head from "next/head";
import DashboardCard from "./card";
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
import { Bar } from "react-chartjs-2";
import Link from "next/link";

const TeamLeaderDashboard = () => {
    const [dashboard, setDashboard] = useState<TeamLeaderDashboard>();

    useEffect(() => {
        API.DASHBOARDS.GET_TL_DB().then(
            (res) => res && !res.error && setDashboard(res.data)
        );
    }, []);

    if (dashboard === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    ChartJS.register(
        CategoryScale,
        LinearScale,
        BarElement,
        ArcElement,
        Title,
        Tooltip,
        Legend
    );

    return (
        <div className="grid grid-cols-12 gap-8 p-8 w-full">
            <Head>
                <title>ATS - Home</title>
            </Head>
            <DashboardCard label="Members" value={dashboard.members} size={4} />
            <div className="col-span-4 p-4 bg-white rounded flex items-center justify-start gap-1">
                <div className="grow">
                    <div className="text-sm font-bold">Active Projects</div>
                    <div className="text-2xl">{dashboard.projects}</div>
                </div>
                <div className="py-3 bg-slate-400 opacity-50 pl-[1px]"></div>
                <div className="grow flex flex-col items-end">
                    <div className="text-sm font-bold">Projects with Tasks</div>
                    <div className="text-2xl">
                        {dashboard.projectsDetails.length}
                    </div>
                </div>
            </div>
            <DashboardCard
                label="Active Tasks"
                value={dashboard.activeTasks}
                size={4}
            />
            <div className="col-span-full bg-white rounded px-2">
                <Bar
                    data={{
                        labels: dashboard.projectsDetails.map((g) => g.name),
                        datasets: [
                            {
                                data: dashboard.projectsDetails.map(
                                    (g) => g.tasksCount
                                ),
                                backgroundColor: "rgb(53, 162, 235)",
                            },
                        ],
                    }}
                    options={{
                        responsive: true,
                        plugins: {
                            legend: {
                                display: false,
                            },
                            title: {
                                display: true,
                                text: "Tasks Per Project",
                                font: {
                                    size: 20,
                                },
                            },
                        },
                    }}
                />
            </div>
            <div className="col-span-8 bg-white rounded px-2 h-96">
                <Bar
                    data={{
                        labels: dashboard.tasksPerUser.map((g) => g.name),
                        datasets: [
                            {
                                data: dashboard.tasksPerUser.map(
                                    (g) => g.tasksCount
                                ),
                                backgroundColor: "rgb(162, 53, 235)",
                            },
                        ],
                    }}
                    options={{
                        responsive: true,
                        plugins: {
                            legend: {
                                display: false,
                            },
                            title: {
                                display: true,
                                text: "Tasks Per User",
                                font: {
                                    size: 20,
                                },
                            },
                        },
                    }}
                />
            </div>
            <div className="pb-4 col-span-4 bg-white rounded px-2 h-96 flex flex-col items-center gap-2">
                <div className="text-xl font-bold opacity-60 py-1">
                    Projects Navigation
                </div>
                <div className="px-2 rounded-lg border-name border-2 border-solid border-white flex justify-between w-full">
                    <div>Project Name</div>
                    <div>Tasks Number</div>
                </div>
                <div className="grow overflow-y-auto flex flex-col gap-2 w-full">
                    {dashboard.projectsDetails.map((p) => (
                        <Link
                            className="px-2 py-4 rounded-lg border-slate-500 border-2 border-solid flex justify-between group hover:border-blue-500 hover:text-blue-500"
                            key={p.id}
                            href={`/tasks/${p.id}`}
                        >
                            <div className="group-hover:underline">
                                {p.name}
                            </div>
                            <div>{p.tasksCount}</div>
                        </Link>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default TeamLeaderDashboard;
