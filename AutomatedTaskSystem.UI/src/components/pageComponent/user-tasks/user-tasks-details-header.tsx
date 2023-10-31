import {
	PlayIcon,
	QueueListIcon,
	Square2StackIcon,
	UserIcon,
} from "@heroicons/react/24/outline";
import CrossIcon from "../../../assets/Icons/Cross";

interface Props {
	name: string;
	group: string;
	tasks: {
		backlog: number;
		todo: number;
		doing: number;
	};
}

const UserTasksDetailsHeader = (props: Props) => {
	return (
		<div className="flex gap-4 px-6 border-b border-solid border-slate-200 h-20 items-center">
			<div className="flex gap-2">
				<UserIcon className="stroke-black w-7" />
				<div>
					<span className="text-lg font-bold whitespace-nowrap">
						{props.name}
					</span>
					<p>{props.group}</p>
				</div>
			</div>
			<div className="pl-[1px] bg-slate-200 h-full"></div>
			<div className="text-slate-600">
				<div className="text-sm mb-1">Backlog:</div>
				<div className="flex gap-2">
					<QueueListIcon className="w-6 stroke-slate-600" />
					<span className="whitespace-nowrap">
						{props.tasks.backlog} Tasks
					</span>
				</div>
			</div>
			<div className="pl-[1px] bg-slate-200 h-full"></div>
			<div className="text-slate-600">
				<div className="text-sm mb-1">To Do:</div>
				<div className="flex gap-2">
					<Square2StackIcon className="w-6 stroke-slate-600" />
					<span className="whitespace-nowrap">
						{props.tasks.todo} Tasks
					</span>
				</div>
			</div>
			<div className="pl-[1px] bg-slate-200 h-full"></div>
			<div className="text-slate-600">
				<div className="text-sm mb-1">Doing:</div>
				<div className="flex gap-2">
					<PlayIcon className="w-6 stroke-slate-600" />
					<span className="whitespace-nowrap">
						{props.tasks.doing} Tasks
					</span>
				</div>
			</div>
			<div className="pl-[1px] bg-slate-200 h-full"></div>
			<button className="p-1 box-content" title="exit">
				<CrossIcon className="stroke-black" />
			</button>
		</div>
	);
};

export default UserTasksDetailsHeader;
