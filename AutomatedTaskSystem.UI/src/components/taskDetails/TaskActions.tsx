import HandPointingIcon from "../../assets/Icons/HandPointing";
import { BasicInfo } from "../../lib/API";
import {
    PlusIcon,
    PauseIcon,
    FlagIcon,
    UserIcon,
    ExclamationCircleIcon,
    ForwardIcon,
    ArrowUturnDownIcon,
} from "@heroicons/react/24/outline";
import { PlayIcon, CheckIcon, ArrowPathIcon } from "@heroicons/react/24/solid";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { ITask } from ".";
import Link from "next/link";
import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import JumpForm from "./jumpForm";

interface Props {
    access: "WorkOn" | "Manage" | "WorkOnAndManage" | "None";
    user: BasicInfo | null;
    status: "Backlog" | "To Do" | "Doing" | "Done" | "Rollback";
    taskId: number;
    handleUpdate: (res: ITask) => void;
    flag: boolean;
    pause: boolean;
    isReview: boolean;
    projectId: number;
    priority: number | null;
}

const TaskAction: React.FC<Props> = ({
    access,
    status,
    user,
    taskId,
    handleUpdate,
    flag,
    pause,
    projectId,
    isReview,
}) => {
    const [priorityFocus, setPriorityFocus] = useState(false);
    const auth = useAppSelector((s) => s.authSlice);

    const updatePrio = (value: null | number) => {
        API.TASKS.UPDATE_PRIORITY(
            taskId,
            value === 1 || value === 2 || value === 3 ? value : null
        ).then((res) => {
            console.log(res);
            if (res && !res.error) {
                handleUpdate(res.data);
            }
        });
    };

    const proceedTask = () =>
        API.TASKS.PROCEED(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    const flagTask = () =>
        API.TASKS.FLAG_TASK(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    const pauseTask = () =>
        API.TASKS.PAUSE(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    const skipTask = () =>
        API.TASKS.SKIP(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });

    return access !== "None" && status !== "Rollback" && status !== "Done" ? (
        <div className="px-6 mt-6">
            <div className="flex items-center gap-2">
                <div>
                    <HandPointingIcon className="stroke-black" />
                </div>
                <div className="font-bold text-lg">Actions</div>
            </div>
            <div className="flex gap-4 mt-1 flex-wrap">
                {(access === "WorkOnAndManage" || access == "WorkOn") &&
                    !pause &&
                    !flag && (
                        <button
                            onClick={proceedTask}
                            className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-blue-400 bg-blue-500 text-white"
                        >
                            {status === "Backlog" ? (
                                <>
                                    <PlusIcon className="w-6 h-6" />
                                    <div>Add</div>
                                </>
                            ) : status === "To Do" ? (
                                <>
                                    <PlayIcon className="w-6 h-6" />
                                    <div>Start</div>
                                </>
                            ) : (
                                <>
                                    <CheckIcon className="w-6 h-6" />
                                    <div>Complete</div>
                                </>
                            )}
                        </button>
                    )}
                {(access === "WorkOnAndManage" || access == "WorkOn") &&
                    !pause &&
                    !flag &&
                    isReview &&
                    status === "Doing" && (
                        <Link
                            href={{
                                pathname: `/tasks/${projectId}`,
                                query: {
                                    form: "rollback",
                                    taskId: taskId,
                                },
                            }}
                        >
                            <button className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-orange-400 bg-orange-500 text-white">
                                <ArrowPathIcon className="w-6 h-6" />
                                <div>Rollback</div>
                            </button>
                        </Link>
                    )}
                {!flag && user && user.id === auth.id && pause ? (
                    <button
                        onClick={pauseTask}
                        className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-blue-400 bg-blue-500 text-white"
                    >
                        <PlayIcon className="w-6 h-6" />
                        <div>Resume</div>
                    </button>
                ) : !flag && status === "Doing" ? (
                    <button
                        onClick={pauseTask}
                        className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-blue-400 bg-blue-500 text-white"
                    >
                        <PauseIcon className="w-6 h-6" />
                        <div>Pause</div>
                    </button>
                ) : (
                    <></>
                )}
                {!flag &&
                    (access === "Manage" || access === "WorkOnAndManage") && (
                        <Link
                            href={{
                                pathname: `/tasks/${projectId}`,
                                query: {
                                    form: "task-assign",
                                    taskId: taskId,
                                },
                            }}
                        >
                            <button className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-blue-400 bg-blue-500 text-white">
                                <UserIcon className="w-6 h-6" />
                                <div>{user ? "Re-assign" : "Assign"}</div>
                            </button>
                        </Link>
                    )}
                {status !== "Backlog" && (
                    <button
                        onClick={flagTask}
                        className={`flex gap-1 px-3 py-1 rounded border-2 border-solid ${
                            flag
                                ? "border-blue-400 bg-blue-500"
                                : "border-red-400 bg-red-500"
                        } text-white`}
                    >
                        {flag ? (
                            <>
                                <FlagIcon className="w-6 h-6" />
                                <div>Clear Flag</div>
                            </>
                        ) : (
                            <>
                                <FlagIcon className="w-6 h-6" />
                                <div>Flag</div>
                            </>
                        )}
                    </button>
                )}
                {(access === "WorkOnAndManage" || access === "Manage") && (
                    <div
                        onFocusCapture={() => setPriorityFocus(true)}
                        onBlurCapture={() => setPriorityFocus(false)}
                        className="flex justify-end items-center relative"
                    >
                        <button className="flex gap-1 px-3 py-1 rounded border-2 border-solid text-white border-blue-400 bg-blue-500 ">
                            <ExclamationCircleIcon className="w-6 h-6" />
                            <div>Change Priority</div>
                        </button>
                        <AnimatePresence>
                            {priorityFocus && (
                                <motion.div
                                    initial={{
                                        opacity: 0,
                                    }}
                                    animate={{
                                        opacity: 1,
                                    }}
                                    exit={{
                                        opacity: 0,
                                    }}
                                    className="left-full absolute p-2 rounded-md bg-white border-2 border-solid border-slate-100 z-20 flex flex-col gap-2 ml-1"
                                >
                                    <button
                                        className="flex justify-center gap-1 px-3 py-1 rounded border-2 border-solid text-white border-rose-400 bg-rose-500"
                                        onClick={() => updatePrio(1)}
                                    >
                                        High
                                    </button>
                                    <button
                                        className="flex justify-center gap-1 px-3 py-1 rounded border-2 border-solid text-white border-orange-400 bg-orange-500"
                                        onClick={() => updatePrio(2)}
                                    >
                                        Medium
                                    </button>
                                    <button
                                        className="flex justify-center gap-1 px-3 py-1 rounded border-2 border-solid text-white border-sky-400 bg-sky-500"
                                        onClick={() => updatePrio(3)}
                                    >
                                        Low
                                    </button>
                                    <button
                                        className="flex justify-center gap-1 px-3 py-1 rounded border-2 border-solid text-black border-slate-300 bg-white"
                                        onClick={() => updatePrio(null)}
                                    >
                                        None
                                    </button>
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </div>
                )}
                {auth.role === 1 && (
                    <button
                        onClick={skipTask}
                        className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-cyan-400 bg-cyan-500 text-white"
                    >
                        <ForwardIcon className="w-6 h-6" />
                        <div>Skip</div>
                    </button>
                )}
                {auth.role === 1 && (
                    <Link
                        href={{
                            pathname: `/tasks/${projectId}`,
                            query: {
                                taskId: taskId,
                                form: "jump",
                            },
                        }}
                    >
                        <button className="flex gap-1 px-3 py-1 rounded border-2 border-solid border-cyan-400 bg-cyan-500 text-white">
                            <ArrowUturnDownIcon className="w-6 h-6 -scale-x-100" />
                            <div>Jump</div>
                        </button>
                    </Link>
                )}
            </div>
                <JumpForm taskId={taskId} projectId={projectId} />
        </div>
    ) : (
        <></>
    );
};

export default TaskAction;
