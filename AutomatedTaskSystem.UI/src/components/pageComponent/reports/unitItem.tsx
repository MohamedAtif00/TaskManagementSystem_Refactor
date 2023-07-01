import { useState } from "react";
import ReportLessonItem from "./lessonItem";

interface Props {
	name: string;
	running: number;
	done: number;
	idle: number;
	lessons: LessonReport[]
}

const UnitReportItem = ({ name, running, done, idle, lessons }: Props) => {
	const [toggle, setToggle] = useState(false);

	return (
		<div>
			<div onClick={() => setToggle(_ => !_)} className="px-8 h-10 text-sm flex items-center justify-between cursor-pointer border-y border-slate-200 border-solid">
				<div>{name}</div>
				<div className="grid grid-cols-4 gap-4 text-end">
					<div className="text-slate-400 w-20">{idle}</div>
					<div className="text-orange-600">{running}</div>
					<div className="text-emerald-500">{done}</div>
					<div>{done + running + idle}</div>
				</div>
			</div>
			{toggle && <div className="pl-12 cursor-default">
				<div className="pl-4 border-slate-300 border-solid border-l">{lessons.map(l => {
					return (<ReportLessonItem
						key={l.id}
						running={l.runningLearningObjectives}
						done={l.doneLearningObjectives}
						name={l.name}
						learningObjectives={l.learningObjectives}
						idle={l.idleLearningObjectives}
					/>);
				})}</div>
			</div>}
		</div>
	);
}

export default UnitReportItem;
