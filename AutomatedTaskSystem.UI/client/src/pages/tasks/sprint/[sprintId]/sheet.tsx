import { Fragment, useEffect, useState } from "react";
import { useAppSelector } from "../../../../app/hooks";
import Link from "next/link";
import TaskIcon from "../../../../assets/Icons/Task";
import useTaskPathHandler from "../../../../components/taskDetails/useTaskPathHandler.ts";
import Head from "next/head";
import Loader from "../../../../components/loader";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import { ArrowPathRoundedSquareIcon } from "@heroicons/react/24/outline";
import TaskDetails from "../../../../components/taskDetails";
import CreateStandAloneTaskForm from "../../../../components/pageComponent/tasks/CreateStandAloneForm";
import { PauseIcon } from "@heroicons/react/24/solid";

interface LocalProject {
    id: number;
    name: string;
    workableTasks: BasicInfo[];
}

const SheetView = () => {
    const [project, setProject] = useState<LocalProject>();
    const [units, setUnits] = useState<UnitChip[]>();
    const auth = useAppSelector((e) => e.authSlice);
    const router = useRouter();
    const pathHandler = useTaskPathHandler({type:"tasks"});

    useEffect(() => {
        const id = router.query.projectId;
        if (id !== undefined)
            API.TASKS.GET_PROJECT_SHEET(id).then(
                (res) => {
                    if (res && !res.error) {
                        const { id, name, workableTasks } = res.data;
                        setProject({
                            id, name, workableTasks
                        });
                        setUnits(res.data.units);
                    }
                }
            );
    }, [router.query.projectId]);

    if (project === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

    const refreshTasks = () => {
        API.TASKS.GET_PROJECT_SHEET(project.id.toString()).then(
            (res) => {
                if (res && !res.error)
                    setUnits(res.data.units)
            }
        );
    };

    return (<div className="grow max-w-full">
        <Head>
            <title>TMS - {project.name} Tasks</title>
        </Head>
        <div className="sticky left-0 right-0 top-0 px-4 bg-white py-4 border-solid border-b border-sky-950 border-t-0 flex justify-between">
            <div className="flex items-center gap-2">
                <TaskIcon color={"#29313d"} />
                <span className="text-2xl font-bold">{project.name}</span>
            </div>
            <div className="flex gap-4 items-center">
                <div>
                    <Link href={{
                        pathname: `/tasks/${project.id}/board`,
                    }} onClick={() => {
                        localStorage.setItem("tasks:view", "board");
                    }}>
                        <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-pink-700 hover:text-pink-700 transition ease-in">Board View</button>
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
            </div>
        </div>
        <div className="overflow-auto py-4 relative">
            {units?.map(u => {
                return (
                    <Fragment key={`unit:${u.id}`}>
                        <div className="px-2 py-1 text-xl sticky left-0 bg-white">{u.name}</div>
                        {u.lessons.map(l => <Fragment key={`lesson:${l.id}`}>
                            <div className="px-2 py-1 text-xl sticky left-0 bg-white">{l.name}</div>
                            {l.los.map(lo =>
                                <Fragment key={`lo:${lo.id}`}>
                                    <div className="flex gap-2 px-2 sticky left-0 bg-white">
                                        <div>{lo.name}</div>
                                        <div>{lo.tag}</div>
                                        <div>{lo.template}</div>
                                        <div>{lo.environment}</div>
                                        <div>{lo.schema.name}</div>
                                    </div>
                                    <div className="flex gap-1 px-2">
                                        {lo.tasks.map(t =>
                                            <Link
                                                href={{
                                                    pathname: `/tasks/${project.id}/sheet`,
                                                    query: {
                                                        taskId: t.id,
                                                    },
                                                }}
                                                key={`task:${t.id}`}
                                                className={`flex py-1 rounded border border-solid gap-1 ${t.status === 0 ? "text-black" : "text-white"} whitespace-nowrap ${t.status === 0 ? "bg-white" : t.status === 1 ? "bg-blue-600" : t.status === 2 ? "bg-orange-600" : t.status === 3 ? "bg-green-600" : "bg-red-500"}`}
                                            >
                                                <div className="flex items-center justify-center flex-col">
                                                    <div className="flex px-2 gap-1">
                                                        {t.isRollback && <div>
                                                            <ArrowPathRoundedSquareIcon className="h-6 w-6" />
                                                        </div>}
                                                        <div>{t.name}</div>
                                                        {t.paused && <div className="p-1 rounded-full">
                                                            <PauseIcon className="h-4 w-4" />
                                                        </div>}
                                                    </div>
                                                    {
                                                        t.user &&
                                                        <div className={`text-xs border-solid border-t border-white/30 w-full text-center px-2 ${t.status === 0 ? "text-black/75" : "text-white/75"}`}>{t.user.name}</div>
                                                    }
                                                </div>
                                            </Link>)}
                                    </div>
                                </Fragment>
                            )}
                        </Fragment>)}
                    </Fragment>
                );
            })}
        </div>
        <TaskDetails
            refreshTasks={refreshTasks}
            type="tasks"
        />
        <CreateStandAloneTaskForm
            refreshTasks={refreshTasks}
            projectId={project.id}
            type="tasks"
        />
    </div>);
}

export default SheetView;
