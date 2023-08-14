import API from "../../lib/API";
import { ChevronRightIcon } from "@heroicons/react/24/solid";
import { SetStateAction, useRef, useState } from "react";
import { ITask } from ".";

interface Props {
    loId: number;
    updateTask: (value: SetStateAction<ITask | undefined>) => void;
}

const AddCommentForm: React.FC<Props> = ({ loId, updateTask }) => {
    const [inputFocus, setInputFocus] = useState(false);
    const commentRefTextArea = useRef<HTMLTextAreaElement>(null);
    const [value, setValue] = useState("");
    const [submitting, setSubmitting] = useState(false);

    const handleAddComment = (e: React.FormEvent<HTMLFormElement>) => {
        if (submitting) return;
        e.preventDefault();
        setSubmitting(true);
        API.TASKS.COMMENT(loId, value).then((res) => {
            if (res && !res.error) {
                updateTask((ps) => {
                    return {
                        ...ps!,
                        comments: [res.data, ...ps!.comments],
                    };
                });
                setValue("");
                setSubmitting(false);
            }
        });
    };
    return (
        <form onSubmit={handleAddComment}>
            <div
                onClick={() => {
                    commentRefTextArea.current?.focus();
                }}
                className={`p-2 rounded-md flex flex-col gap-1 border-2 border-solid ${
                    inputFocus
                        ? "border-blue-600 shadow-blue-300 shadow-md"
                        : "border-slate-700"
                }`}
            >
                <textarea
                    ref={commentRefTextArea}
                    onFocus={() => setInputFocus(true)}
                    onBlur={() => setInputFocus(false)}
                    value={value}
                    onChange={(e) => setValue(e.target.value)}
                    className="resize-none outline-none"
                ></textarea>
                <div className="pt-[1px] bg-slate-100"></div>
                <div className="flex justify-between mt-2">
                    <div className="opacity-40">Leave a Comment</div>
                    <button
                        type="submit"
                        onClick={(e) => e.stopPropagation()}
                        className={`flex items-center pl-3 pr-1 ${
                            !submitting && value !== ""
                                ? "bg-blue-500"
                                : "bg-slate-400"
                        } text-white py-1 rounded`}
                    >
                        <div>Send</div>
                        <ChevronRightIcon className="w-5 h-5" />
                    </button>
                </div>
            </div>
        </form>
    );
};

export default AddCommentForm;
