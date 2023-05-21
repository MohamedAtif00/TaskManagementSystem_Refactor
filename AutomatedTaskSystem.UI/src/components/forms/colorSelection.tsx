import styles from "./styles.module.scss";

const ColorSelection = ({
	list,
	active,
	onClick,
}: {
	list: { color: string; id: number }[];
	active: number;
	onClick: (id: number) => void;
}) => {
	return (
		<div className={styles.colors}>
			<div className={styles.label}>Color:</div>
			<div className={styles.list}>
				{list.map((c) => (
					<div
						key={c.id}
						className={[
							styles.color,
							active === c.id ? styles.activeColor : "",
						].join(" ")}
						onClick={() => onClick(c.id)}
					>
						<div style={{ background: c.color }}></div>
					</div>
				))}
			</div>
		</div>
	);
};

export default ColorSelection;
