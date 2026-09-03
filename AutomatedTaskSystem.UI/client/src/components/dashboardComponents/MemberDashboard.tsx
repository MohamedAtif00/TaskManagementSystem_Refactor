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
    StatCard,
    SubjectsOverviewCard,
} from "./dashboardShared";

const PRIORITY_LABELS: Record<number, { label: string; className: string }> = {
    1: { label: "High", className: "text-red-600 bg-red-50" },
    2: { label: "Medium", className: "text-blue-600 bg-blue-50" },
    3: { label: "Low", className: "text-orange-500 bg-orange-50" },
};

const TaskBoardCard = ({ task }: { task: MemberTaskBoardItem }) => {
    const priority = PRIORITY_LABELS[task.priority];
    return (
        <Link
            href={`/tasks/${task.projectId}/board?taskId=${task.id}`}
            className="block bg-gray-50 rounded-lg p-3 border border-gray-100 hover:border-blue-300 transition-colors"
        >
            <div className="text-xs text-gray-500 mb-1">{task.projectName}</div>
            <div
                className={`text-sm font-medium text-gray-800 mb-2 ${
                    task.isCompleted ? "line-through text-gray-500" : ""
                }`}
            >
                {task.name}
            </div>
            <div className="flex items-center justify-between">
                {priority ? (
                    <span className={`text-xs font-semibold px-2 py-0.5 rounded ${priority.className}`}>
                        {priority.label}
                    </span>
                ) : (
                    <span />
                )}
                {task.dueDate && (
                    <span
                        className={`text-xs font-medium px-2 py-0.5 rounded ${
                            task.isDueToday
                                ? "text-red-600 bg-red-50"
                                : task.isCompleted
                                ? "text-teal-600 bg-teal-50"
                                : "text-blue-600 bg-blue-50"
                        }`}
                    >
                        {task.isCompleted ? "Completed" : task.isDueToday ? "Today" : task.dueDate}
                    </span>
                )}
            </div>
        </Link>
    );
};

