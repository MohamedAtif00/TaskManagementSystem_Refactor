import Link from "next/link";
import CrossIcon from "../../assets/Icons/Cross";

interface Props {
	taskId: number;
	projectId: number;
	isReview: boolean;
}

const RollbackHistory: React.FC<Props> = ({ taskId, projectId, isReview }) => {
	return (
		<div className="fixed top-0 bottom-0 left-0 right-0 bg-black/25 z-50 flex items-center justify-center">
			<div className="bg-white rounded-lg overflow-hidden">
				<div className="px-6 py-3 text-xl border-b border-slate-200 border-solid flex justify-between items-center">
					<div>Rollback History:</div>
					<Link
						href={{
							pathname: `/tasks/${projectId}`,
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
							<span className="font-bold">3</span> Rollbacks{" "}
							{isReview ? "to" : "from"}:
						</div>
						<div className="grid grid-cols-2 px-6 gap-3">
							<div className="text-sm">ID GD Review</div>
							<div>I don't Like it</div>
						</div>
						<div className="grid grid-cols-2 px-6 gap-3">
							<div className="text-sm">Senior GD Review</div>
							<div>Bad colors</div>
						</div>
					</div>
					<div className="flex flex-col gap-2">
						<div className="text-lg px-6">
							<span className="font-bold">6</span> Notes{" "}
							{isReview ? "to" : "from"}:
						</div>
						<div className="grid grid-cols-2 px-6 gap-3">
							<div className="text-sm">QC</div>
							<div>Should never have passed</div>
						</div>
						<div className="grid grid-cols-2 px-6 gap-3">
							<div className="text-sm">Dev TL Review</div>
							<div>Nah</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};

export default RollbackHistory;
