import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { ITask } from ".";
import CommentIcon from "../../assets/Icons/Comment";
import TaskIcon from "../../assets/Icons/Task";
import API from "../../lib/API";
import ExpansionPanel from "../expansionPanel";
import Backdrop from "../forms/backdrop";

interface Props {
	projectId: number;
}

const NonActionableTaskDetails = ({ projectId }: Props) => {
	const [task, setTask] = useState<ITask>();
	const router = useRouter();
	const [comment, setComment] = useState("");

	useEffect(() => {
		const id = router.query.taskId;
		if (id)
			API.TASKS.GET_ONE(id).then((res) => {
				if (res) {
					setTask(res);
				}
			});
		else setTask(undefined);
	}, [setTask, router.query.taskId]);

	const postComment = (e: React.FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		task &&
			API.TASKS.COMMENT(task.id, comment).then((res) => {
				if (res) {
					setTask((ps) => {
						if (ps)
							return {
								...ps,
								comments: [...ps!.comments, res],
							};
						return ps;
					});
					setComment("");
				}
			});
	};

	if (task === undefined) return <></>;

	return (
		<Backdrop mainRoute={`/reports/${projectId}`}>
			<div className="bg-white px-8 py-8 rounded-lg cursor-default w-[30rem] max-h-[35rem]">
				<div className="text-slate-500 flex justify-between">
					<div>{task.learningObjective.name}</div>
					<div>{task.schema.name}</div>
				</div>
				<div className="select-none mt-2 flex justify-start items-center gap-4">
					<h1 className="text-xl flex items-center gap-2">
						<TaskIcon color="black" />
						<div>{task.name}</div>
					</h1>
					<div
						className={`px-3 py-1 rounded-3xl ${
							task.status === "Done"
								? "bg-emerald-500 text-white"
								: task.status === "Doing"
								? "bg-orange-500 text-white"
								: task.status === "Rollback"
								? "bg-black text-white"
								: task.status === "To Do"
								? "bg-blue-500 text-white"
								: "border-2 border-black border-solid"
						}`}
					>
						{task.status}
					</div>
				</div>
				<ExpansionPanel header="Template" content={task.template} />
				<ExpansionPanel header="Tag" content={task.tag} />
				<ExpansionPanel
					header="Environment"
					content={task.environment}
				/>
				{/* 
				<div className="select-none mt-2 gap-4">
					<h1 className="text-xl flex items-center gap-2">
						<CommentIcon color="black" />
						<div>Comments</div>
					</h1>
					<form onSubmit={postComment}>
						<div className="flex gap-4">
							<textarea
								className="overflow-hidden resize-none h-8 max-h-16 py-1 px-4 focus:border-blue-200 grow border rounded-lg border-solid border-slate-400"
								onChange={(e) => {
									const element = e.currentTarget;
									element.style.height = "2rem";
									element.style.height =
										element.scrollHeight + "px";
									setComment(e.target.value);
								}}
								value={comment}
							/>
							<button
								className="px-4 py-1 bg-blue-500 text-white rounded-md"
								type="submit"
							>
								Send
							</button>
						</div>
					</form>
				</div>
				*/}
			</div>
		</Backdrop>
	);
};

export default NonActionableTaskDetails;
