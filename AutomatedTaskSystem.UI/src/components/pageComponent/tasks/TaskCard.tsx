import Link from "next/link";
import { useRouter } from "next/router";
import RotatingArrowsIcon from "../../../assets/Icons/RotatingArrows";
import { PauseIcon } from "@heroicons/react/24/solid";

const TaskCard = ({
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
    paused,
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
    paused: boolean;
}) => {
    const router = useRouter();
    const projectId = router.query.projectId;

    return (
        <Link
            href={{
                pathname: `/tasks/${projectId}/board`,
                query: {
                    taskId: id,
                },
            }}
            className="relative"
        >
            <div
                className={[
                    flagged
                        ? "bg-red-300"
                        : attention
                        ? "bg-emerald-200"
                        : "bg-blue-200",
                    "rounded-lg flex flex-col transition ease-in group",
                    flagged
                        ? "border-2 border-solid border-red-200"
                        : attention
                        ? "border-2 border-solid border-emerald-200"
                        : "border-2 border-solid border-blue-200",
                ].join(" ")}
            >
                {isRollback || paused ? (
                    <div
                        className={`flex px-4 pt-4 pb-2 ${
                            paused ? "justify-between" : "justify-end"
                        }`}
                    >
                        {paused && (
                            <div className="w-8 h-8 shrink-0 flex items-center justify-center rounded-full border-black border-solid border-2">
                                <PauseIcon className="w-6 h-6" />
                            </div>
                        )}
                        {isRollback && (
                            <div className="w-8 h-8 shrink-0 flex relative items-center justify-center">
                                <RotatingArrowsIcon className="fill-red-600 w-full h-full" />
                                <div className="absolute text-red-600">
                                    {rollbackCount}
                                </div>
                            </div>
                        )}
                    </div>
                ) : (
                    ""
                )}
                <div
                    className={`px-6 flex flex-col${
                        isRollback || paused ? "" : " pt-4"
                    }`}
                >
                    <div className="text-lg text-black group-hover:text-2xl group-hover:mt-1 mt-0 transition-all ease-out">
                        <div className="whitespace-normal">{lo}</div>
                    </div>
                    <div
                        className={`flex ${
                            priority !== 0 ? "justify-between" : "justify-end"
                        } mt-2`}
                    >
                        {priority !== 0 && (
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
                    className={`bg-white mt-4 px-6 flex flex-col gap-4 py-4 rounded-b-md`}
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

export default TaskCard;
