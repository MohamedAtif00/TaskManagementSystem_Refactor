import { useState } from "react";
import ReportLessonItem from "./lessonItem";

interface Props {
	name: string;
	running: number;
	done: number
	lessons: LessonReport[]
}

const UnitReportItem = ({ name, running, done, lessons }: Props) => {
	const [toggle, setToggle] = useState(false);

	return (
		<div>
			<div onClick={() => setToggle(_ => !_)} className="px-8 h-10 text-sm flex items-center justify-between cursor-pointer border-y border-slate-200 border-solid">
				<div>{name}</div>
				<div className="grid grid-cols-3 gap-4 text-end">
					<div className="text-orange-600 w-10">{running}</div>
					<div className="text-emerald-500">{done}</div>
					<div>{done + running}</div>
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
					/>);
				})}</div>
			</div>}
		</div>
	);
}

export default UnitReportItem;
