import { ChatBubbleBottomCenterIcon } from "@heroicons/react/24/outline";
import { SetStateAction } from "react";
import { IComment, ITask } from "..";
import AddCommentForm from "./AddCommentForm";
import dateHandler from "../../../lib/DateHandler";
import { useAppSelector } from "../../../app/hooks";
import Link from "next/link";
import useTaskPathHandler from "../useTaskPathHandler.ts";

interface Props {
    taskId: number;
    updateTask: (value: SetStateAction<ITask | undefined>) => void;
    comments: IComment[];
    reload: () => void;
}

interface CommentProps {
    id: number;
    content: string;
    user: BasicInfo;
    date: string;
    isEdited: boolean;
    isDeleted: boolean;
    taskId: number;
}

const Comment: React.FC<CommentProps> = ({
    content,
    user,
    date,
    isEdited,
    isDeleted,
    taskId,
    id,
}) => {
    const auth = useAppSelector((u) => u.authSlice);
    const formatted = dateHandler(date);
    const pathHandler = useTaskPathHandler();

    return (
        <div
            className={`flex flex-col gap-2 items-start justify-center border-solid border rounded-md border-slate-200 p-2${
                isDeleted ? " opacity-60" : ""
            }`}
        >
            <div className="flex gap-4 items-center">
                <div className="p-4 border border-solid border-slate-50 rounded-full bg-slate-100"></div>
                <div className="text-lg font-medium">{user.name}</div>
                <div className="flex gap-2">
                    <div className="opacity-50 ">
                        {formatted.date} - {formatted.hours}:{formatted.minutes}{" "}
                        {formatted.con}
                    </div>
                    {isEdited && (
                        <div className="italic opacity-50">(Edited)</div>
                    )}
                    {user.id === auth.id && !isDeleted && (
                        <Link
                            className="text-blue-600 hover:underline"
                            href={{
                                pathname: pathHandler(),
                                query: {
                                    taskId,
                                    form: "edit-comment",
                                    commentId: id,
                                },
                            }}
                        >
                            Edit
                        </Link>
                    )}
                    {(user.id === auth.id || auth.role === 0) && !isDeleted && (
                        <Link
                            className="text-rose-600 hover:underline"
                            href={{
                                pathname: pathHandler(),
                                query: {
                                    taskId,
                                    form: "delete-comment",
                                    commentId: id,
                                },
                            }}
                        >
                            Delete
                        </Link>
                    )}
                </div>
            </div>
            {isDeleted ? (
                <p className="pl-12 text-red-300">Deleted Comment</p>
            ) : (
                <p className="pl-12">{content}</p>
            )}
        </div>
    );
};

const TaskComments: React.FC<Props> = (props) => {
    return (
        <div className="px-6 flex flex-col gap-2 mt-6 grow overflow-y-auto min-h-[10rem] shrink pb-4 relative">
            <div className="text-xl flex gap-2 top-0 bg-white sticky pb-2 z-10">
                <ChatBubbleBottomCenterIcon className="w-7 h-7 stroke-black" />
                <div className="font-bold">Comments</div>
            </div>
            <AddCommentForm
                reload={props.reload}
                updateTask={props.updateTask}
                taskId={props.taskId}
            />
            {props.comments.map((c) => (
                <Comment
                    id={c.id}
                    key={c.id}
                    taskId={props.taskId}
                    date={c.timestamp}
                    content={c.content}
                    user={c.user}
                    isEdited={c.isEdited}
                    isDeleted={c.isDeleted}
                />
            ))}
        </div>
    );
};

export default TaskComments;
