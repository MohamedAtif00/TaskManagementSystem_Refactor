import Link from "next/link";
import { IComment } from "..";
import { useState } from "react";
import API from "../../../lib/API";
import { useRouter } from "next/router";

interface Props {
    projectId: number;
    taskId: number;
    comment?: IComment;
    update: () => void;
}

const EditCommentForm: React.FC<Props> = ({
    projectId,
    taskId,
    comment,
    update,
}) => {
    const [content, setContent] = useState(comment ? comment.content : "");
    const router = useRouter();

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
        e.preventDefault();

        comment &&
            content !== comment.content &&
            API.TASKS.EDIT_COMMENT(taskId, comment.id, content).then((res) => {
                if (res && !res.error)
                    update();
                router.push(`/tasks/${projectId}?taskId=${taskId}`);
            });
    };

    return (
        <div
            key="edit-comment-form"
            className="fixed z-50 top-0 left-0 right-0 bottom-0 bg-black/25 flex items-center justify-center"
        >
            <div className="bg-white rounded-lg border-slate-200 border border-solid">
                <h4 className="text-2xl font-bold px-8 pt-4 pb-2 border-slate-200 border-b border-solid mb-2">
                    Edit Comment
                </h4>
                {comment ? (
                    <form onSubmit={handleSubmit}>
                        <div className="px-8">
                            <textarea
                                onChange={(e) => setContent(e.target.value)}
                                className="resize-none outline-none p-2 w-[30rem] rounded-md border-2 border-solid border-slate-700 focus:border-blue-600 focus:shadow-blue-300 focus:shadow-md"
                                value={content}
                            ></textarea>
                        </div>
                        <div className="grid grid-cols-2 px-8 py-4 gap-2">
                            <Link
                                href={{
                                    pathname: `/tasks/${projectId}`,
                                    query: {
                                        taskId: taskId,
                                    },
                                }}
                            >
                                <button
                                    type="button"
                                    className="w-full bg-black text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1"
                                >
                                    Cancel
                                </button>
                            </Link>
                            <button
                                type="submit"
                                className="bg-blue-500 text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1 cursor-pointer"
                            >
                                Confirm
                            </button>
                        </div>
                    </form>
                ) : (
                    <div className="grid grid-cols-1 px-8 py-4 gap-2">
                        <div>Comment is not found</div>
                        <Link
                            href={{
                                pathname: `/tasks/${projectId}`,
                                query: {
                                    taskId: taskId,
                                },
                            }}
                        >
                            <div className="bg-black text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1">
                                Back
                            </div>
                        </Link>
                    </div>
                )}
            </div>
        </div>
    );
};

export default EditCommentForm;
