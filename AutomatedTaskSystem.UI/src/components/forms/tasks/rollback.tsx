import { useEffect, useState } from "react";
import API from "../../../lib/API";
import { ITask } from "../../taskDetails";
import { motion } from "framer-motion";
import Head from "next/head";
import Loader from "../../loader";
import CrossIcon from "../../../assets/Icons/Cross";
import Link from "next/link";
import CustomizedCombobox from "../../formComponents/Combobox";
import useTaskPathHandler from "../../taskDetails/useTaskPathHandler.ts";

interface Props {
	update: (params: ITask) => void;
	taskId: number;
}

interface Log {
	step: BasicInfo;
	note: string;
	isSelected: boolean;
}

const RollbackForm = ({ taskId, update }: Props) => {
	const [logs, setLogs] = useState<Log[]>([]);
	const [step, setStep] = useState<BasicInfo>();
	const [clarification, setClarification] = useState<string>();
	const [rollbackPoints, setRollbackPoints] = useState<BasicInfo[]>();
	const [review, setReview] = useState(false);
	const [error, setError] = useState<string>();
	const pathHandler = useTaskPathHandler();

	useEffect(() => {
		API.TASKS.PREVIOUS_TASKS(taskId).then((res) => {
			if (res) {
				setRollbackPoints(res);
				setLogs(
					res.map((s) => ({
						isSelected: false,
						note: "",
						step: s,
					}))
				);
				return;
			}
			setRollbackPoints(undefined);
			setLogs([]);
		});
	}, [taskId]);

	if (rollbackPoints === undefined)
		return (
			<div className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0">
				<Head>
					<title>ATS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
		e.preventDefault();

		if (step === undefined) return setError("Please select a valid step");

		if (!review) return setReview(true);

		API.TASKS.ROLLBACK({
			taskId,
			stepId: step.id,
			logs: logs
				.filter((l) => l.isSelected)
				.map((l) => ({
					note: l.note,
					stepId: l.step.id,
				})),
			clarification,
		}).then((res) => {
			res && !res.error && update(res.data);
		});
	};

	const toggleLog = (log: Log) =>
		setLogs((ps) => {
			const newState: Log[] = ps.map((element) => ({
				isSelected:
					element.step.id === log.step.id
						? !log.isSelected
						: element.isSelected,
				note: element.note,
				step: element.step,
			}));
			return newState;
		});

	const updateLogNote = (str: string, log: Log) =>
		setLogs((ps) => {
			const newState: Log[] = ps.map((element) => ({
				isSelected: element.isSelected,
				note: element.step.id === log.step.id ? str : element.note,
				step: element.step,
			}));
			return newState;
		});

	return (
		<motion.div
			initial={{ opacity: 0 }}
			animate={{ opacity: 1 }}
			className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0 overflow-y-auto"
		>
			<div className="rounded-lg bg-white shadow-md">
				<div className="flex justify-between py-4 px-6 items-center border-b border-solid border-slate-200">
					<div className="text-xl">Roll Back Task</div>
					<Link
						href={{
							pathname: pathHandler(),
							query: {
								taskId: taskId,
							},
						}}
					>
						<button>
							<CrossIcon className="stroke-black" />
						</button>
					</Link>
				</div>
				<form className="overflow-hidden" onSubmit={handleSubmit}>
					{review ? (
						<div className="flex flex-col gap-2">
							<div className="px-4 pt-3 flex gap-4 items-center">
								<div>
									<span className="font-bold">
										Rolling back
									</span>{" "}
									to:
								</div>
								<div className="p-2 text-lg font-bold border border-solid border-orange-300 rounded-md text-white bg-orange-500">
									{step?.name}
								</div>
							</div>
							<div className="px-4">
								<div className="text-sm text-slate-700">
									Clarification:
								</div>
								<div>
									{clarification ? clarification : "None"}
								</div>
							</div>
							<div className="px-4 flex flex-col gap-2 pb-3 text-sm text-slate-700 max-h-60 overflow-hidden">
								<div>Notes:</div>
								<div className="overflow-y-auto flex flex-col gap-1">
									{logs
										.filter((l) => l.isSelected)
										.map((l) => {
											return (
												<div
													key={l.step.id}
													className="flex gap-1 items-start"
												>
													<div>{l.step.name}:</div>
													<div className="text-black">
														{l.note
															? l.note
															: "None"}
													</div>
												</div>
											);
										})}
								</div>
							</div>
						</div>
					) : (
						<div className="flex items-center overflow-hidden">
							<div className="w-80 py-4 px-6">
								<div className="flex flex-col gap-4">
									<div>
										<div className="text-sm flex justify-between">
											<div>Roll Back to:</div>
											{error && (
												<div className="text-red-600">
													{error}
												</div>
											)}
										</div>
										<CustomizedCombobox
											onChange={(e) => {
												setError(undefined);
												setStep(e);
											}}
											options={rollbackPoints}
											value={step}
										/>
									</div>
									<div>
										<label>
											<div className="text-sm">
												Please try to clarifiy the
												reason:
											</div>
											<input
												type="text"
												className="mt-1 outline-none w-full py-2 px-3 text-sm border-solid border border-slate-300 rounded-lg bg-white text-left"
												value={clarification}
												onChange={(e) => {
													setClarification(
														e.target.value
													);
												}}
											/>
										</label>
									</div>
								</div>
							</div>
							<div className="w-80 py-4 px-6 bg-slate-100 rounded-br-lg max-h-96 overflow-y-auto">
								<div className="text-sm mb-2">Issues:</div>
								<div className="flex gap-2 flex-col">
									{logs.map((l) => {
										return (
											<div
												key={l.step.id}
												className={`flex items-center gap-2 ${
													l.isSelected
														? "opacity-100"
														: "opacity-60"
												}`}
											>
												{l.isSelected ? (
													<div
														onClick={() =>
															toggleLog(l)
														}
														className="pl-3 pb-3 bg-blue-400 border-2 border-solid border-blue-300 rounded-md"
													></div>
												) : (
													<div
														onClick={() =>
															toggleLog(l)
														}
														className="pl-3 pb-3 bg-white/80 border-2 border-solid border-blue-300 rounded-md"
													></div>
												)}
												<div className="grow">
													<div className="text-sm">
														{l.step.name}:
													</div>
													<input
														className="mt-1 outline-none w-full py-2 px-3 text-sm border-solid border border-slate-300 rounded-lg bg-white text-left"
														type="text"
														disabled={!l.isSelected}
														value={l.note}
														onChange={(e) =>
															updateLogNote(
																e.target.value,
																l
															)
														}
													/>
												</div>
											</div>
										);
									})}
								</div>
							</div>
						</div>
					)}
					<div className="w-full flex justify-center items-center gap-8 border-t border-solid border-slate-200 py-4 px-8">
						{review ? (
							<button
								onClick={() => {
									setReview(false);
								}}
								type="button"
								className="px-8 py-2 bg-black text-white border-2 border-solid border-white/50"
							>
								Back
							</button>
						) : (
							<Link
								href={{
									pathname: pathHandler(),
									query: {
										taskId,
									},
								}}
							>
								<button
									type="button"
									className="px-8 py-2 bg-black text-white border-2 border-solid border-white/50"
								>
									Cancel
								</button>
							</Link>
						)}
						<button
							className="px-8 py-2 bg-blue-600 text-white border-2 border-solid border-white/50"
							type="submit"
						>
							{review ? "Submit" : "Review"}
						</button>
					</div>
				</form>
			</div>
		</motion.div>
	);
};

export default RollbackForm;
