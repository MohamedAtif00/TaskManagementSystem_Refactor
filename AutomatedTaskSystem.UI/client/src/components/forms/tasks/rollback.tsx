import { useEffect, useState } from "react";
import API from "../../../lib/API";
import { ITask } from "../../taskDetails";
import { motion } from "framer-motion";
import Head from "next/head";
import Loader from "../../loader";
import CrossIcon from "../../../assets/Icons/Cross";
import Link from "next/link";
import FileUpload from "../../pageComponent/leave/fileUpload";
import useTaskPathHandler from "../../taskDetails/useTaskPathHandler.ts";
import {
	ArrowUturnLeftIcon,
	CheckIcon,
	ExclamationTriangleIcon,
	PaperClipIcon,
} from "@heroicons/react/24/outline";

const ATTACHMENT_ACCEPT = ".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx";
const ATTACHMENT_MAX_SIZE = 10 * 1024 * 1024;

interface Props {
	update: (params: ITask) => void;
	taskId: number;
	type: string;
}

interface Log {
	step: BasicInfo;
	note: string;
	isSelected: boolean;
}

const RollbackForm = ({ taskId, update, type }: Props) => {
	const [logs, setLogs] = useState<Log[]>([]);
	const [step, setStep] = useState<BasicInfo>();
	const [clarification, setClarification] = useState("");
	const [attachments, setAttachments] = useState<File[]>([]);
	const [rollbackPoints, setRollbackPoints] = useState<BasicInfo[]>();
	const [review, setReview] = useState(false);
	const [error, setError] = useState<string>();
	const [submitting, setSubmitting] = useState(false);
	const pathHandler = useTaskPathHandler({ type: type });
	const selectedLogs = logs.filter((l) => l.isSelected);
	const closeHref = {
		pathname: pathHandler(),
		query: { taskId },
	};

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
			setRollbackPoints([]);
			setLogs([]);
		});
	}, [taskId]);

	if (rollbackPoints === undefined)
		return (
			<div className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0">
				<Head>
					<title>TMS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
		e.preventDefault();

		if (step === undefined)
			return setError("Choose the step this task should go back to");
		if (!clarification.trim())
			return setError("Please explain why you are rolling this back");
		if (selectedLogs.length === 0)
			return setError("Select at least one step where the problem started");

		if (!review) {
			setError(undefined);
			return setReview(true);
		}

		setSubmitting(true);
		API.TASKS.ROLLBACK({
			taskId,
			stepId: step.id,
			logs: selectedLogs.map((l) => ({
				note: l.note,
				stepId: l.step.id,
			})),
			clarification: clarification.trim(),
			attachments,
		}).then((res) => {
			if (res && !res.error) return update(res.data);
			setSubmitting(false);
			setReview(false);
			setError(
				res && res.message ? res.message : "Could not roll back the task"
			);
		});
	};

	const toggleLog = (log: Log) => {
		setError(undefined);
		setLogs((ps) =>
			ps.map((element) => ({
				isSelected:
					element.step.id === log.step.id
						? !log.isSelected
						: element.isSelected,
				note: element.note,
				step: element.step,
			}))
		);
	};

	const updateLogNote = (str: string, log: Log) =>
		setLogs((ps) =>
			ps.map((element) => ({
				isSelected: element.isSelected,
				note: element.step.id === log.step.id ? str : element.note,
				step: element.step,
			}))
		);

	return (
		<motion.div
			initial={{ opacity: 0 }}
			animate={{ opacity: 1 }}
			className="fixed z-50 flex justify-center items-center bg-black/40 top-0 left-0 bottom-0 right-0 overflow-y-auto p-4"
		>
			<div className="rounded-xl bg-white shadow-xl w-full max-w-2xl my-6">
				<div className="flex justify-between py-4 px-6 items-start border-b border-solid border-slate-200">
					<div className="flex items-start gap-3">
						<div className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-orange-100 text-orange-600">
							<ArrowUturnLeftIcon className="h-5 w-5" />
						</div>
						<div>
							<div className="text-xl font-semibold text-slate-900">
								{review ? "Confirm rollback" : "Roll back this task"}
							</div>
							<div className="text-sm text-slate-500 mt-0.5">
								{review
									? "Double-check the details. This sends the task back and cannot be undone from here."
									: "Send the work back to an earlier step so it can be fixed."}
							</div>
						</div>
					</div>
					<Link href={closeHref} className="p-1 rounded hover:bg-slate-100">
						<button type="button" aria-label="Close">
							<CrossIcon className="stroke-slate-500" />
						</button>
					</Link>
				</div>
				<form className="overflow-hidden" onSubmit={handleSubmit}>
					{rollbackPoints.length === 0 ? (
						<div className="px-6 py-10 text-center">
							<p className="text-slate-700 font-medium">
								No earlier steps are available
							</p>
							<p className="text-sm text-slate-500 mt-1">
								This task cannot be rolled back because there are no previous
								steps to send it to.
							</p>
						</div>
					) : review ? (
						<div className="px-6 py-5 flex flex-col gap-4">
							<div className="flex items-start gap-2 rounded-lg border border-solid border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
								<ExclamationTriangleIcon className="h-5 w-5 shrink-0 mt-0.5" />
								<span>
									The task will return to{" "}
									<strong>{step?.name}</strong> and people on later steps
									will need to redo their work.
								</span>
							</div>
							<div className="grid gap-3 text-sm">
								<div className="rounded-lg border border-solid border-slate-200 p-3">
									<div className="text-xs uppercase tracking-wide text-slate-500">
										Send back to
									</div>
									<div className="mt-1 font-semibold text-slate-900">
										{step?.name}
									</div>
								</div>
								<div className="rounded-lg border border-solid border-slate-200 p-3">
									<div className="text-xs uppercase tracking-wide text-slate-500">
										Reason
									</div>
									<div className="mt-1 text-slate-800 whitespace-pre-wrap">
										{clarification.trim()}
									</div>
								</div>
								<div className="rounded-lg border border-solid border-slate-200 p-3">
									<div className="text-xs uppercase tracking-wide text-slate-500">
										Problem started at
									</div>
									<div className="mt-2 flex flex-col gap-2">
										{selectedLogs.map((l) => (
											<div key={l.step.id}>
												<div className="font-medium text-slate-900">
													{l.step.name}
												</div>
												<div className="text-slate-600">
													{l.note.trim() ? l.note : "No extra note"}
												</div>
											</div>
										))}
									</div>
								</div>
								<div className="rounded-lg border border-solid border-slate-200 p-3">
									<div className="text-xs uppercase tracking-wide text-slate-500">
										Attachments
									</div>
									{attachments.length === 0 ? (
										<div className="mt-1 text-slate-500">None</div>
									) : (
										<div className="mt-2 flex flex-col gap-1">
											{attachments.map((f) => (
												<div
													key={f.name}
													className="flex items-center gap-1.5 text-slate-800"
												>
													<PaperClipIcon className="h-4 w-4 text-slate-400 shrink-0" />
													<span className="truncate">{f.name}</span>
												</div>
											))}
										</div>
									)}
								</div>
							</div>
						</div>
					) : (
						<div className="px-6 py-5 flex flex-col gap-5 max-h-[70vh] overflow-y-auto">
							<section>
								<div className="flex items-baseline justify-between gap-2">
									<label className="text-sm font-medium text-slate-800">
										1. Send back to{" "}
										<span className="text-red-600">*</span>
									</label>
								</div>
								<p className="text-xs text-slate-500 mt-0.5 mb-2">
									Choose the earlier step this task should return to.
								</p>
								<div className="flex flex-wrap gap-2">
									{rollbackPoints.map((point) => {
										const selected = step?.id === point.id;
										return (
											<button
												key={point.id}
												type="button"
												onClick={() => {
													setError(undefined);
													setStep(point);
												}}
												className={`inline-flex items-center gap-1.5 rounded-lg border-2 border-solid px-3 py-2 text-sm transition-colors ${
													selected
														? "border-orange-400 bg-orange-50 text-orange-800 font-medium"
														: "border-slate-200 bg-white text-slate-700 hover:border-slate-300"
												}`}
											>
												{selected && (
													<CheckIcon className="h-4 w-4" />
												)}
												{point.name}
											</button>
										);
									})}
								</div>
							</section>

							<section>
								<label htmlFor="rollback-reason" className="text-sm font-medium text-slate-800">
									2. Why are you rolling this back?{" "}
									<span className="text-red-600">*</span>
								</label>
								<p className="text-xs text-slate-500 mt-0.5 mb-2">
									A short explanation helps the next person know what to fix.
								</p>
								<textarea
									id="rollback-reason"
									required
									rows={3}
									placeholder="e.g. The source files were incomplete, so production needs to redo this."
									className="w-full outline-none py-2 px-3 text-sm border-solid border border-slate-300 rounded-lg bg-white text-left resize-y min-h-[4.5rem] focus:ring-2 focus:ring-orange-300 focus:border-orange-400"
									value={clarification}
									onChange={(e) => {
										setError(undefined);
										setClarification(e.target.value);
									}}
								/>
							</section>

							<section>
								<div className="text-sm font-medium text-slate-800">
									3. Where did the problem start?{" "}
									<span className="text-red-600">*</span>
								</div>
								<p className="text-xs text-slate-500 mt-0.5 mb-2">
									Select every step that needs attention. Notes are optional.
								</p>
								<div className="flex flex-col gap-2">
									{logs.map((l) => {
										const selected = l.isSelected;
										return (
											<label
												key={l.step.id}
												className={`flex items-start gap-3 rounded-lg border-2 border-solid p-3 cursor-pointer transition-colors ${
													selected
														? "border-orange-300 bg-orange-50"
														: "border-slate-200 bg-white hover:border-slate-300"
												}`}
											>
												<input
													type="checkbox"
													checked={selected}
													onChange={() => toggleLog(l)}
													className="mt-1 h-4 w-4 accent-orange-500"
												/>
												<div className="grow min-w-0">
													<div className="text-sm font-medium text-slate-800">
														{l.step.name}
													</div>
													{selected && (
														<input
															className="mt-2 outline-none w-full py-2 px-3 text-sm border-solid border border-slate-300 rounded-lg bg-white text-left focus:ring-2 focus:ring-orange-300"
															type="text"
															placeholder="Optional note for this step"
															value={l.note}
															onClick={(e) => e.stopPropagation()}
															onChange={(e) =>
																updateLogNote(e.target.value, l)
															}
														/>
													)}
												</div>
											</label>
										);
									})}
								</div>
							</section>

							<section>
								<div className="text-sm font-medium text-slate-800">
									4. Attachments{" "}
									<span className="text-slate-400 font-normal">(optional)</span>
								</div>
								<p className="text-xs text-slate-500 mt-0.5 mb-2">
									Word, PDF, Excel, or PowerPoint — up to 10MB each.
								</p>
								<FileUpload
									multiple
									accept={ATTACHMENT_ACCEPT}
									maxSize={ATTACHMENT_MAX_SIZE}
									label="Select files"
									currentFiles={attachments}
									onMultipleFileChange={setAttachments}
									onFileChange={() => {}}
								/>
							</section>
						</div>
					)}

					{error && (
						<div className="mx-6 mb-2 rounded-lg bg-red-50 border border-solid border-red-200 px-3 py-2 text-sm text-red-700">
							{error}
						</div>
					)}

					<div className="w-full flex justify-end items-center gap-2 border-t border-solid border-slate-200 py-4 px-6">
						{rollbackPoints.length === 0 ? (
							<Link href={closeHref}>
								<button
									type="button"
									className="px-4 py-2 rounded-lg border border-solid border-slate-300 text-slate-700 hover:bg-slate-50"
								>
									Close
								</button>
							</Link>
						) : (
							<>
								{review ? (
									<button
										onClick={() => {
											setReview(false);
											setError(undefined);
										}}
										type="button"
										className="px-4 py-2 rounded-lg border border-solid border-slate-300 text-slate-700 hover:bg-slate-50"
									>
										Back
									</button>
								) : (
									<Link href={closeHref}>
										<button
											type="button"
											className="px-4 py-2 rounded-lg border border-solid border-slate-300 text-slate-700 hover:bg-slate-50"
										>
											Cancel
										</button>
									</Link>
								)}
								<button
									className="px-4 py-2 rounded-lg bg-orange-500 hover:bg-orange-600 text-white font-medium disabled:opacity-50"
									type="submit"
									disabled={submitting}
								>
									{submitting
										? "Rolling back..."
										: review
											? "Confirm rollback"
											: "Review"}
								</button>
							</>
						)}
					</div>
				</form>
			</div>
		</motion.div>
	);
};

export default RollbackForm;
