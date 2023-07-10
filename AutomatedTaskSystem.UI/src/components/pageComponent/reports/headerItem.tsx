interface Props {
	label: string;
	count: number;
}

const HeaderItem = ({ label, count }: Props) => {
	return (<div className="flex flex-col items-end">
		<div className="text-lg font-bold">{label}</div>
		<div>{count}</div>
	</div>);
}

export default HeaderItem;
