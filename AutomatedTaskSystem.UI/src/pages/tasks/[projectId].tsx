import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import AssignTask from "../../components/forms/tasks/assignToTask";
import Header from "../../components/header/header";
import TaskDetails from "../../components/taskDetails";
import API from "../../lib/API";
import styles from "../../styles/tasks.module.scss";
import Head from "next/head";
import Loader from "../../components/loader";
import CreateStandAloneTaskForm from "../../components/pageComponent/tasks/CreateStandAloneForm";

const Task = ({
    id,
    name,
    lo,
    flagged = false,
    attention,
    userName,
    from,
    isRollback,
    rollbackCount,
    priority,
}: {
    id: number;
    name: string;
    lo: string;
    flagged: boolean;
    attention: boolean;
    userName?: string;
    from: string;
    isRollback: boolean;
    rollbackCount: number;
    priority: number | null;
}) => {
    const router = useRouter();
    const projectId = router.query.projectId;

    return (
        <Link
            href={{
                pathname: `/tasks/${projectId}`,
                query: {
                    taskId: id,
                },
            }}
            className="relative"
        >
            <div
                className={[
                    "rounded-lg bg-white flex flex-col transition ease-in group",
                    flagged
                        ? "border-2 border-solid border-red-200"
                        : attention
                        ? "border-2 border-solid border-emerald-200"
                        : "border-2 border-solid border-blue-200",
                ].join(" ")}
            >
                {isRollback ? (
                    <div className="flex px-2 pt-4 justify-end">
                        <div className="py-1 text-white px-4 rounded-full bg-orange-600 flex gap-2 items-end border-solid border-2 border-orange-400">
                            <div className="text-xl font-bold">Rollback</div>
                            <div>{`#${rollbackCount}`}</div>
                        </div>
                    </div>
                ) : (
                    ""
                )}
                <div
                    className={`px-6 flex flex-col${isRollback ? "" : " pt-4"}`}
                >
                    <div className="text-lg text-black group-hover:text-2xl group-hover:mt-1 mt-0 transition-all ease-out">
                        <div className="whitespace-normal">{lo}</div>
                    </div>
                    <div
                        className={`flex ${
                            priority ? "justify-between" : "justify-end"
                        } mt-2`}
                    >
                        {priority && (
                            <div className="flex flex-col items-start">
                                <div className="text-xs opacity-60 flex justify-end">
                                    Priority:
                                </div>
                                {priority === 1 ? (
                                    <div className="font-bold text-red-600">
                                        High
                                    </div>
                                ) : priority === 2 ? (
                                    <div className="font-bold text-orange-500">
                                        Medium
                                    </div>
                                ) : priority === 3 ? (
                                    <div className="font-bold text-blue-600">
                                        Low
                                    </div>
                                ) : (
                                    ""
                                )}
                            </div>
                        )}
                        {from && (
                            <div>
                                <div className="text-xs opacity-60 text-orange-600 flex justify-end">
                                    From:
                                </div>
                                <div className="text-sm opacity-60">{from}</div>
                            </div>
                        )}
                    </div>
                </div>
                <div
                    className={`${
                        flagged
                            ? "bg-red-200"
                            : attention
                            ? "bg-emerald-200"
                            : "bg-blue-200"
                    } mt-4 px-6 flex flex-col gap-4 py-4 rounded-b-md`}
                >
                    <div className="font-medium whitespace-normal">
                        <div>{name}</div>
                    </div>
                    {userName ? (
                        <div className="text-slate-600">
                            <div>{userName}</div>
                        </div>
                    ) : (
                        ""
                    )}
                </div>
            </div>
        </Link>
    );
};

