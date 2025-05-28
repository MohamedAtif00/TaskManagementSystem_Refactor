import Link from "next/link";
import { IComment } from "..";
import API from "../../../lib/API";
import { useRouter } from "next/router";
import useTaskPathHandler from "../useTaskPathHandler.ts";

interface Props {
    taskId: number;
    comment?: IComment;
    update: () => void;
     type:"tasks"|"sprints"
}

const DeleteCommentForm: React.FC<Props> = ({
    taskId,
    comment,
    update,
    type
}) => {
    const router = useRouter();
    const pathHandler = useTaskPathHandler({type});

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
        e.preventDefault();

        comment &&
            API.TASKS.DELETE_COMMENT(taskId, comment.id).then((res) => {
                if (res && !res.error) update();
                router.push(`${pathHandler()}?taskId=${taskId}`);
            });
    };

    return (
        <div
            key="edit-comment-form"
            className="fixed z-50 top-0 left-0 right-0 bottom-0 bg-black/25 flex items-center justify-center"
        >
            <div className="bg-white rounded-lg border-slate-200 border border-solid">
                <h4 className="text-2xl font-bold px-8 pt-4 pb-2 border-slate-200 border-b border-solid mb-2">
                    Delete Comment
                </h4>
                {comment ? (
                    <form onSubmit={handleSubmit}>
                        <div className="px-8">
                            <div className="p-2 w-[30rem] rounded-md border-2 border-solid border-rose-500 text-black/75 text-sm">
                                {comment.content}
                            </div>
                            <p>Are you sure you want to delete comment?</p>
                        </div>
                        <div className="grid grid-cols-2 px-8 py-4 gap-2">
                            <Link
                                href={{
                                    pathname: pathHandler(),
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
                                className="bg-red-500 text-white text-lg font-bold text-center border-2 border-solid border-white/50 py-1 cursor-pointer"
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
                                pathname: pathHandler(),
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

export default DeleteCommentForm;
