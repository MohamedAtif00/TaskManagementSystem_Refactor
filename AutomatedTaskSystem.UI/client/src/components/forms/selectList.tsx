import styles from "./styles.module.scss";

interface Props {
	label: string;
	updateList: (n: number) => void;
	list: { id: number; name: string }[];
	selected: number[];
}

const SelectList = ({ label, list, updateList, selected }: Props) => {
	if (list.length === 0) {
		return <></>;
	}
	return (
		<div className={styles.select}>
			<div className={styles.info}>
				<div className={styles.label}>{label}:</div>
			</div>
			{list.map((s) => (
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
			))}
		</div>
	);
};

export default SelectList;
