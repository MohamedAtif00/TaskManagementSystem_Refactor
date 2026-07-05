import { FlagIcon } from "@heroicons/react/24/solid";
import { FlagNotificationData } from "./flagNotificationUtils";

interface Props {
    data: FlagNotificationData;
    createdAt?: string;
    isRead?: boolean;
    compact?: boolean;
}

const FlagNotificationContent: React.FC<Props> = ({
    data,
    createdAt,
    isRead = false,
    compact = false,
}) => {
    const {
        flaggedByUserName = "A user",
        taskName = "a task",
        learningObjectiveName,
        projectName,
        comment,
        canView = false,
    } = data;

    return (
        <div className={`flex gap-3 ${compact ? "" : "w-full"}`}>
            <div
                className={`flex-shrink-0 rounded-full flex items-center justify-center ${
                    compact ? "w-9 h-9" : "w-11 h-11"
                } ${
                    isRead
                        ? "bg-rose-100 border border-rose-300"
                        : "bg-red-100 border-2 border-red-400 shadow-sm"
                }`}
            >
                <FlagIcon
                    className={`${compact ? "w-4 h-4" : "w-5 h-5"} text-red-600`}
                />
            </div>

            <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                    <div className="flex items-center gap-2 flex-wrap">
                        <span
                            className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider ${
                                isRead
                                    ? "bg-rose-100 text-rose-600"
                                    : "bg-red-500 text-white"
                            }`}
                        >
                            Flagged
                        </span>
                        <span
                            className={`text-sm font-semibold ${
                                isRead ? "text-rose-700" : "text-red-700"
                            }`}
                        >
                            Task needs attention
                        </span>
                    </div>
                    {createdAt && (
                        <span
                            className={`text-xs whitespace-nowrap ${
                                isRead ? "text-rose-400" : "text-red-400"
                            }`}
                        >
                            {createdAt}
                        </span>
                    )}
                </div>

                <p
                    className={`mt-1.5 text-sm leading-relaxed ${
                        isRead ? "text-slate-600" : "text-slate-800"
                    }`}
                >
                    <span className="font-semibold text-red-700">
                        {flaggedByUserName}
                    </span>{" "}
                    flagged{" "}
                    <span className="font-semibold">{taskName}</span>
                    {learningObjectiveName && (
                        <>
                            {" "}
                            in LO{" "}
                            <span className="font-medium text-slate-700">
                                {learningObjectiveName}
                            </span>
                        </>
                    )}
                    {projectName && (
                        <>
                            {" "}
                            in{" "}
                            <span className="font-medium text-slate-700">
                                {projectName}
                            </span>
                        </>
                    )}
                </p>

                {comment && (
                    <div
                        className={`mt-2.5 rounded-lg border px-3 py-2 ${
                            isRead
                                ? "bg-rose-50/80 border-rose-200"
                                : "bg-red-50 border-red-200"
                        }`}
                    >
                        <div
                            className={`text-[10px] font-semibold uppercase tracking-wide mb-1 ${
                                isRead ? "text-rose-500" : "text-red-500"
                            }`}
                        >
                            Comment
                        </div>
                        <p
                            className={`text-sm italic leading-relaxed ${
                                isRead ? "text-rose-900/80" : "text-red-900"
                            }`}
                        >
                            &ldquo;{comment}&rdquo;
                        </p>
                    </div>
                )}

                {!compact && canView && (
                    <p
                        className={`mt-2 text-xs font-medium ${
                            isRead ? "text-rose-500" : "text-red-500"
                        }`}
                    >
                        Click to view flagged task →
                    </p>
                )}
            </div>
        </div>
    );
};

export default FlagNotificationContent;