const MemberDashboard = () => {
    const [dashboard, setDashboard] = useState<MemberDashboard>();

    useEffect(() => {
        API.DASHBOARDS.GET_MEMBER_DB().then(
            (res) => res && !res.error && setDashboard(res.data)
        );
    }, []);

    if (dashboard === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full min-h-screen ">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    const tasksOverview = dashboard.tasksOverview;

    return (
        <div className="min-h-screen  text-[#29313D] p-6 pl-8">
            <Head>
                <title>TMS - Dashboard</title>
            </Head>

            <DashboardHeader
                title="Dashboard"
                subtitle="Manage your personal assignments and deadlines."
            />

            {/* Stat Cards */}
            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-4 mb-6">
                <StatCard
                    label="Projects"
                    value={dashboard.projects}
                    badge={`+${dashboard.activeProjects} Active`}
                />
                <StatCard label="Sprints" value={dashboard.sprints} />
                <StatCard
                    label="Learning Objectives"
                    value={dashboard.learningObjectives}
                    badge={`+${dashboard.learningObjectivesThisMonth} this month`}
                />
                <StatCard
                    label="Team Performance"
                    value={`${dashboard.teamPerformance}%`}
                />
                <StatCard label="Active Tasks" value={dashboard.activeTasks} />
            </div>

            {/* Charts Row */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                <SubjectsOverviewCard subjects={dashboard.subjectsOverview} />
                <DonutChart
                    title="Your Tasks Overview"
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

            {/* My Task Board */}
            <div className="bg-white rounded-xl p-6 mb-6">
                <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-bold text-gray-800">My Task Board</h3>
                    <Link href="/tasks" className="text-sm text-blue-600 hover:underline font-medium">
                        See All
                    </Link>
                </div>
                <div className="grid grid-cols-3 gap-4">
                    <div>
                        <div className="text-sm font-semibold text-gray-500 mb-3 px-1">To Do</div>
                        <div className="flex flex-col gap-3">
                            {dashboard.toDoTasks.length === 0 ? (
                                <p className="text-sm text-gray-400 px-1">No tasks</p>
                            ) : (
                                dashboard.toDoTasks.slice(0, 2).map((t) => <TaskBoardCard key={t.id} task={t} />)
                            )}
                        </div>
                    </div>
                    <div>
                        <div className="text-sm font-semibold text-gray-500 mb-3 px-1">In Progress</div>
                        <div className="flex flex-col gap-3">
                            {dashboard.inProgressTasks.length === 0 ? (
                                <p className="text-sm text-gray-400 px-1">No tasks</p>
                            ) : (
                                dashboard.inProgressTasks.slice(0, 2).map((t) => <TaskBoardCard key={t.id} task={t} />)
                            )}
                        </div>
                    </div>
                    <div>
                        <div className="text-sm font-semibold text-gray-500 mb-3 px-1">Done</div>
                        <div className="flex flex-col gap-3">
                            {dashboard.doneTasks.length === 0 ? (
                                <p className="text-sm text-gray-400 px-1">No tasks</p>
                            ) : (
                                dashboard.doneTasks.slice(0, 2).map((t) => <TaskBoardCard key={t.id} task={t} />)
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Bottom Row */}
            <div className="grid grid-cols-3 gap-4">
                {/* Sprint Deadlines */}
                <div className="bg-white rounded-xl p-6">
                    <h3 className="text-lg font-bold text-gray-800 mb-4">Sprint Deadlines</h3>
                    <div className="flex flex-col gap-4">
                        {dashboard.sprintDeadlines.length === 0 ? (
                            <p className="text-sm text-gray-400">No active sprints</p>
                        ) : (
                            dashboard.sprintDeadlines.map((s) => {
                                const barColor =
                                    s.urgency === "critical"
                                        ? "bg-red-500"
                                        : s.urgency === "warning"
                                        ? "bg-orange-400"
                                        : "bg-blue-500";
                                const badgeColor =
                                    s.urgency === "critical"
                                        ? "text-red-600 bg-red-50"
                                        : s.urgency === "warning"
                                        ? "text-orange-600 bg-orange-50"
                                        : "text-blue-600 bg-blue-50";
                                return (
                                    <div key={s.sprintId} className="border-b border-gray-100 pb-4 last:border-0 last:pb-0">
                                        <div className="flex items-center justify-between mb-1">
                                            <span className="text-sm font-semibold text-gray-800">
                                                {s.sprintName}
                                            </span>
                                            <span className={`text-xs font-medium px-2 py-0.5 rounded ${badgeColor}`}>
                                                {s.daysLeft} days left
                                            </span>
                                        </div>
                                        <p className="text-xs text-gray-500 mb-2">{s.taskName}</p>
                                        <div className="w-full bg-gray-100 rounded-full h-2 mb-1">
                                            <div
                                                className={`h-2 rounded-full ${barColor}`}
                                                style={{ width: `${s.progressPercent}%` }}
                                            />
                                        </div>
                                        <p className="text-xs text-gray-400">{s.deadlineDate}</p>
                                    </div>
                                );
                            })
                        )}
                    </div>
                </div>

                {/* Escalated Tasks */}
                <div className="bg-white rounded-xl p-6">
                    <h3 className="text-lg font-bold text-gray-800 mb-4">Tasks Escalated to High</h3>
                    <div className="flex flex-col gap-3">
                        {dashboard.escalatedTasks.length === 0 ? (
                            <p className="text-sm text-gray-400">No escalated tasks</p>
                        ) : (
                            dashboard.escalatedTasks.map((t, i) => (
                                <Link
                                    key={`${t.taskId}-${i}`}
                                    href={`/tasks/${t.projectId}/board?taskId=${t.taskId}`}
                                    className={`flex items-start gap-3 p-3 rounded-lg ${
                                        i % 2 === 0 ? "bg-red-50" : "bg-yellow-50"
                                    } hover:opacity-80 transition-opacity`}
                                >
                                    <ExclamationTriangleIcon className="w-4 h-4 text-red-500 shrink-0 mt-0.5" />
                                    <div>
                                        <p className="text-sm font-medium text-gray-800">
                                            {t.taskName}
                                        </p>
                                        <p className="text-xs text-gray-500 mt-0.5">
                                            Escalated by {t.escalatedBy} · {t.escalatedAt}
                                        </p>
                                    </div>
                                </Link>
                            ))
                        )}
                    </div>
                </div>

                {/* Work Updates */}
                <div className="bg-white rounded-xl p-6">
                    <h3 className="text-lg font-bold text-gray-800 mb-4">Work Updates</h3>
                    <div className="flex flex-col gap-4">
                        {dashboard.workUpdates.length === 0 ? (
                            <p className="text-sm text-gray-400">No recent updates</p>
                        ) : (
                            dashboard.workUpdates.map((u) => (
                                <div key={u.id} className="border-b border-gray-100 pb-3 last:border-0 last:pb-0">
                                    <div className="flex items-center justify-between mb-1">
                                        <span className="text-sm font-semibold text-gray-800">
                                            {u.authorName}
                                        </span>
                                        <span className="text-xs text-gray-400">
                                            {formatDistanceToNow(new Date(u.createdAt), { addSuffix: true })}
                                        </span>
                                    </div>
                                    <p className="text-xs text-gray-500 mb-1">{u.projectName}</p>
                                    <p className="text-sm text-gray-600 line-clamp-2">{u.message}</p>
                                </div>
                            ))
                        )}
                    </div>
                    <Link
                        href="/notifications"
                        className="text-sm text-blue-600 hover:underline font-medium mt-4 block"
                    >
                        View all updates
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default MemberDashboard;
