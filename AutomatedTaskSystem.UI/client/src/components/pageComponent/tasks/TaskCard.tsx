import Link from "next/link";
import { useRouter } from "next/router";
import RotatingArrowsIcon from "../../../assets/Icons/RotatingArrows"; // Adjust path as needed
import { PauseIcon } from "@heroicons/react/24/solid"; // Adjust path as needed

const TaskCard = ({
    id, // This is the taskId
    name, // This is the task name
    lo, // This is the Learning Objective Name (for display)
    flagged = false,
    attention,
    userName,
    from,
    isRollback,
    rollbackCount,
    priority,
    paused,
    baseDuration,
    duration,
    type // 'tasks' or 'sprints'
}: {
    id: number;
    name: string;
    lo: string; // Learning Objective Name (string, for display)
    flagged: boolean;
    attention: boolean;
    userName?: string;
    from: string;
    isRollback: boolean;
    rollbackCount: number;
    priority: number | null;
    paused: boolean;
    baseDuration?: number;
    duration?: number;
    type: "tasks" | "sprints"
}) => {
    const router = useRouter();

    // Extract relevant IDs directly from the router's query
    const parentId = type === "tasks" ? router.query.projectId : router.query.sprintId;
    const learningObjectId = router.query.learningObjectId; // Get LO ID from the route

    // Ensure parentId and learningObjectId are strings (as router.query can return string | string[])
    const currentParentId = Array.isArray(parentId) ? parentId[0] : parentId;
    const currentLearningObjectId = Array.isArray(learningObjectId) ? learningObjectId[0] : learningObjectId;

    // Construct the href dynamically
    // This assumes your target route for a task detail page looks like:
    // `/[type]/[projectIdOrSprintId]/[learningObjectId]/[taskId]`
    const linkHref = {
        pathname: `/${type}/${currentParentId}/${currentLearningObjectId}/${id}`, // `id` is the taskId
    };

    return (
        <Link
            href={linkHref}
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
                {/* Conditional rendering for rollback and paused states */}
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
                                {/* Assuming RotatingArrowsIcon is an SVG component */}
                                <RotatingArrowsIcon className="fill-red-600 w-full h-full" />
                                <div className="absolute text-red-600 font-bold text-sm">
                                    {rollbackCount}
                                </div>
                            </div>
                        )}
                    </div>
                ) : (
                    ""
                )}

                {/* Conditional rendering for exceeded base duration */}
                {baseDuration !== undefined && baseDuration > 0 && duration !== undefined && duration > baseDuration ? (
                    <div className="text-xs text-red-600 font-medium bg-red-100 px-3 py-1 rounded-md self-start mx-auto -mt-2">
                        Exceeded base duration!
                    </div>
                ) : null}

                <div
                    className={`px-6 flex flex-col${
                        isRollback || paused ? "" : " pt-4"
                    }`}
                >
                    {/* Display Learning Objective name */}
                    <div className="text-lg text-black group-hover:text-2xl group-hover:mt-1 mt-0 transition-all ease-out">
                        <div className="whitespace-normal font-semibold text-gray-800">{lo}</div>
                    </div>

                    {/* Priority and "From" information */}
                    <div
                        className={`flex ${
                            priority !== null && priority !== 0 ? "justify-between" : "justify-end"
                        } mt-2 items-center`}
                    >
                        {priority !== null && priority !== 0 && ( // Ensure priority is not null and not 0 for display
                            <div className="flex flex-col items-start">
                                <div className="text-xs opacity-60 text-gray-500">
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
                                    <div className="font-bold text-gray-500">
                                        N/A
                                    </div>
                                )}
                            </div>
                        )}
                        {from && (
                            <div className="text-right">
                                <div className="text-xs opacity-60 text-orange-600">
                                    From:
                                </div>
                                <div className="text-sm opacity-80 font-medium text-gray-700">{from}</div>
                            </div>
                        )}
                    </div>
                </div>

                {/* Bottom block: Task Name and User Name */}
                <div
                    className={`bg-white mt-4 px-6 flex flex-col gap-4 py-4 rounded-b-md border-t border-gray-100`}
                >
                    <div className="font-medium whitespace-normal text-base">
                        <div className="text-gray-900">{name}</div>
                    </div>
                    {userName ? (
                        <div className="text-slate-600 text-sm">
                            <div>Assigned to: <span className="font-medium">{userName}</span></div>
                        </div>
                    ) : (
                        <div className="text-slate-500 text-sm">
                            No user assigned
                        </div>
                    )}
                </div>
            </div>
        </Link>
    );
};

export default TaskCard;