import Link from "next/link";
import CrossIcon from "../../assets/Icons/Cross";
import { useEffect, useState } from "react";
import API from "../../lib/API";
import Head from "next/head";
import Loader from "../loader";
import useTaskPathHandler from "./useTaskPathHandler.ts";
import { FileText } from "lucide-react";
import {
	ArrowUturnLeftIcon,
	ExclamationCircleIcon,
} from "@heroicons/react/24/outline";

interface Props {
	taskId: number;
	isReview: boolean;
	type: string;
}

interface History {
	rollbacks: {
		id: number;
		clarification?: string;
		task: BasicInfo;
		attachments: {
			id: number;
			fileName: string;
			contentType: string;
			fileSize: number;
		}[];
	}[];
	issues: {
		id: number;
		note?: string;
		task: BasicInfo;
	}[];
}

const RollbackHistory: React.FC<Props> = ({ taskId, isReview, type }) => {
	const [history, setHistory] = useState<History>();
	const pathHandler = useTaskPathHandler({ type: type });

	useEffect(() => {
		API.TASKS.GET_ROLLBACK_HISTORY(taskId).then(
			(res) => res && !res.error && setHistory(res.data)
		);
	}, [taskId]);

	if (history === undefined)
		return (
			<div className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0">
				<Head>
					<title>TMS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	const rollbackLabel = isReview ? "sent back to" : "came from";
	const issueLabel = isReview ? "noted at" : "noted from";

	return (
		<div className="fixed top-0 bottom-0 left-0 right-0 bg-black/40 z-50 flex items-center justify-center p-4">
			<div className="bg-white rounded-xl overflow-hidden w-full max-w-lg shadow-xl">
				<div className="px-6 py-4 border-b border-slate-200 border-solid flex justify-between items-start">
					<div>
						<div className="text-xl font-semibold text-slate-900">
							Rollback history
						</div>
						<div className="text-sm text-slate-500 mt-0.5">
							Earlier send-backs and issues on this task
						</div>
					</div>
					<Link
						href={{
							pathname: pathHandler(),
							query: {
								taskId,
							},
						}}
						className="p-1 rounded hover:bg-slate-100"
						aria-label="Close"
					>
						<CrossIcon className="stroke-slate-500" />
					</Link>
				</div>
				<div className="max-h-[70vh] py-4 overflow-auto">
					<section className="px-6 pb-4 mb-4 border-b border-slate-200 border-solid">
						<div className="flex items-center gap-2 text-slate-800 font-medium">
							<ArrowUturnLeftIcon className="h-4 w-4 text-orange-500" />
							<span>
								{history.rollbacks.length}{" "}
								{history.rollbacks.length === 1 ? "rollback" : "rollbacks"}{" "}
								{rollbackLabel}
							</span>
						</div>
						{history.rollbacks.length === 0 ? (
							<p className="mt-3 text-sm text-slate-500">
								No rollbacks recorded yet.
							</p>
						) : (
							<div className="mt-3 flex flex-col gap-2">
								{history.rollbacks.map((m) => (
									<div
										key={m.id}
										className="rounded-lg border border-solid border-slate-200 bg-slate-50 p-3"
									>
										<div className="text-sm font-medium text-slate-900">
											{m.task.name}
										</div>
										{m.clarification ? (
											<p className="mt-1 text-sm text-slate-600">
												{m.clarification}
											</p>
										) : (
											<p className="mt-1 text-sm text-slate-400 italic">
												No reason given
											</p>
										)}
										{m.attachments?.length > 0 && (
											<div className="mt-2 flex flex-col gap-1">
												{m.attachments.map((a) => (
													<a
														key={a.id}
														href={API.TASKS.GET_ROLLBACK_ATTACHMENT_URL(
															a.id
														)}
														target="_blank"
														rel="noreferrer"
														className="flex items-center gap-1 text-sm text-blue-600 hover:underline"
													>
														<FileText className="h-4 w-4 shrink-0" />
														<span className="truncate">
															{a.fileName}
														</span>
													</a>
												))}
											</div>
										)}
									</div>
								))}
							</div>
						)}
					</section>
					<section className="px-6">
						<div className="flex items-center gap-2 text-slate-800 font-medium">
							<ExclamationCircleIcon className="h-4 w-4 text-amber-500" />
							<span>
								{history.issues.length}{" "}
								{history.issues.length === 1 ? "issue" : "issues"}{" "}
								{issueLabel}
							</span>
						</div>
						{history.issues.length === 0 ? (
							<p className="mt-3 text-sm text-slate-500">
								No issues recorded yet.
							</p>
						) : (
							<div className="mt-3 flex flex-col gap-2">
								{history.issues.map((m) => (
									<div
										key={m.id}
										className="rounded-lg border border-solid border-slate-200 bg-slate-50 p-3"
									>
										<div className="text-sm font-medium text-slate-900">
											{m.task.name}
										</div>
										<p className="mt-1 text-sm text-slate-600">
											{m.note ? m.note : "No note"}
										</p>
									</div>
								))}
							</div>
						)}
					</section>
				</div>
			</div>
		</div>
	);
};

export default RollbackHistory;
