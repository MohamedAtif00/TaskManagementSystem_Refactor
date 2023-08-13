import HandPointingIcon from "../../assets/Icons/HandPointing";
import { BasicInfo } from "../../lib/API";
import {
    PlusIcon,
    PauseIcon,
    FlagIcon,
    UserIcon,
} from "@heroicons/react/24/outline";
import { PlayIcon, CheckIcon, ArrowPathIcon } from "@heroicons/react/24/solid";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { ITask } from ".";
import Link from "next/link";

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
    const auth = useAppSelector((s) => s.authSlice);

    const proceedTask = () => {
        API.TASKS.PROCEED(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    };
    const flagTask = () => {
        API.TASKS.FLAG_TASK(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    };
    const pauseTask = () => {
        API.TASKS.PAUSE(taskId).then((res) => {
            if (res && !res.error) handleUpdate(res.data);
        });
    };

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
            </div>
        </div>
    ) : (
        <></>
    );
};

export default TaskAction;
