interface Props {
	name: string;
	running: number;
	done: number
}

const UnitReportItem = ({ name, running, done }: Props) => (
	<div className="text-sm flex justify-between px-8">
		<div>{name}</div>
		<div className="grid grid-cols-3 gap-4 text-end">
			<div className="text-orange-600 w-10">{running}</div>
			<div className="text-emerald-500">{done}</div>
			<div>{done}</div>
		</div>
	</div>
);

export default UnitReportItem;
