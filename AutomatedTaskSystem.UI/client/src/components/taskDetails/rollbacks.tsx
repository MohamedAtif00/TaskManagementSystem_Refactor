import Link from "next/link";
import CrossIcon from "../../assets/Icons/Cross";
import { useEffect, useState } from "react";
import API from "../../lib/API";
import Head from "next/head";
import Loader from "../loader";
import useTaskPathHandler from "./useTaskPathHandler.ts";

interface Props {
	taskId: number;
	isReview: boolean;
	type:string
}

interface History {
	rollbacks: {
		id: number;
		clarification?: string;
		task: BasicInfo;
	}[];
	issues: {
		id: number;
		note?: string;
		task: BasicInfo;
	}[];
}

const RollbackHistory: React.FC<Props> = ({ taskId, isReview ,type}) => {
	const [history, setHistory] = useState<History>();
	const pathHandler = useTaskPathHandler({type:type});

	useEffect(() => {
		API.TASKS.GET_ROLLBACK_HISTORY(taskId).then(
			(res) => res && !res.error && setHistory(res.data)
		);
	}, [taskId]);

	if (history === undefined)
		return (
			<div className="fixed z-50 flex justify-center items-center bg-black/25 top-0 left-0 bottom-0 right-0">
				<Head>
					<title>ATS - Loading</title>
				</Head>
				<Loader />
			</div>
		);

	return (
		<div className="fixed top-0 bottom-0 left-0 right-0 bg-black/25 z-50 flex items-center justify-center">
			<div className="bg-white rounded-lg overflow-hidden">
				<div className="px-6 py-3 text-xl border-b border-slate-200 border-solid flex justify-between items-center">
					<div>Rollback History:</div>
					<Link
						href={{
							pathname: pathHandler(),
							query: {
								taskId,
							},
						}}
					>
						<CrossIcon className="stroke-black" />
					</Link>
				</div>
				<div className="w-96 max-h-96 py-3 overflow-auto">
					<div className="flex flex-col gap-2 border-b border-slate-200 border-solid mb-4 pb-3">
						<div className="text-lg px-6">
							<span className="font-bold">
								{history.rollbacks.length}
							</span>{" "}
							Rollbacks {isReview ? "to" : "from"}:
						</div>
						{history.rollbacks.map((m) => (
							<div
								key={m.id}
								className="grid grid-cols-2 px-6 gap-3"
							>
								<div className="text-sm">{m.task.name}</div>
								<div>{m.clarification}</div>
							</div>
						))}
					</div>
					<div className="flex flex-col gap-2">
						<div className="text-lg px-6">
							<span className="font-bold">
								{history.issues.length}
							</span>{" "}
							Issues {isReview ? "at" : "noted from"}:
						</div>
						{history.issues.map((m) => (
							<div
								key={m.id}
								className="grid grid-cols-2 px-6 gap-3"
							>
								<div className="text-sm">{m.task.name}</div>
								<div>{m.note ? m.note : "No note"}</div>
							</div>
						))}
					</div>
				</div>
			</div>
		</div>
	);
};

export default RollbackHistory;
