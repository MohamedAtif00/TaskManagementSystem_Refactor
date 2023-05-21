import styles from "./styles.module.scss";

const SelectNodesWithToggle = ({
	label,
	selected,
	updateList,
	list,
	toggle,
	checkbox,
}: {
	label: string;
	selected: number[];
	updateList: (n: number) => void;
	list: { id: number; name: string }[];
	checkbox: boolean;
	toggle: () => void;
}) => {
	return (
		<div className={styles.select}>
			<div className={styles.info}>
				<div className={styles.label}>{label}:</div>
				<div onClick={toggle} style={{ padding: "0.75rem 0" }}>
					<div
						className={[
							styles.checkbox,
							checkbox ? styles.checked : "",
						].join(" ")}
					></div>
					<div>Start Point</div>
				</div>
			</div>
			{!checkbox
				? list.map((s) => (
						<div
							onClick={() => updateList(s.id)}
							className={[
								styles.item,
								selected.includes(s.id) ? styles.selected : "",
							].join(" ")}
							key={s.id}
						>
							{s.name}
						</div>
				  ))
				: ""}
		</div>
	);
};

export default SelectNodesWithToggle;
