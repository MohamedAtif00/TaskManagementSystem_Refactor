import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import TaskIcon from "../../assets/Icons/Task";
import API, { BasicInfo } from "../../lib/API";
import { AnimatePresence, motion } from "framer-motion";
import CrossIcon from "../../assets/Icons/Cross";
import StatusBadge from "./StatusBadge";
import LoBadge from "./LoBadge";
import TaskAction from "./TaskActions";
import DateLabel from "./DateLabel";
import TaskComments from "./TaskComments";
import RollbackForm from "../../components/forms/tasks/rollback";
import { ClockIcon } from "@heroicons/react/24/solid";
import CreatedActivity from "./RecentActivity/createdActivity";

export interface IComment {
    user: {
        id: number;
        name: string;
    };
    id: number;
    content: string;
    timestamp: string;
}

export interface ITask {
    createdAt: string;
    user: BasicInfo | null;
    access: "WorkOn" | "Manage" | "WorkOnAndManage" | "None";
    pause: boolean;
    error: false;
    id: number;
    name: string;
    learningObjective: { id: number; name: string };
    tag: string;
    template: string;
    environment: string;
    schema: { id: number; name: string };
    isReview: boolean;
    status: TaskStatus;
    flagged: boolean;
    comments: IComment[];
    startedAt: string | null;
    doneAt: string | null;
    priority: TaskPriority;
}

interface Props {
    projectId: number;
    refreshTasks: () => void;
}

const TaskDetails = ({ projectId, refreshTasks }: Props) => {
    const [task, setTask] = useState<ITask>();
    const router = useRouter();

    useEffect(() => {
        const id = router.query.taskId;
        if (id)
            API.TASKS.GET_ONE(id).then((res) => {
                if (res && !res.error) {
                    setTask(res.data);
                }
            });
        else setTask(undefined);
    }, [setTask, router.query.taskId]);

    const handleUpdate = (res: ITask) => {
        setTask(res);
        refreshTasks();
    };

    const exit = () => router.push(`/tasks/${projectId}`);

    return (
        <AnimatePresence>
            {task && (
                <motion.div
                    initial={{
                        backgroundColor: "#00000000",
                    }}
                    animate={{
                        backgroundColor: "#00000066",
                    }}
                    exit={{
                        backgroundColor: "#00000000",
                        scale: 1.1,
                    }}
                    className="p-12 fixed top-0 left-0 right-0 bottom-0 z-40 flex justify-end"
                >
                    <motion.div
                        initial={{
                            scale: 0.5,
                            opacity: 0,
                        }}
                        animate={{
                            opacity: 1,
                            scale: 1,
                        }}
                        transition={{
                            type: "just",
                        }}
                        exit={{
                            opacity: 0,
                        }}
                        className="rounded-lg bg-white flex flex-col grow"
                    >
                        <div className="flex items-center justify-between border-b border-solid border-slate-200 px-6 shrink-0">
                            <div className="flex gap-4">
                                <div className="py-4 flex">
                                    <StatusBadge status={task.status} />
                                </div>
                                {task.priority !== 0 && (
                                    <>
                                        <div className="pl-[1px] bg-slate-200"></div>
                                        <div className="text-sm flex flex-col justify-center items-start text-slate-600">
                                            <div>Priority:</div>
                                            <div
                                                className={`text-base font-bold ${
                                                    task.priority === 1
                                                        ? "text-rose-400"
                                                        : task.priority === 2
                                                        ? "text-orange-400"
                                                        : "text-blue-400"
                                                }`}
                                            >
                                                {task.priority === 1
                                                    ? "High"
                                                    : task.priority === 2
                                                    ? "Medium"
                                                    : "Low"}
                                            </div>
                                        </div>
                                    </>
                                )}
                                <div className="pl-[1px] bg-slate-200"></div>
                                <DateLabel
                                    label="Created"
                                    date={task.createdAt}
                                />
                                {task.startedAt !== null && (
                                    <>
                                        <div className="pl-[1px] bg-slate-200"></div>
                                        <DateLabel
                                            label="Started"
                                            date={task.startedAt}
                                        />
                                    </>
                                )}
                                {task.doneAt !== null && (
                                    <>
                                        <div className="pl-[1px] bg-slate-200"></div>
                                        <DateLabel
                                            label="Done"
                                            date={task.doneAt}
                                        />
                                    </>
                                )}
                            </div>
                            <button className="p-1 box-content" onClick={exit}>
                                <CrossIcon className="stroke-black" />
                            </button>
                        </div>
                        <div className="grid grid-cols-3 grow overflow-y-hidden">
                            <div className="flex flex-col overflow-y-auto col-span-2">
                                <div className="flex items-center justify-start text-xl px-6 mt-3">
                                    <TaskIcon className="w-7 h-7 stroke-black mr-2" />
                                    <div className="font-bold mr-2">
                                        {task.name}
                                    </div>
                                    <div className="p-1 bg-slate-400 rounded-full"></div>
                                    <div className="ml-2 font-bold">
                                        {task.learningObjective.name}
                                    </div>
                                </div>
                                <div className="flex gap-2 text-slate-600 mt-4 px-6">
                                    <LoBadge
                                        text={task.schema.name}
                                        label="Schema"
                                    />
                                    {task.environment !== "" && (
                                        <LoBadge
                                            text={task.environment}
                                            label="Environment"
                                        />
                                    )}
                                    {task.tag !== "" && (
                                        <LoBadge text={task.tag} label="Tag" />
                                    )}
                                    {task.template !== "" && (
                                        <LoBadge
                                            text={task.template}
                                            label="Template"
                                        />
                                    )}
                                </div>
                                <TaskAction
                                    priority={task.priority}
                                    projectId={projectId}
                                    isReview={task.isReview}
                                    pause={task.pause}
                                    flag={task.flagged}
                                    taskId={task.id}
                                    handleUpdate={handleUpdate}
                                    user={task.user}
                                    status={task.status}
                                    access={task.access}
                                />
                                <TaskComments
                                    loId={task.learningObjective.id}
                                    updateTask={setTask}
                                    comments={task.comments}
                                />
                                <RollbackForm
                                    update={(data) => {
                                        setTask(data);
                                        router.push(
                                            `/tasks/${router.query.projectId}?taskId=${task.id}`
                                        );
                                        refreshTasks();
                                    }}
                                />
                            </div>
                            <div className="bg-blue-50 flex flex-col overflow-hidden rounded-br-lg">
                                <h1 className="px-6 mt-3 font-bold text-xl flex gap-2 items-center text-slate-700">
                                    <ClockIcon className="h-7 w-7" />
                                    <div>Recent Activity</div>
                                </h1>
                                <div className="grow flex items-center flex-col overflow-y-auto px-6 py-4">
                                    <CreatedActivity
                                        date={task.createdAt}
                                        name={task.name}
                                    />
                                    <div className="justify-center items-center flex flex-col gap-2">
                                        <div className="pl-1 h-8 rounded-b-full bg-slate-300"></div>
                                        <div className="text-sm text-slate-500">
                                            2 Hours
                                        </div>
                                        <div className="pl-1 h-8 rounded-full bg-slate-300"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </motion.div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default TaskDetails;
