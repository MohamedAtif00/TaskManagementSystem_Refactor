import styles from "./styles.module.scss";

interface Props {
	label?: string;
	updateSelected: (n: number) => void;
	list: { id: number; name: string }[];
	selected: number;
}

const SelectOne = ({ label, list, updateSelected, selected }: Props) => {
	if (list.length === 0) {
		return <></>;
	}
	return (
		<div className={styles.select}>
			{label ? (
				<div className={styles.info}>
					<div className={styles.label}>{label}:</div>
				</div>
			) : (
				""
			)}
			{list.map((s) => (
				<div
					onClick={() => updateSelected(s.id)}
					className={[
						styles.item,
						selected == s.id ? styles.selected : "",
					].join(" ")}
					key={s.id}
				>
					{s.name}
				</div>
			))}
		</div>
	);
};

export default SelectOne;
