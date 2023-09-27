import { ChatBubbleBottomCenterIcon } from "@heroicons/react/24/outline";
import { SetStateAction } from "react";
import { IComment, ITask } from ".";
import AddCommentForm from "./AddCommentForm";
import dateHandler from "../../lib/DateHandler";

interface Props {
	taskId: number
    updateTask: (value: SetStateAction<ITask | undefined>) => void;
    comments: IComment[];
	reload: () => void;
}

interface CommentProps {
    content: string;
    user: string;
    date: string;
}

const Comment: React.FC<CommentProps> = ({ content, user, date }) => {
    const formatted = dateHandler(date);

    return (
        <div className="flex flex-col gap-2 items-start justify-center border-solid border rounded-md border-slate-200 p-2">
            <div className="flex gap-4 items-center">
                <div className="p-4 border border-solid border-slate-50 rounded-full bg-slate-100"></div>
                <div className="text-lg font-medium">{user}</div>
                <div className="opacity-50">
                    {formatted.date} - {formatted.hours}:{formatted.minutes}
                    {formatted.con}
                </div>
            </div>
            <p>{content}</p>
        </div>
    );
};

const TaskComments: React.FC<Props> = (props) => {
    const comments: JSX.Element[] = [];

    props.comments.forEach((c) => {
        comments.push(<div key={`${c.id}-hr`}></div>);
        comments.push(
            <Comment
                key={c.id}
                date={c.timestamp}
                content={c.content}
                user={c.user.name}
            />
        );
    });

    return (
        <div className="px-6 flex flex-col gap-2 mt-6 grow overflow-y-auto min-h-[10rem] shrink pb-4 relative">
            <div className="text-xl flex gap-2 top-0 bg-white sticky pb-2 z-10">
                <ChatBubbleBottomCenterIcon className="w-7 h-7 stroke-black" />
                <div className="font-bold">Comments</div>
            </div>
            <AddCommentForm reload={props.reload} updateTask={props.updateTask} taskId={props.taskId} />
            {comments}
        </div>
    );
};

export default TaskComments;
