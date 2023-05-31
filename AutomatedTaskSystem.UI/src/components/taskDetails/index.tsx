import Link from "next/link";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppSelector } from "../../app/hooks";
import HandPointingIcon from "../../assets/Icons/HandPointing";
import TaskIcon from "../../assets/Icons/Task";
import API from "../../lib/API";
import QueryButton from "../button/queryButton";
import ExpansionPanel from "../expansionPanel";
import Backdrop from "../forms/backdrop";
import RollbackForm from "../forms/tasks/rollback";
import PriorityDropDown from "../formComponents/PriorityDropDown";

const dateHandler = (params: string) => {
	const date = new Date(params);

	const WeekDays = new Map();

	const MonthNames = new Map<number, string>();

	MonthNames.set(0, "Jan");
	MonthNames.set(1, "Feb");
	MonthNames.set(2, "Mar");
	MonthNames.set(3, "Apr");
	MonthNames.set(4, "May");
	MonthNames.set(5, "Jun");
	MonthNames.set(6, "Jul");
	MonthNames.set(7, "Aug");
	MonthNames.set(8, "Sep");
	MonthNames.set(9, "Oct");
	MonthNames.set(10, "Nov");
	MonthNames.set(11, "Dec");

	WeekDays.set(0, "Sun");
	WeekDays.set(1, "Mon");
	WeekDays.set(2, "Tue");
	WeekDays.set(3, "Wed");
	WeekDays.set(4, "Thu");
	WeekDays.set(5, "Fri");
	WeekDays.set(6, "Sat");

	const TimeDiff = Date.now() - date.getTime();

	let time: string;

	const h = Math.floor(TimeDiff / 3600000);
	const d = Math.floor(TimeDiff / 86400000);

	if (h < 1 && h >= 0) {
		time = `${Math.floor(TimeDiff / 60000)} Minutes ago`;
	} else if (d === 0) {
		time = `${Math.floor(h)} Hour${h > 1 ? "s" : ""} Ago`;
	} else if (d > 0 && d < 2) {
		time = `Yeseterday, ${
			date.getHours() % 12 < 10 ? "0" : ""
		}${date.getHours()}:${
			date.getMinutes() < 10 ? "0" : ""
		}${date.getMinutes()}`;
	} else if (d > 0 && d < 7) {
		time = `${WeekDays.get(date.getDay())}, ${
			date.getHours() % 12 < 10 ? "0" : ""
		}${date.getHours()}:${
			date.getMinutes() < 10 ? "0" : ""
		}${date.getMinutes()}`;
	} else if (d > 0 && d < 365) {
		time = `${MonthNames.get(date.getMonth())}, ${date.getDate()}`;
	} else {
		time = `${MonthNames.get(
			date.getMonth()
		)}, ${date.getDate()}, ${date.getFullYear()}`;
	}

	return time;
};

export interface ITask {
	pause: boolean;
	error: false;
	id: number;
	name: string;
	learningObjective: { id: number; name: string };
	tag: string;
	template: string;
	environment: string;
	schema: { id: number; name: string };
	isReview: boolean;
	status: "Backlog" | "To Do" | "Doing" | "Done" | "Rollback";
	flagged: boolean;
	comments: {
		user: {
			id: number;
			name: string;
		};
		id: number;
		content: string;
		timestamp: string;
	}[];
	startedAt?: string;
	doneAt?: string;
	priority?: number;
}

interface Props {
	projectId: number;
	refreshTasks: () => void;
}

