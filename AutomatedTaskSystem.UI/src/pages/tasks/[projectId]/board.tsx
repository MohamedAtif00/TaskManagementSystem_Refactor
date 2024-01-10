import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../../app/hooks";
import TaskDetails from "../../../components/taskDetails";
import API from "../../../lib/API";
import Head from "next/head";
import Loader from "../../../components/loader";
import CreateStandAloneTaskForm from "../../../components/pageComponent/tasks/CreateStandAloneForm";
import TaskCol from "../../../components/pageComponent/tasks/TasksCol";
import useTaskPathHandler from "../../../components/taskDetails/useTaskPathHandler.ts";
import TaskIcon from "../../../assets/Icons/Task";
import Link from "next/link";

const TaskBoard = () => {
    const [tasks, setTasks] = useState<TaskInfo[]>();
    const [filteredTasks, setFilteredTasks] = useState<TaskInfo[]>([]);
    const [project, setProject] = useState<IProject>();
    const [toggleFilter, setToggleFilter] = useState(false);
    const [loFilter, setLoFilter] = useState("");
    const auth = useAppSelector((e) => e.authSlice);
    const router = useRouter();
    const pathHandler = useTaskPathHandler();

    useEffect(() => {
        const id = router.query.projectId;
        if (id !== undefined)
            API.PROJECTS.GET_ONE(id).then(
                (res) => res && !res.error && setProject(res.data)
            );
    }, [router.query.projectId]);

    useEffect(() => {
        project &&
            API.TASKS.GET_ALL_CARDS(project.id.toString()).then(
                (res) => { if (res && !res.error) { setTasks(res.data); setFilteredTasks(res.data); } }
            );
    }, [project]);

    useEffect(() => {
        if (toggleFilter && tasks) {
            setFilteredTasks(tasks);
        }
    }, [tasks, setFilteredTasks, toggleFilter])

    useEffect(() => {
        if (project) {
            const refreshInterval = setInterval(() => {
                API.TASKS.GET_ALL_CARDS(project.id.toString()).then(
                    (res) => { if (res && !res.error) { setTasks(res.data); searchLos(); } }
                );
            }, 30000);
            return () => clearInterval(refreshInterval);
        }
    }, [project]);

    const searchLos = () => {
        if (loFilter !== "" && tasks !== undefined) {
            setToggleFilter(true);
            setFilteredTasks(
                tasks
                    .filter((_) => _.learningObjective.name.toLowerCase().split("_").join("").includes(loFilter.toLowerCase().split("_").join("")))
                    .sort((A, B) => {
                        const a = A.learningObjective.name.toLowerCase(),
                            b = B.learningObjective.name.toLowerCase();
                        return a > b ? 1 : a < b ? -1 : 0;
                    })
            );
            return;
        } else {
            setToggleFilter(false);
        }
        tasks &&
            setFilteredTasks(
                tasks.sort((A, B) => {
                    const a = A.learningObjective.name.toLowerCase(),
                        b = B.learningObjective.name.toLowerCase();
                    return a > b ? 1 : a < b ? -1 : 0;
                })
            );
    }

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
        if (projectId)
            API.TASKS.GET_ALL_CARDS(projectId).then(
                (res) => {
                    if (res && !res.error)
                        setTasks(res.data)
                }
            );
    };

    if (!project) return <div>Loading</div>;

    const view = {
        backlog: (toggleFilter ? filteredTasks : tasks).filter((t) => t.status === 0),
        todo: (toggleFilter ? filteredTasks : tasks).filter((t) => t.status === 1),
        doing: (toggleFilter ? filteredTasks : tasks).filter((t) => t.status === 2),
        done: (toggleFilter ? filteredTasks : tasks).filter(
            (t) => t.status === 3 || t.status === 4
        ),
    };

    return (
        <>
            <Head>
                <title>ATS - {project.name} Tasks</title>
            </Head>
            <div className="w-full h-screen overflow-hidden flex flex-col">
                <div className="px-8 relative">
                    <div className="sticky left-0 right-0 top-0 px-4 bg-white py-4 rounded-b-md border-solid border-2 border-sky-950 border-t-0 flex justify-between">
                        <div className="flex items-center gap-2">
                            <TaskIcon color={"#29313d"} />
                            <span className="text-2xl font-bold">{project.name}</span>
                        </div>
                        <div className="flex gap-4 items-center">
                            <div>
                                <Link href={{
                                    pathname: `/tasks/${project.id}/sheet`,
                                }} onClick={() => {
                                    localStorage.setItem("tasks:view", "sheet");
                                }}>
                                    <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-pink-700 hover:text-pink-700 transition ease-in">Sheet View</button>
                                </Link>
                            </div>
                            <div>
                                {auth.role !== 3 &&
                                    <Link href={{
                                        pathname: pathHandler(),
                                        query: {
                                            form: "new-task",
                                        },
                                    }}>
                                        <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-green-600 hover:text-green-600 transition ease-in">New Task</button>
                                    </Link>}
                            </div>
                            <div>
                                <form className="flex gap-4 items-end" onSubmit={e => { e.preventDefault(); searchLos(); }}>
                                    <label>
                                        <div className="text-xs mb-1">Search LO:</div>
                                        <input type="text" className="px-2 py-1 bg-slate-200 border-slate-400 border border-solid rounded-md text-sm" onChange={e => setLoFilter(e.target.value)} value={loFilter} />
                                    </label>
                                    <input type="submit" value="Search" className="px-2 py-1 bg-sky-500 border border-solid border-sky-300 rounded-md text-white text-sm" />
                                </form>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="px-8 overflow-x-auto flex grow">
                    <div className="flex gap-1 bg-slate-50">
                        <TaskCol label="Backlog" items={view.backlog} />
                        <TaskCol label="To Do" items={view.todo} />
                        <TaskCol label="Doing" items={view.doing} />
                        <TaskCol label="Done" items={view.done} />
                    </div>
                </div>
                <TaskDetails
                    refreshTasks={refreshTasks}
                />
                <CreateStandAloneTaskForm
                    refreshTasks={refreshTasks}
                    projectId={project.id}
                />
            </div>
        </>
    );
};

export default TaskBoard;

