import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import API from "../../lib/API";
import { ITask } from ".";
import Loader from "../loader";
import CustomizedCombobox from "../formComponents/Combobox";

interface Props {
    taskId: number;
    onClose: () => void;
    onSuccess: (task: ITask) => void;
}

const FlagTaskModal: React.FC<Props> = ({ taskId, onClose, onSuccess }) => {
    const [teamLeaders, setTeamLeaders] = useState<BasicInfo[]>();
    const [selectedTeamLeader, setSelectedTeamLeader] = useState<BasicInfo>();
    const [comment, setComment] = useState("");
    const [error, setError] = useState<string>();
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        API.RESOURCES.USERS.GET_TEAM_LEADERS().then((res) => {
            if (res && !res.error && res.data) {
                setTeamLeaders(res.data);
                return;
            }
            setTeamLeaders([]);
        });
    }, []);

    const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
        e.preventDefault();
        setError(undefined);

        if (!selectedTeamLeader) {
            setError("Please select a team leader or section head");
            return;
        }

        if (!comment.trim()) {
            setError("Please enter a comment");
            return;
        }

        setSubmitting(true);
        API.TASKS.FLAG_TASK({
            taskId,
            teamLeaderId: selectedTeamLeader.id,
            comment: comment.trim(),
        }).then((res) => {
            setSubmitting(false);
            if (res && !res.error) {
                onSuccess(res.data);
                onClose();
                return;
            }
            setError(res && "message" in res ? res.message : "Failed to flag task");
        });
    };

    if (teamLeaders === undefined) {
        return (
            <div className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0">
                <Loader />
            </div>
        );
    }

    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0 overflow-y-auto"
        >
            <div className="rounded-lg bg-white shadow-md w-full max-w-md mx-4">
                <div className="flex justify-between py-4 px-6 items-center border-b border-solid border-slate-200">
                    <div className="text-xl font-semibold">Flag Task</div>
                    <button type="button" onClick={onClose} className="text-slate-500 hover:text-slate-700">
                        ✕
                    </button>
                </div>
                <form onSubmit={handleSubmit} className="p-6 flex flex-col gap-4">
                    <div>
                        <div className="text-sm text-slate-700 mb-1">Team Leader / Section Head</div>
                        {teamLeaders.length > 0 ? (
                            <CustomizedCombobox
                                value={selectedTeamLeader}
                                onChange={setSelectedTeamLeader}
                                options={teamLeaders}
                            />
                        ) : (
                            <div className="text-sm text-slate-500">
                                No team leaders or section heads available.
                            </div>
                        )}
                    </div>
                    <div>
                        <div className="text-sm text-slate-700 mb-1">Comment</div>
                        <textarea
                            value={comment}
                            onChange={(e) => setComment(e.target.value)}
                            rows={4}
                            placeholder="Describe why this task is being flagged..."
                            className="w-full border border-solid border-slate-300 rounded-lg p-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        />
                    </div>
                    {error && <div className="text-sm text-red-500">{error}</div>}
                    <div className="flex justify-end gap-2 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 rounded border border-solid border-slate-300 text-slate-700"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={submitting || teamLeaders.length === 0}
                            className="px-4 py-2 rounded border-2 border-solid border-red-400 bg-red-500 text-white disabled:opacity-50"
                        >
                            {submitting ? "Flagging..." : "Flag Task"}
                        </button>
                    </div>
                </form>
            </div>
        </motion.div>
    );
};

export default FlagTaskModal;
