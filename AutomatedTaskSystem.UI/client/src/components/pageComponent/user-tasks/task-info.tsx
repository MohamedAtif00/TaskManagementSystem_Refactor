import { EyeIcon } from "@heroicons/react/24/outline";
import Link from "next/link";

interface Props {
	name: string;
	loName: string;
	projectId: number;
	taskId: number;
}

const TaskInfo = (props: Props) => {
	return (
		<div className="border-blue-500 border border-solid rounded-lg overflow-hidden">
			<p className="text-xs text-slate-600 text-center p-2">
				{props.name}
			</p>
			<p className="text-sm px-2 mb-2">{props.loName}</p>
			<Link
				href={{
					pathname: `/tasks/${props.projectId}`,
					query: {
						taskId: props.taskId,
					},
				}}
				className="bg-blue-500 px-4 py-2 flex justify-center gap-2 text-white w-full text-center"
			>
				<EyeIcon className="h-5" />
				<div className="text-sm">View</div>
			</Link>
		</div>
	);
};

export default TaskInfo;
