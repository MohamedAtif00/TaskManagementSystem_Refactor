import { useEffect, useState } from "react";
import Head from "next/head";
import Link from "next/link";
import { formatDistanceToNow } from "date-fns";
import { ExclamationTriangleIcon } from "@heroicons/react/24/solid";
import API from "../../lib/API";
import Loader from "../loader";
import {
    DashboardHeader,
    DonutChart,
    ProgressBar,
    StatCard,
    StatusPill,
    SubjectsOverviewCard,
} from "./dashboardShared";

const WORKLOAD_COLORS = ["#3B82F6", "#F59E0B"];
const TABLE_PREVIEW_COUNT = 4;
const LIST_PREVIEW_COUNT = 3;

const ProjectManagerDashboard = () => {
    const [dashboard, setDashboard] = useState<ProjectManagerDashboard>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        API.DASHBOARDS.GET_PM_DB().then((res) => {
            if (res && !res.error) setDashboard(res.data);
            setLoading(false);
        });
    }, []);

    if (loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full min-h-screen">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    if (!dashboard) {
        return (
            <div className="min-h-screen text-[#29313D] p-6 pl-8">
                <Head>
                    <title>TMS - Dashboard</title>
                </Head>
                <DashboardHeader
                    title="Dashboard"
                    subtitle="Unable to load dashboard data."
                />
            </div>
        );
    }

    const tasksOverview = dashboard.tasksOverview;

    return (
        <div className="min-h-screen text-[#29313D] p-6 pl-8">
            <Head>
                <title>TMS - Dashboard</title>
            </Head>

            <DashboardHeader
                title="Dashboard"
                subtitle="High-level operational stats and system health monitors."
            />

            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-4 mb-6">
                <StatCard label="Projects" value={dashboard.projects} />
                <StatCard label="Sprints" value={dashboard.sprints} />
                <StatCard
                    label="Learning Objectives"
                    value={dashboard.learningObjectives}
                />
                <StatCard label="Users" value={dashboard.users} />
                <StatCard label="Active Tasks" value={dashboard.activeTasks} />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl p-6 border border-gray-100">
                    <div className="flex items-center justify-between mb-4">
                        <h3 className="text-base font-bold text-[#29313D]">
                            Teams Workload
                        </h3>
                        <span className="text-xs text-gray-400">
                            Per Learning Objectives
                        </span>
                    </div>
                    <div className="flex flex-col gap-3">
                        {dashboard.teamsWorkload.length === 0 ? (
                            <p className="text-sm text-gray-400">No teams</p>
                        ) : (
                            dashboard.teamsWorkload.map((team, index) => (
                                <div key={team.id} className="flex items-center gap-3">
                                    <span className="text-sm text-gray-700 w-28 shrink-0 truncate">
                                        {team.name}
                                    </span>
                                    <div className="flex-1 bg-gray-100 rounded-full h-2">
                                        <div
                                            className="h-2 rounded-full"
                                            style={{
                                                width: `${team.workloadPercent}%`,
                                                backgroundColor:
                                                    WORKLOAD_COLORS[
                                                        index % WORKLOAD_COLORS.length
                                                    ],
                                            }}
                                        />
                                    </div>
                                    <span className="text-xs font-semibold text-gray-500 w-8 text-right">
                                        {team.workloadPercent}%
                                    </span>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                <SubjectsOverviewCard subjects={dashboard.subjectsOverview} />

                <DonutChart
                    title="Tasks Overview"
                    segments={[
                        { label: "To Do", color: "#D1D5DB", value: tasksOverview.toDo },
                        { label: "Doing", color: "#3B82F6", value: tasksOverview.doing },
                        { label: "Rollback", color: "#F59E0B", value: tasksOverview.rollback },
                        { label: "Flag", color: "#EF4444", value: tasksOverview.flagged },
                        { label: "Done", color: "#10B981", value: tasksOverview.done },
                    ]}
                    centerValue={tasksOverview.total}
                    centerSubLabel="TOTAL TASKS"
                />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
                <div className="lg:col-span-2 bg-white rounded-xl p-6 border border-gray-100 max-h-[311px] overflow-hidden">
                    <div className="flex items-center justify-between mb-4">
                        <h3 className="text-lg font-bold text-gray-800">
                            Projects Overview
                        </h3>
                        <Link
                            href="/tasks"
                            className="text-sm text-blue-600 hover:underline font-medium"
                        >
                            See All
                        </Link>
                    </div>
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-gray-400 text-xs border-b border-gray-100">
                                    <th className="pb-3 pr-4">Project Name</th>
                                    <th className="pb-3 pr-4">Year</th>
                                    <th className="pb-3 pr-4">Status</th>
                                    <th className="pb-3 pr-4">Progress</th>
                                    <th className="pb-3">Deadline</th>
                                </tr>
                            </thead>
                            <tbody>
                                {dashboard.projectsTable.length === 0 ? (
                                    <tr>
                                        <td
                                            colSpan={5}
                                            className="py-4 text-gray-400 text-center"
                                        >
                                            No projects
                                        </td>
                                    </tr>
                                ) : (
                                    dashboard.projectsTable
                                        .slice(0, TABLE_PREVIEW_COUNT)
                                        .map((project) => (
                                        <tr
                                            key={project.id}
                                            className="border-b border-gray-50 last:border-0"
                                        >
                                            <td className="py-3 pr-4">
                                                <Link
                                                    href={`/tasks/${project.id}`}
                                                    className="font-medium text-gray-800 hover:text-blue-600"
                                                >
                                                    {project.name}
                                                </Link>
                                            </td>
                                            <td className="py-3 pr-4 text-gray-500">
                                                {project.year}
                                            </td>
                                            <td className="py-3 pr-4">
                                                <StatusPill status={project.status} />
                                            </td>
                                            <td className="py-3 pr-4 w-32">
                                                <ProgressBar
                                                    percent={project.progressPercent}
                                                    status={project.status}
                                                />
                                            </td>
                                            <td className="py-3 text-gray-500">
                                                {project.deadline}
                                            </td>
                                        </tr>
                                        ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                <div className="bg-white rounded-xl p-6 border border-gray-100 max-h-[311px] overflow-hidden">
                    <h3 className="text-lg font-bold text-gray-800 mb-4">
                        Tasks Flagged & Rollbacked
                    </h3>
                    <div className="flex flex-col gap-3">
                        {dashboard.flaggedRollbackTasks.length === 0 ? (
                            <p className="text-sm text-gray-400">
                                No flagged or rollback tasks
                            </p>
                        ) : (
                            dashboard.flaggedRollbackTasks
                                .slice(0, LIST_PREVIEW_COUNT)
                                .map((task) => (
                                <Link
                                    key={`${task.taskId}-${task.type}`}
                                    href={`/tasks/${task.projectId}/board?taskId=${task.taskId}`}
                                    className={`flex items-start gap-3 p-3 rounded-lg hover:opacity-80 transition-opacity ${
                                        task.type === "flagged"
                                            ? "bg-red-50"
                                            : "bg-yellow-50"
                                    }`}
                                >
                                    <ExclamationTriangleIcon
                                        className={`w-4 h-4 shrink-0 mt-0.5 ${
                                            task.type === "flagged"
                                                ? "text-red-500"
                                                : "text-orange-500"
                                        }`}
                                    />
                                    <div>
                                        <p className="text-sm font-medium text-gray-800">
                                            {task.userName} — {task.taskName}
                                        </p>
                                        <p className="text-xs text-gray-500 mt-0.5">
                                            {task.type === "flagged"
                                                ? "Flagged"
                                                : "Rollbacked"}{" "}
                                            ·{" "}
                                            {formatDistanceToNow(
                                                new Date(task.timestamp),
                                                { addSuffix: true }
                                            )}
                                        </p>
                                    </div>
                                </Link>
                                ))
                        )}
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
                <div className="lg:col-span-2 bg-white rounded-xl p-6 border border-gray-100 max-h-[311px] overflow-hidden">
                    <div className="flex items-center justify-between mb-4">
                        <h3 className="text-lg font-bold text-gray-800">
                            Sprints Overview
                        </h3>
                        <Link
                            href="/sprints"
                            className="text-sm text-blue-600 hover:underline font-medium"
                        >
                            See All
                        </Link>
                    </div>
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left text-gray-400 text-xs border-b border-gray-100">
                                    <th className="pb-3 pr-4">Sprint Name</th>
                                    <th className="pb-3 pr-4">Project Name</th>
                                    <th className="pb-3 pr-4">Year</th>
                                    <th className="pb-3 pr-4">Status</th>
                                    <th className="pb-3 pr-4">Progress</th>
                                    <th className="pb-3">Deadline</th>
                                </tr>
                            </thead>
                            <tbody>
                                {dashboard.sprintsTable.length === 0 ? (
                                    <tr>
                                        <td
                                            colSpan={6}
                                            className="py-4 text-gray-400 text-center"
                                        >
                                            No sprints
                                        </td>
                                    </tr>
                                ) : (
                                    dashboard.sprintsTable
                                        .slice(0, TABLE_PREVIEW_COUNT)
                                        .map((sprint) => (
                                        <tr
                                            key={sprint.id}
                                            className="border-b border-gray-50 last:border-0"
                                        >
                                            <td className="py-3 pr-4">
                                                <Link
                                                    href={`/sprints/${sprint.id}`}
                                                    className="font-medium text-gray-800 hover:text-blue-600"
                                                >
                                                    {sprint.name}
                                                </Link>
                                            </td>
                                            <td className="py-3 pr-4 text-gray-600">
                                                {sprint.projectName}
                                            </td>
                                            <td className="py-3 pr-4 text-gray-500">
                                                {sprint.year}
                                            </td>
                                            <td className="py-3 pr-4">
                                                <StatusPill status={sprint.status} />
                                            </td>
                                            <td className="py-3 pr-4 w-32">
                                                <ProgressBar
                                                    percent={sprint.progressPercent}
                                                    status={sprint.status}
                                                />
                                            </td>
                                            <td className="py-3 text-gray-500">
                                                {sprint.deadline}
                                            </td>
                                        </tr>
                                        ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                <div className="bg-white rounded-xl p-6 border border-gray-100 max-h-[311px] overflow-hidden">
                    <h3 className="text-lg font-bold text-gray-800 mb-4">Activity Log</h3>
                    <div className="flex flex-col gap-4">
                        {dashboard.activityLog.length === 0 ? (
                            <p className="text-sm text-gray-400">No recent activity</p>
                        ) : (
                            dashboard.activityLog
                                .slice(0, LIST_PREVIEW_COUNT)
                                .map((activity) => (
                                <div
                                    key={activity.id}
                                    className="flex items-start gap-3 border-b border-gray-50 pb-3 last:border-0 last:pb-0"
                                >
                                    <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-xs font-bold shrink-0">
                                        {activity.initials}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm text-gray-700 leading-snug">
                                            {activity.message}
                                        </p>
                                        <p className="text-xs text-gray-400 mt-1">
                                            {formatDistanceToNow(
                                                new Date(activity.createdAt),
                                                { addSuffix: true }
                                            )}
                                        </p>
                                    </div>
                                </div>
                                ))
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ProjectManagerDashboard;
