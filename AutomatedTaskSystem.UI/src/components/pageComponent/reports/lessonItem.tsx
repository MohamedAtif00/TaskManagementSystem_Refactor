import { useState } from "react";

interface Props {
	name: string;
	running: number;
	done: number;
	idle: number;
	learningObjectives: LearningObjectiveReport[];
}

const ReportLessonItem = ({ name, idle, running, done, learningObjectives }: Props) => {
	const [toggle, setToggle] = useState(false);

	return (
		<div>
			<div onClick={() => setToggle(_ => !_)} className="cursor-pointer h-10 text-sm flex items-center justify-between pr-8">
				<div>{name}</div>
				<div className="grid grid-cols-4 gap-4 text-end">
					<div className="text-slate-400 w-20">{idle}</div>
					<div className="text-orange-600">{running}</div>
					<div className="text-emerald-500">{done}</div>
					<div>{done + running + idle}</div>
				</div>
			</div>
			{toggle && <div className="text-sm pl-4 cursor-default">
				<div className="border-slate-300 border-solid border-l">{learningObjectives.map(lo => {
					const sd = lo.started ? new Date(lo.started) : null;
					const ed = lo.done ? new Date(lo.done) : null;

					return (<div className="px-4 flex justify-between items-center border-solid border-slate-300 py-2" key={lo.id}>
						{ed ?
							<div className="text-emerald-500">{lo.name}</div>
							:
							sd ? <div className="text-orange-600">{lo.name}</div> : <div className="text-slate-500">{lo.name}</div>
						}
						<div className="grid grid-cols-2 gap-4">
							<div>{sd && <>
								<div className="text-xs">Started:</div>
								<div>{sd.getHours()}:{sd.getMinutes()} - {sd.getDate()}/{sd.getMonth()}/{sd.getFullYear()}</div>
							</>}</div>
							<div>
								{ed && <>
									<div className="text-xs">Done:</div>
									<div>{ed.getHours()}:{ed.getMinutes()} - {ed.getDate()}/{ed.getMonth()}/{ed.getFullYear()}</div>
								</>}
							</div>
						</div>
					</div>);
				})}</div>
			</div>}
		</div>
	);
}

export default ReportLessonItem;
