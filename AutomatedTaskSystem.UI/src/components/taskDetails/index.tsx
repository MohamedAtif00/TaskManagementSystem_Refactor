import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import TaskIcon from "../../assets/Icons/Task";
import API, { BasicInfo } from "../../lib/API";
import PriorityDropDown from "../formComponents/PriorityDropDown";
import { AnimatePresence, motion } from "framer-motion";
import CrossIcon from "../../assets/Icons/Cross";
import StatusBadge from "./StatusBadge";
import LoBadge from "./LoBadge";
import TaskAction from "./TaskActions";
import DateLabel from "./DateLabel";

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
    status: "Backlog" | "To Do" | "Doing" | "Done" | "Rollback";
    flagged: boolean;
    comments: {
        user: {
            id: number;
            name: string;
        };
        id: number;
        content: string;
        timestamp: string;
    }[];
    startedAt?: string;
    doneAt?: string;
    priority?: number;
}

interface Props {
    projectId: number;
    refreshTasks: () => void;
}

const TaskDetails = ({ projectId, refreshTasks }: Props) => {
    const [task, setTask] = useState<ITask>();
    const [prio, setPrio] = useState<number | null>(null);
    const router = useRouter();
    const auth = useAppSelector((s) => s.authSlice);
    const [comment, setComment] = useState("");

    useEffect(() => {
        const id = router.query.taskId;
        if (id)
            API.TASKS.GET_ONE(id).then((res) => {
                if (res && !res.error) {
                    setTask(res.data);
                    setPrio(res.data.priority ? res.data.priority : null);
                }
            });
        else {
            setTask(undefined);
            setPrio(null);
        }
    }, [setTask, router.query.taskId]);

    const updatePrio = (value: null | number) => {
        setPrio(value);
        task &&
            API.TASKS.UPDATE_PRIORITY(
                task.id,
                value === 1 || value === 2 || value === 3 ? value : null
            ).then((res) => {
                if (res && !res.error) {
                    setTask(res.data);
                    refreshTasks();
                }
            });
    };
    const handleAddComment = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        task &&
            API.TASKS.COMMENT(task.learningObjective.id, comment).then(
                (res) => {
                    if (res && !res.error) {
                        setTask((ps) => {
                            return {
                                ...ps!,
                                comments: [res.data, ...ps!.comments],
                            };
                        });
                        setComment("");
                    }
                }
            );
    };

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
                            scale: 0,
                        }}
                        className="rounded-lg bg-white flex flex-col grow"
                    >
                        <div className="flex items-center justify-between border-b border-solid border-slate-200 px-6">
                            <div className="flex gap-4">
                                <div className="py-4 flex">
                                    <StatusBadge status={task.status} />
                                </div>
                                <div className="pl-[1px] bg-slate-200"></div>
                                <DateLabel
                                    label="Created"
                                    date={task.createdAt}
                                />
                                {task.startedAt !== undefined && (
                                    <>
                                        <div className="pl-[1px] bg-slate-200"></div>
                                        <DateLabel
                                            label="Started"
                                            date={task.startedAt}
                                        />
                                    </>
                                )}
                                {task.doneAt !== undefined && (
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
                        <div className="grid grid-cols-1 grow">
                            <div>
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
                            </div>
                        </div>
                    </motion.div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default TaskDetails;
