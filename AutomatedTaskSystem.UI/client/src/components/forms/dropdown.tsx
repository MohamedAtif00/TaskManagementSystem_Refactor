import styles from "./styles.module.scss";
import ArrowIcon from "../../assets/Icons/Arrow";
import React, { useEffect, useRef, useState } from "react";

interface Props {
	label: string;
	options: { id: number; name: string }[];
	id: number;
	handleChange: (id: number) => void;
}

const Dropdown = ({ label, options, id, handleChange }: Props) => {
	const [active, setActive] = useState(false);
	const listRef = useRef<HTMLDivElement>(null);
	const [height, setHeight] = useState(0);

	const foundOption = options.find((opt) => opt.id === id);

	useEffect(() => {
		if (active && listRef.current) {
			setHeight(listRef.current.offsetHeight);
		} else {
			setHeight(0);
		}
	}, [setHeight, setActive, listRef, active]);

	return (
		<div
			className={styles.dropdown}
			onMouseLeave={() => (active ? setActive(false) : "")}
			style={{
				paddingBottom: `calc(${height}px - 2.75rem)`,
			}}
		>
			<div className={styles.label}>{label}:</div>
			<div
				className={[styles.list, active ? styles.active : ""].join(" ")}
				ref={listRef}
			>
				{options.map((opt) => {
					return (
						<div
							key={opt.id}
							onClick={() => {
								handleChange(opt.id);
								setActive(false);
							}}
						>
							{opt.name}
						</div>
					);
				})}
			</div>
			<button
				className={styles.info}
				onClick={(e) => {
					e.preventDefault();
					setActive((ps) => !ps);
				}}
			>
				<div className={foundOption ? styles.darkFont : ""}>
					{foundOption
						? foundOption.name
						: `Please select a ${label}`}
				</div>
				<div>
					<ArrowIcon color="#DBDFE5" />
				</div>
			</button>
		</div>
	);
};

export default Dropdown;