const Tasks = () => {
    const [tasks, setTasks] = useState<TaskInfo[]>();
    const [filteredTasks, setFilteredTasks] = useState<TaskInfo[]>([]);
    const [project, setProject] = useState<IProject>();
    const [loFilter, setLoFilter] = useState(0);
    const auth = useAppSelector((e) => e.authSlice);
    const router = useRouter();

    useEffect(() => {
        if (loFilter > 0 && tasks !== undefined) {
            setFilteredTasks(
                tasks
                    .filter((_) => _.learningObjective.id === loFilter)
                    .sort((A, B) => {
                        const a = A.learningObjective.name.toLowerCase(),
                            b = B.learningObjective.name.toLowerCase();
                        return a > b ? 1 : a < b ? -1 : 0;
                    })
            );
            return;
        }
        tasks &&
            setFilteredTasks(
                tasks.sort((A, B) => {
                    const a = A.learningObjective.name.toLowerCase(),
                        b = B.learningObjective.name.toLowerCase();
                    return a > b ? 1 : a < b ? -1 : 0;
                })
            );
    }, [loFilter, tasks, setFilteredTasks]);

    useEffect(() => {
        const id = router.query.projectId;
        if (id !== undefined)
            API.PROJECTS.GET_ONE(id).then(
                (res) => res && !res.error && setProject(res.data)
            );
    }, [router.query.projectId]);

    useEffect(() => {
        project &&
            API.TASKS.GET_ALL(project.id.toString()).then(
                (res) => res && !res.error && setTasks(res.data)
            );
    }, [project]);

    useEffect(() => {
        if (project) {
            const refreshInterval = setInterval(() => {
                API.TASKS.GET_ALL(project.id.toString()).then(
                    (res) => res && !res.error && setTasks(res.data)
                );
            }, 60000);
            return () => clearInterval(refreshInterval);
        }
    }, [project]);

    if (tasks === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    const refreshTasks = () => {
        const projectId = router.query.projectId;
        if (projectId) {
            API.TASKS.GET_ALL(projectId).then(
                (res) => res && !res.error && setTasks(res.data)
            );
        }
    };

    const los: { id: number; name: string }[] = [];
    tasks.forEach((t) => {
        if (!los.find((_) => _.id === t.learningObjective.id))
            los.push({
                id: t.learningObjective.id,
                name: t.learningObjective.name,
            });
    });

    if (!project) return <div>Loading</div>;

    const view = {
        backlog: filteredTasks.filter((t) => t.status === "Backlog"),
        todo: filteredTasks.filter((t) => t.status === "To Do"),
        doing: filteredTasks.filter((t) => t.status === "Doing"),
        done: filteredTasks.filter(
            (t) => t.status === "Done" || t.status === "Rollback"
        ),
    };

    return (
        <>
            <Head>
                <title>ATS - {project.name} Tasks</title>
            </Head>
            <div className={["w-full", styles.container].join(" ")}>
                <Header text={project.name} icon="Task">
                    {auth.role !== 4 ? (
                        <QueryButton
                            icon={<PlusIcon />}
                            text="New Task"
                            url={{
                                pathname: `/tasks/${project.id}`,
                                query: {
                                    form: "new-task",
                                },
                            }}
                        />
                    ) : (
                        <></>
                    )}
                    <div>
                        <select
                            className="text-base font-normal"
                            value={loFilter}
                            onChange={(e) => {
                                const value = e.target.value;
                                const id = parseInt(value);

                                if (isNaN(id)) setLoFilter(0);
                                else setLoFilter(id);
                            }}
                        >
                            <option value={0}>None</option>
                            {los.map((lo) => (
                                <option key={lo.id} value={lo.id}>
                                    {lo.name}
                                </option>
                            ))}
                        </select>
                    </div>
                </Header>
                <div className={styles.tasks}>
                    <div className={styles.col}>
                        <h3>Backlogs</h3>
                        {[
                            ...view.backlog.filter((t) => t.priority === 1),
                            ...view.backlog.filter((t) => t.priority === 2),
                            ...view.backlog.filter((t) => t.priority === 3),
                            ...view.backlog.filter((t) => !t.priority),
                        ].map((t) => (
                            <Task
                                priority={t.priority}
                                attention={t.attention}
                                id={t.id}
                                key={t.id}
                                name={t.name}
                                lo={t.learningObjective.name}
                                flagged={t.flagged}
                                userName={t.user && t.user.name}
                                from={t.from}
                                isRollback={t.isRollback}
                                rollbackCount={t.rollbackCount}
                            />
                        ))}
                    </div>
                    <div className={styles.col}>
                        <h3>To Do</h3>
                        {[
                            ...view.todo.filter((t) => t.priority === 1),
                            ...view.todo.filter((t) => t.priority === 2),
                            ...view.todo.filter((t) => t.priority === 3),
                            ...view.todo.filter((t) => !t.priority),
                        ].map((t) => (
                            <Task
                                priority={t.priority}
                                attention={t.attention}
                                id={t.id}
                                key={t.id}
                                name={t.name}
                                lo={t.learningObjective.name}
                                flagged={t.flagged}
                                userName={t.user && t.user.name}
                                from={t.from}
                                isRollback={t.isRollback}
                                rollbackCount={t.rollbackCount}
                            />
                        ))}
                    </div>
                    <div className={styles.col}>
                        <h3>Doing</h3>
                        {[
                            ...view.doing.filter((t) => t.priority === 1),
                            ...view.doing.filter((t) => t.priority === 2),
                            ...view.doing.filter((t) => t.priority === 3),
                            ...view.doing.filter((t) => !t.priority),
                        ].map((t) => {
                            return (
                                <Task
                                    priority={t.priority}
                                    attention={t.attention}
                                    id={t.id}
                                    key={t.id}
                                    name={t.name}
                                    lo={t.learningObjective.name}
                                    flagged={t.flagged}
                                    userName={t.user && t.user.name}
                                    from={t.from}
                                    isRollback={t.isRollback}
                                    rollbackCount={t.rollbackCount}
                                />
                            );
                        })}
                    </div>
                    <div className={styles.col}>
                        <h3>Done</h3>
                        {view.done.map((t) => {
                            return (
                                <Task
                                    priority={null}
                                    id={t.id}
                                    attention={t.attention}
                                    key={t.id}
                                    name={t.name}
                                    lo={t.learningObjective.name}
                                    flagged={t.flagged}
                                    userName={t.user && t.user.name}
                                    from={t.from}
                                    isRollback={t.isRollback}
                                    rollbackCount={t.rollbackCount}
                                />
                            );
                        })}
                    </div>
                </div>
                <TaskDetails
                    refreshTasks={refreshTasks}
                    projectId={project.id}
                />
                {router.query.form === "task-assign" && (
                    <AssignTask
                        taskId={router.query.taskId!}
                        refreshTask={refreshTasks}
                    />
                )}
                <CreateStandAloneTaskForm
                    refreshTasks={refreshTasks}
                    projectId={project.id}
                />
            </div>
        </>
    );
};

export default Tasks;
