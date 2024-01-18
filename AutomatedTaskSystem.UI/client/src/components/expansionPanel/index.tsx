import { useState } from "react";

interface Props {
	header: string;
	content: string;
}

const ExpansionPanel = ({ header, content }: Props) => {
	const [toggled, setToggled] = useState(false);

	return (
		<div className="border-b border-slate-400 border-solid">
			<div
				className="px-4 py-2 text-lg flex items-center justify-between"
				onClick={() => setToggled((_) => !_)}
			>
				<div>{header}</div>
				<div
					className={`transition-all ease-out duration-300 ${
						toggled ? "" : "rotate-180"
					}`}
				>
					<svg
						width="20"
						height="8"
						viewBox="0 0 20 8"
						fill="none"
						xmlns="http://www.w3.org/2000/svg"
					>
						<path
							fillRule="evenodd"
							clipRule="evenodd"
							d="M9.21566 0.233449C9.66617 -0.0778165 10.3338 -0.0778165 10.7843 0.233449L19.5843 6.31344C20.0859 6.65998 20.1413 7.2662 19.7082 7.66746C19.275 8.06872 18.5172 8.11308 18.0156 7.76653L10 2.22846L1.98436 7.76653C1.48278 8.11308 0.725012 8.06872 0.291832 7.66746C-0.141349 7.2662 -0.0859021 6.65998 0.415675 6.31344L9.21566 0.233449Z"
							fill="black"
						/>
					</svg>
				</div>
			</div>
			<div
				className={`px-4 overflow-hidden transition-all ease-out duration-300 ${
					toggled ? "max-h-60 pb-2" : "max-h-0 py-0"
				}`}
			>
				{content === "" ? "None" : content}
			</div>
		</div>
	);
};

export default ExpansionPanel;
