import {
	ArrowPathIcon,
	BookmarkSlashIcon,
	CalendarIcon,
	ChatBubbleBottomCenterTextIcon,
	CheckIcon,
	ClockIcon,
	DocumentPlusIcon,
	ExclamationCircleIcon,
	FlagIcon,
	ForwardIcon,
	PauseIcon,
	PlayIcon,
	PlusIcon,
	UserIcon,
} from "@heroicons/react/24/outline";
import dateHandler from "../../../lib/DateHandler";
import { ITaskActivity } from "..";

type Props = {
	name: string;
} & ITaskActivity;

const TaskActivity: React.FC<Props> = (props) => {
	const date = dateHandler(props.timeStamp);

	return (
		<div className="w-full border-2 border-solid border-slate-300 p-2 rounded-md text-sm">
			{props.type === 1 ? (
				<div className="flex gap-4">
					<div>
						<DocumentPlusIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">{props.name}</span> was
							created
							{props.actorOne ? (
								<>
									{" "}
									by{" "}
									<span className="font-bold">
										{props.actorOne.name}
									</span>
								</>
							) : (
								"."
							)}
						</div>
					</div>
				</div>
			) : props.type === 2 ? (
				<div className="flex gap-4">
					<div>
						<PlusIcon className="p-1 h-6 w-6 rounded-full border border-solid border-blue-600 box-content stroke-blue-600" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							added task to their To Do list.
						</div>
					</div>
				</div>
			) : props.type === 3 ? (
				<div className="flex gap-4">
					<div>
						<PlayIcon className="p-1 h-6 w-6 rounded-full border border-solid border-orange-600 box-content stroke-orange-600" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task started by{" "}
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 4 ? (
				<div className="flex gap-4">
					<div>
						<CheckIcon className="p-1 h-6 w-6 rounded-full border border-solid box-content border-emerald-600 stroke-emerald-600" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task completed by{" "}
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 5 ? (
				<div className="flex gap-4">
					<div>
						<CheckIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							rolled back Task to{" "}
							<span className="font-bold">
								{props.secondaryTask
									? props.secondaryTask.name
									: "a previous task"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 6 ? (
				<div className="flex gap-4">
					<div>
						<PauseIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							paused the task.
						</div>
					</div>
				</div>
			) : props.type === 7 ? (
				<div className="flex gap-4">
					<div>
						<PlayIcon className="p-1 h-6 w-6 rounded-full border border-solid border-orange-600 box-content stroke-orange-600" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							resumed the task.
						</div>
					</div>
				</div>
			) : props.type === 8 ? (
				<div className="flex gap-4">
					<div>
						<FlagIcon className="p-1 h-6 w-6 rounded-full border border-solid border-rose-600 box-content stroke-red-600 stroke-2" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							flagged the task
							{props.actorTwo ? (
								<>
									{" "}
									for{" "}
									<span className="font-bold">
										{props.actorTwo.name}
									</span>
								</>
							) : null}
							.
						</div>
						{props.additionalInfo && (
							<div className="text-sm text-slate-600 mt-1">
								&ldquo;{props.additionalInfo}&rdquo;
							</div>
						)}
					</div>
				</div>
			) : props.type === 9 ? (
				<div className="flex gap-4">
					<div>
						<BookmarkSlashIcon className="p-1 h-6 w-6 rounded-full border border-solid border-blue-600 box-content stroke-blue-600" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							cleared flag.
						</div>
					</div>
				</div>
			) : props.type === 10 ? (
				<div className="flex gap-4">
					<div>
						<UserIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							assigned the task to{" "}
							<span className="font-bold">
								{props.actorTwo
									? props.actorTwo.name
									: "no one"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 11 ? (
				<div className="flex gap-4">
					<div>
						<ChatBubbleBottomCenterTextIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							left a comment.
						</div>
					</div>
				</div>
			) : props.type === 12 ? (
				<div className="flex gap-4">
					<div>
						<ArrowPathIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task was rolled back from{" "}
							<span className="font-bold">
								{props.secondaryTask
									? props.secondaryTask.name
									: "User"}
							</span>{" "}
							by{" "}
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 13 ? (
				<div className="flex gap-4">
					<div>
						<ExclamationCircleIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							updated task priority to{" "}
							<span className="font-bold">
								{props.additionalInfo
									? props.additionalInfo
									: "None"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 14 ? (
				<div className="flex gap-4">
					<div>
						<ForwardIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task was skipped by{" "}
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 15 ? (
				<div className="flex gap-4">
					<div>
						<ForwardIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>Task was completed due to process change.</div>
					</div>
				</div>
			) : props.type === 16 ? (
				<div className="flex gap-4">
					<div>
						<ForwardIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task was skipped due to a jump by{" "}
							<span className="font-bold">
								{props.actorOne
									? props.actorOne.name
									: "a User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 17 ? (
				<div className="flex gap-4">
					<div>
						<ExclamationCircleIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task was reactivated due to a jump by{" "}
							<span className="font-bold">
								{props.actorOne
									? props.actorOne.name
									: "a User"}
							</span>
							.
						</div>
					</div>
				</div>
			) : props.type === 18 ? (
				<div className="flex gap-4">
					<div>
						<ExclamationCircleIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							Task was reactivated due to a previous task
							completion.
						</div>
					</div>
				</div>
			) : props.type === 19 ? (
				<div className="flex gap-4">
					<div>
						<ChatBubbleBottomCenterTextIcon className="p-1 h-6 w-6 rounded-full border border-solid border-blue-400 box-content stroke-blue-500" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							edited a comment.
						</div>
					</div>
				</div>
			) : props.type === 20 ? (
				<div className="flex gap-4">
					<div>
						<ChatBubbleBottomCenterTextIcon className="p-1 h-6 w-6 rounded-full border border-solid border-red-400 box-content stroke-red-500" />
					</div>
					<div className="flex flex-col justify-center">
						<div>
							<span className="font-bold">
								{props.actorOne ? props.actorOne.name : "User"}
							</span>{" "}
							removed a comment.
						</div>
					</div>
				</div>
			) : (
				""
			)}
			<div className="flex gap-4 justify-center text-slate-600 text-xs">
				<div className="flex gap-1 items-center w-32 justify-end">
					<CalendarIcon className="w-5 h-5" />
					<div>{date.date}</div>
				</div>
				<div className="flex gap-1 items-center w-32 justify-start">
					<ClockIcon className="w-5 h-5" />
					<div>{`${date.hours}:${date.minutes} ${date.con}`}</div>
				</div>
			</div>
		</div>
	);
};

export default TaskActivity;
