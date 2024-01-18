import HeaderItem from "./headerItem";

interface Props {
	name: string;
	running: number;
	done: number;
	idle: number;
}

const ReportHeader = ({ name, running, done, idle }: Props) => {
	return <div className="shrink-0 flex items-center justify-between px-8 w-full h-20 border-b border-solid border-slate-500">
		<div className="text-2xl font-bold">{name}</div>
		<div className="grid grid-cols-4 gap-4">
			<div className="text-slate-400 w-20">
				<HeaderItem label="Idle" count={idle} />
			</div>
			<div className="text-orange-600">
				<HeaderItem label="Running" count={running} />
			</div>
			<div className="text-emerald-600">
				<HeaderItem label="Done" count={done} />
			</div>
			<div className="text-black">
				<HeaderItem label="Total" count={done + running + idle} />
			</div>
		</div>
	</div>;
}

export default ReportHeader;
