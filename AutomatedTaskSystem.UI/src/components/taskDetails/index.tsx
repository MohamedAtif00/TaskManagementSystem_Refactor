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
import TaskComments from "./taskComments";
import RollbackForm from "../../components/forms/tasks/rollback";
import { ClockIcon } from "@heroicons/react/24/solid";
import TaskActivity from "./RecentActivity/taskActivity";
import AssignTask from "../forms/tasks/assignToTask";
import DurationBadge from "./durationBadge";
import Link from "next/link";
import EditCommentForm from "./taskComments/EditCommentForm";
import DeleteCommentForm from "./taskComments/DeleteComment";

export interface IComment {
    user: {
        id: number;
        name: string;
    };
    id: number;
    content: string;
    timestamp: string;
    isEdited: boolean;
    isDeleted: boolean;
}

export interface ITaskActivity {
    id: number;
    type: TaskActivityType;
    secondaryTask?: BasicInfo;
    actorOne?: BasicInfo;
    actorTwo?: BasicInfo;
    timeStamp: string;
    additionalInfo?: string;
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
    activities: ITaskActivity[];
    duration: number;
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
                if (res && !res.error) setTask(res.data);
            });
        else setTask(undefined);
    }, [setTask, router.query.taskId]);

    const handleUpdate = (res: ITask) => {
        setTask(res);
        refreshTasks();
    };

    const reload = () => {
        const id = router.query.taskId;
        if (id)
            API.TASKS.GET_ONE(id).then((res) => {
                if (res && !res.error) setTask(res.data);
            });
    };

    const exit = () => router.push(`/tasks/${projectId}`);

    const proceedTask = () => {
        if (task) {
            API.TASKS.PROCEED(task.id).then((res) => {
                if (res && !res.error) handleUpdate(res.data);
            });
            router.push(`/tasks/${projectId}?taskId=${task.id}`);
        }
    };

    return (
        <AnimatePresence>
            {task && (
                <motion.div
                    key="main-task"
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
                                {task.duration > 0 && (
                                    <DurationBadge duration={task.duration} />
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
                                    projectId={projectId}
                                    taskId={task.id}
                                    updateTask={setTask}
                                    comments={task.comments}
                                    reload={reload}
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
                                    {task.activities.reduce((acc, value, i) => {
                                        const previousValue =
                                            i > 0 && task.activities[i - 1];
                                        const newAcc = [...acc];

                                        if (previousValue) {
                                            const previousDate = new Date(
                                                previousValue.timeStamp
                                            ).getTime();
                                            const valueDate = new Date(
                                                value.timeStamp
                                            ).getTime();

                                            const diffInSeconds =
                                                (previousDate - valueDate) /
                                                1000;

                                            const time = {
                                                days: Math.floor(
                                                    diffInSeconds / 86400
                                                ),
                                                hours:
                                                    Math.floor(
                                                        diffInSeconds / 3600
                                                    ) % 24,
                                                minutes:
                                                    Math.floor(
                                                        diffInSeconds / 60
                                                    ) % 60,
                                            };

                                            newAcc.push(
                                                <div
                                                    key={`${value.id}-spliter-${i}`}
                                                    className="flex items-center flex-col"
                                                >
                                                    <div className="pl-1 bg-slate-300 h-4 rounded-b-full"></div>
                                                    <div className="text-sm text-slate-500">
                                                        {time.days > 0 &&
                                                            `${time.days} Days, `}
                                                        {time.hours > 0
                                                            ? `${time.hours}:${
                                                                  time.minutes <
                                                                  10
                                                                      ? `0${time.minutes} Hours`
                                                                      : `${time.minutes} Hours`
                                                              }`
                                                            : time.minutes < 10
                                                            ? `0${time.minutes} Minutes`
                                                            : `${time.minutes} Minutes`}
                                                    </div>
                                                    <div className="pl-1 bg-slate-300 h-4 rounded-t-full"></div>
                                                </div>
                                            );
                                        }

                                        newAcc.push(
                                            <TaskActivity
                                                key={value.id}
                                                name={task.name}
                                                {...value}
                                            />
                                        );

                                        return newAcc;
                                    }, [] as JSX.Element[])}
                                </div>
                            </div>
                        </div>
                    </motion.div>
                </motion.div>
            )}
            {task && router.query.form === "task-assign" && (
                <AssignTask
					key="assign-task-form"
                    taskId={task.id.toString()}
                    refreshTask={() => {
                        reload();
                        refreshTasks();
                    }}
                />
            )}
            {task && router.query.form === "proceed" && (
                <div
                    key="confirmation-message"
                    className="fixed z-50 top-0 left-0 right-0 bottom-0 bg-black/25 flex items-center justify-center"
                >
                    <div className="bg-white rounded-lg border-slate-200 border border-solid">
                        <h4 className="text-2xl font-bold px-8 pt-4 pb-2 border-slate-200 border-b border-solid mb-2">
                            Complete Task
                        </h4>
                        <p className="px-8 pt-4">
                            Are you sure you want to continue?
                        </p>
                        <div className="grid grid-cols-2 px-8 py-4 gap-2">
                            <Link
                                href={{
                                    pathname: `/tasks/${projectId}`,
                                    query: {
                                        taskId: task.id,
                                    },
                                }}
                            >
                                <div className="bg-black text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1">
                                    Cancel
                                </div>
                            </Link>
                            <div
                                className="bg-emerald-500 text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1 cursor-pointer"
                                onClick={proceedTask}
                            >
                                Confirm
                            </div>
                        </div>
                    </div>
                </div>
            )}
            {task &&
                router.query.form === "edit-comment" &&
                router.query.commentId && (
                    <EditCommentForm
						key="edit-comment-form"
                        update={reload}
                        taskId={task.id}
                        projectId={projectId}
                        comment={task.comments.find((c) => {
                            const cid = parseInt(
                                router.query.commentId!.toString()
                            );

                            return !isNaN(cid) && c.id === cid;
                        })}
                    />
                )}
            {task &&
                router.query.form === "delete-comment" &&
                router.query.commentId && (
                    <DeleteCommentForm
						key="delete-comment-form"
                        update={reload}
                        taskId={task.id}
                        projectId={projectId}
                        comment={task.comments.find((c) => {
                            const cid = parseInt(
                                router.query.commentId!.toString()
                            );

                            return !isNaN(cid) && c.id === cid;
                        })}
                    />
                )}
        </AnimatePresence>
    );
};

export default TaskDetails;