const TaskDetails = ({ projectId, refreshTasks }: Props) => {
	const [task, setTask] = useState<ITask>();
	const [prio, setPrio] = useState<number | null>(null);
	const router = useRouter();
	const auth = useAppSelector((s) => s.authSlice);

	useEffect(() => {
		const id = router.query.taskId;
		if (id)
			API.TASKS.GET_ONE(id).then((res) => {
				if (res) {
					setTask(res);
					setPrio(res.priority ? res.priority : null);
				}
			});
		else {
			setTask(undefined);
			setPrio(null);
		}
	}, [setTask, router.query.taskId]);

	if (task === undefined) return <></>;

	const mainAction = () => {
		switch (task.status) {
			case "Backlog":
				return API.TASKS.ADD_TODO(task.id).then((res) => {
					if (res) {
						refreshTasks();
						if (!res.error) setTask(res);
					}
				});
			case "To Do":
				return API.TASKS.DOING(task.id).then((res) => {
					if (res) {
						refreshTasks();
						if (!res.error) setTask(res);
					}
				});
			case "Doing": {
				if (task.isReview)
					return API.TASKS.APPROVE(task.id).then((res) => {
						if (res) {
							refreshTasks();
							if (!res.error) {
								setTask(res);
							}
						}
					});
				return API.TASKS.COMPLETE(task.id).then((res) => {
					if (res) {
						refreshTasks();
						if (!res.error) setTask(res);
					}
				});
			}
		}
	};
	const flagTask = () => {
		API.TASKS.FLAG_TASK(task.id).then((res) => {
			if (res) {
				if (!res.error) setTask(res);
				refreshTasks();
			}
		});
	};
	const unflagTask = () => {
		API.TASKS.UNFLAG_TASK(task.id).then((res) => {
			if (res) {
				if (!res.error) setTask(res);
				refreshTasks();
			}
		});
	};
	const pauseTask = () => {
		API.TASKS.PAUSE(task.id).then((res) => {
			if (res) {
				if (!res.error) setTask(res);
				refreshTasks();
			}
		});
	};
	const unpauseTask = () => {
		API.TASKS.UNPAUSE(task.id).then((res) => {
			if (res) {
				if (!res.error) setTask(res);
				refreshTasks();
			}
		});
	};
	const updatePrio = (value: null | number) => {
		setPrio(value);
		API.TASKS.UPDATE_PRIORITY(
			task.id,
			value === 1 || value === 2 || value === 3 ? value : null
		).then((res) => {
			if (res && !res.error) {
				setTask(res.data);
				refreshTasks();
			}
		});
	};

	return (
		<>
			<Backdrop mainRoute={`/tasks/${projectId}`}>
				<div className="bg-white px-8 py-8 rounded-lg cursor-default w-[35rem] max-h-[35rem]">
					<div className="text-slate-500 flex justify-between">
						<div>{task.learningObjective.name}</div>
						<div>{task.schema.name}</div>
					</div>
					<div className="flex justify-between">
						{task.startedAt ? (
							<div className="text-sm text-slate-600">
								Start Date: {dateHandler(task.startedAt)}
							</div>
						) : (
							""
						)}
						{task.doneAt ? (
							<div className="text-sm text-slate-600">
								End Date: {dateHandler(task.doneAt)}
							</div>
						) : (
							""
						)}
					</div>
					<div className="select-none mt-2 flex justify-start items-center gap-4">
						<h1 className="text-xl flex items-center gap-2">
							<TaskIcon color="black" />
							<div>
								<div className="text-lg max-w-[10rem]">
									{task.name}
								</div>
							</div>
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
					{task.priority && (
						<div className="flex justify-end">
							<div className="flex gap-2 items-center">
								<div className="text-sm">Priority:</div>
								{task.priority === 1 ? (
									<div className="rounded-full border-2 border-solid border-white border-opacity-30 py-1 px-4 text-lg font-bold text-white bg-red-600">
										High
									</div>
								) : task.priority === 2 ? (
									<div className="rounded-full border-2 border-solid border-white border-opacity-30 py-1 px-4 text-lg font-bold text-white bg-yellow-600">
										Medium
									</div>
								) : task.priority === 3 ? (
									<div className="rounded-full border-2 border-solid border-white border-opacity-30 py-1 px-4 text-lg font-bold text-white bg-blue-600">
										Low
									</div>
								) : (
									""
								)}
							</div>
						</div>
					)}
					<div>
						<h1 className="text-xl mt-4 flex items-center gap-2">
							<HandPointingIcon className="stroke-black" />
							<div>Actions</div>
						</h1>
						<div className="flex gap-2">
							{!task.pause && task.status === "Doing" ? (
								<button
									className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1"
									onClick={pauseTask}
								>
									Pause
								</button>
							) : (
								<></>
							)}
							{task.flagged ? (
								<button
									className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1"
									onClick={unflagTask}
								>
									Clear Flag
								</button>
							) : task.pause ? (
								<>
									<button
										className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1"
										onClick={unpauseTask}
									>
										Resume
									</button>
								</>
							) : (
								<>
									{task.status === "Doing" &&
									task.isReview ? (
										<>
											<Link
												href={`/tasks/${router.query.projectId}?form=rollback&taskId=${task.id}`}
											>
												<button className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1">
													Rollback
												</button>
											</Link>
											<button
												className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1"
												onClick={mainAction}
											>
												Approve
											</button>
										</>
									) : (
										task.status !== "Done" &&
										task.status !== "Rollback" && (
											<button
												className="px-3 rounded bg-blue-500 text-white flex items-center justify-center py-1"
												onClick={mainAction}
											>
												{task.status === "Backlog"
													? "Add"
													: task.status === "To Do"
													? "Start"
													: "Complete"}
											</button>
										)
									)}
									{task.status !== "Done" &&
									task.status !== "Rollback" ? (
										<>
											{auth.role == 1 ||
											auth.role == 2 ||
											auth.role == 3 ? (
												<>
													<QueryButton
														text="Re-assign"
														url={{
															pathname: `/tasks/${router.query.projectId}`,
															query: {
																form: "task-assign",
																taskId: task.id,
															},
														}}
													/>
												</>
											) : (
												""
											)}
										</>
									) : (
										""
									)}
									{task.status !== "Backlog" &&
										task.status !== "Done" &&
										task.status !== "Rollback" && (
											<button
												className="px-3 rounded bg-red-600 text-white flex items-center justify-center py-1"
												onClick={flagTask}
											>
												Flag
											</button>
										)}
								</>
							)}
						</div>
					</div>
					<div className="flex items-center justify-end">
						<div className="min-w-[10rem]">
							<PriorityDropDown
								value={
									prio === 1
										? { id: 1, name: "High" }
										: prio === 2
										? { id: 2, name: "Medium" }
										: prio === 3
										? { id: 3, name: "Low" }
										: { id: 4, name: "None" }
								}
								handleChange={(e) => {
									updatePrio(e.id);
								}}
							/>
						</div>
					</div>
					<ExpansionPanel header="Template" content={task.template} />
					<ExpansionPanel header="Tag" content={task.tag} />
					<ExpansionPanel
						header="Environment"
						content={task.environment}
					/>
				</div>
			</Backdrop>
			{task.isReview ? (
				<RollbackForm
					name={task.name}
					update={(res) => {
						setTask(res);
						refreshTasks();
						router.back();
					}}
				/>
			) : (
				""
			)}
		</>
	);
};

export default TaskDetails;
