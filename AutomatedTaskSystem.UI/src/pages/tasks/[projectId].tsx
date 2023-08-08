import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import QueryButton from "../../components/button/queryButton";
import AssignTask from "../../components/forms/tasks/assignToTask";
import Header from "../../components/header/header";
import TaskDetails from "../../components/taskDetails";
import API from "../../lib/API";
import Head from "next/head";
import Loader from "../../components/loader";
import CreateStandAloneTaskForm from "../../components/pageComponent/tasks/CreateStandAloneForm";
import TaskCard from "../../components/pageComponent/tasks/TaskCard";
import TaskCol from "../../components/pageComponent/tasks/TasksCol";

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

    console.log(view);

    return (
        <>
            <Head>
                <title>ATS - {project.name} Tasks</title>
            </Head>
            <div
                className={
                    "w-full h-screen overflow-hidden flex flex-col gap-4"
                }
            >
                <div className="px-8">
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
                </div>
                <div className="px-8 overflow-x-auto flex grow">
                    <div className="flex gap-1 bg-slate-50">
                        <TaskCol
                            label="Backlog"
                            items={(() => {
                                console.log(view.backlog);
                                return view.backlog;
                            })()}
                        />
                        <TaskCol label="To Do" items={view.todo} />
                        <TaskCol label="Doing" items={view.doing} />
                        <TaskCol label="Done" items={view.done} />
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
