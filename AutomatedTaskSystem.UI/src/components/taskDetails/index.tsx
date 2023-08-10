import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import HandPointingIcon from "../../assets/Icons/HandPointing";
import TaskIcon from "../../assets/Icons/Task";
import API from "../../lib/API";
import QueryButton from "../button/queryButton";
import ExpansionPanel from "../expansionPanel";
import Backdrop from "../forms/backdrop";
import RollbackForm from "../forms/tasks/rollback";
import PriorityDropDown from "../formComponents/PriorityDropDown";
import { AnimatePresence, motion } from "framer-motion";
import CrossIcon from "../../assets/Icons/Cross";
import StatusBadge from "./StatusBadge";
import LoBadge from "./LoBadge";

const dateHandler = (params: string) => {
    const date = new Date(params);

    const WeekDays = new Map();

    const MonthNames = new Map<number, string>();

    MonthNames.set(0, "Jan");
    MonthNames.set(1, "Feb");
    MonthNames.set(2, "Mar");
    MonthNames.set(3, "Apr");
    MonthNames.set(4, "May");
    MonthNames.set(5, "Jun");
    MonthNames.set(6, "Jul");
    MonthNames.set(7, "Aug");
    MonthNames.set(8, "Sep");
    MonthNames.set(9, "Oct");
    MonthNames.set(10, "Nov");
    MonthNames.set(11, "Dec");

    WeekDays.set(0, "Sun");
    WeekDays.set(1, "Mon");
    WeekDays.set(2, "Tue");
    WeekDays.set(3, "Wed");
    WeekDays.set(4, "Thu");
    WeekDays.set(5, "Fri");
    WeekDays.set(6, "Sat");

    const TimeDiff = Date.now() - date.getTime();

    let time: string;

    const h = Math.floor(TimeDiff / 3600000);
    const d = Math.floor(TimeDiff / 86400000);

    if (h < 1 && h >= 0) {
        time = `${Math.floor(TimeDiff / 60000)} Minutes ago`;
    } else if (d === 0) {
        time = `${Math.floor(h)} Hour${h > 1 ? "s" : ""} Ago`;
    } else if (d > 0 && d < 2) {
        time = `Yeseterday, ${
            date.getHours() % 12 < 10 ? "0" : ""
        }${date.getHours()}:${
            date.getMinutes() < 10 ? "0" : ""
        }${date.getMinutes()}`;
    } else if (d > 0 && d < 7) {
        time = `${WeekDays.get(date.getDay())}, ${
            date.getHours() % 12 < 10 ? "0" : ""
        }${date.getHours()}:${
            date.getMinutes() < 10 ? "0" : ""
        }${date.getMinutes()}`;
    } else if (d > 0 && d < 365) {
        time = `${MonthNames.get(date.getMonth())}, ${date.getDate()}`;
    } else {
        time = `${MonthNames.get(
            date.getMonth()
        )}, ${date.getDate()}, ${date.getFullYear()}`;
    }

    return time;
};

export interface ITask {
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

    const proceedTask = () => {
        task &&
            API.TASKS.PROCEED(task.id).then((res) => {
                if (res) {
                    refreshTasks();
                    if (!res.error) setTask(res.data);
                }
            });
    };
    const flagTask = () => {
        task &&
            API.TASKS.FLAG_TASK(task.id).then((res) => {
                if (res) {
                    if (!res.error) setTask(res.data);
                    refreshTasks();
                }
            });
    };
    const pauseTask = () => {
        task &&
            API.TASKS.PAUSE(task.id).then((res) => {
                if (res) {
                    if (!res.error) setTask(res.data);
                    refreshTasks();
                }
            });
    };
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
                            translateY: "50%",
                            opacity: 0,
                        }}
                        animate={{
                            translateY: 0,
                            opacity: 1,
                        }}
                        transition={{
                            type: "tween",
                        }}
                        exit={{
                            opacity: 0,
                            translateY: "100%",
                        }}
                        className="rounded-lg bg-white flex flex-col gap-2 grow"
                    >
                        <div className="flex items-center justify-between border-b border-solid border-slate-200 px-6">
                            <div className="py-4">
                                <StatusBadge status={task.status} />
                            </div>
                            <button className="p-1 box-content" onClick={exit}>
                                <CrossIcon className="stroke-black" />
                            </button>
                        </div>
                        <div className="flex items-center justify-start text-xl px-6 mt-3">
                            <TaskIcon className="w-7 h-7 stroke-black mr-2" />
                            <div className="font-bold mr-2">{task.name}</div>
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
                    </motion.div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default TaskDetails;
