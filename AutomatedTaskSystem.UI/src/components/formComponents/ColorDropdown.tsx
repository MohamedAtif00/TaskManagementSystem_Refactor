import { useEffect, useRef, useState } from "react";

interface Props {
	value: { id: number; name: string } | null;
	options: { id: number; name: string }[];
	handleChange: (v: { id: number; name: string }) => void;
}

const getDropdownPosition = (target: EventTarget) => {
	if (target instanceof Element) {
		const { top, left, width } = target.getBoundingClientRect();

		return {
			top: top + 44,
			left,
			width,
		};
	}

	return { top: 0, left: 0, width: 0 };
};

const ColorDropdown = ({ value, options, handleChange }: Props) => {
	const [active, setActive] = useState(false);
	const dropdownRef = useRef<HTMLDivElement>(null);
	const dropdownListRef = useRef<HTMLDivElement>(null);
	const [{ top, left, width }, setPos] = useState({
		top: 0,
		left: 0,
		width: 0,
	});

	const handleToggle = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
		e.stopPropagation();
		if (active) return setActive(false);

		setPos(getDropdownPosition(e.currentTarget));
		setActive(true);
	};

	useEffect(() => {
		const handleScrollAndResize = () =>
			active &&
			dropdownRef.current &&
			setPos(getDropdownPosition(dropdownRef.current));

		document.addEventListener("scroll", handleScrollAndResize);
		document.addEventListener("resize", handleScrollAndResize);

		return () => {
			document.removeEventListener("scroll", handleScrollAndResize);
			document.removeEventListener("resize", handleScrollAndResize);
		};
	}, [dropdownRef, active]);

	useEffect(() => {
		const handleClickAway = () => {
			setActive(false);
		};
		document.addEventListener("click", handleClickAway);

		return () => document.removeEventListener("click", handleClickAway);
	}, [dropdownListRef, active]);

	return (
		<div className="flex flex-col relative">
			<label className="text-sm">Color:</label>
			<div
				onClick={handleToggle}
				className="basis-10 items-center flex justify-between px-2 border-2 border-solid hover:bg-slate-100 cursor-default"
				ref={dropdownRef}
			>
				<div
					className={`select-none${
						value === null && " opacity-50 text-xs"
					}`}
				>
					{value ? (
						<div className="flex items-center gap-2">
							<div
								style={{ background: value.name }}
								className="p-2 rounded-full"
							></div>
							<div className="capitalize">{value.name}</div>
						</div>
					) : (
						"Select an item"
					)}
				</div>
				<div>
					<svg
						className="h-4 w-4"
						width="68"
						height="98"
						viewBox="0 0 68 98"
						fill="none"
						xmlns="http://www.w3.org/2000/svg"
					>
						<path
							fillRule="evenodd"
							clipRule="evenodd"
							d="M37.0371 1.39683C36.2772 0.510251 35.1678 0 34.0001 0C32.8324 0 31.723 0.510251 30.963 1.39683L0.963046 36.3968C-0.474643 38.0741 -0.280396 40.5993 1.39691 42.037C3.07421 43.4747 5.59941 43.2805 7.0371 41.6032L34.0001 10.1464L60.963 41.6032C62.4007 43.2805 64.9259 43.4747 66.6032 42.037C68.2805 40.5993 68.4748 38.0741 67.0371 36.3968L37.0371 1.39683ZM7.0371 56.3968C5.59941 54.7195 3.07421 54.5253 1.39691 55.963C-0.280396 57.4007 -0.474643 59.9259 0.963046 61.6032L30.963 96.6032C31.723 97.4897 32.8324 98 34.0001 98C35.1678 98 36.2772 97.4897 37.0371 96.6032L67.0371 61.6032C68.4748 59.9259 68.2805 57.4007 66.6032 55.963C64.9259 54.5253 62.4007 54.7195 60.963 56.3968L34.0001 87.8536L7.0371 56.3968Z"
							fill="black"
						/>
					</svg>
				</div>
			</div>
			{active && (
				<div
					ref={dropdownListRef}
					onClick={(e) => e.stopPropagation()}
					style={{ top, left, width }}
					className="z-50 border border-solid py-1 fixed bg-white flex flex-col max-h-32 overflow-y-auto"
				>
					{options.map((opt) => (
						<div
							onClick={() => {
								setActive(false);
								handleChange(opt);
							}}
							className="px-4 basis-8 shrink-0 hover:bg-slate-300 hover:text-black text-slate-600 flex items-center gap-2"
							key={opt.id}
						>
							<div
								className="p-2 rounded-full"
								style={{ background: opt.name }}
							></div>
							<div className="capitalize">{opt.name}</div>
						</div>
					))}
				</div>
			)}
		</div>
	);
};

export default ColorDropdown;
