import HeaderItem from "./headerItem";

interface Props {
	name: string;
	running: number;
	done: number
}

const ReportHeader = ({ name, running, done }: Props) => {
	return <div className="flex items-center justify-between px-8 w-full h-20 border-b border-solid border-slate-500">
		<div className="text-2xl font-bold">{name}</div>
		<div className="flex gap-4">
			<div className="text-orange-600">
				<HeaderItem label="Running" count={running} />
			</div>
			<div className="text-emerald-600">
				<HeaderItem label="Done" count={done} />
			</div>
			<div className="text-black">
				<HeaderItem label="Total" count={done + running} />
			</div>
		</div>
	</div>;
}

export default ReportHeader;
