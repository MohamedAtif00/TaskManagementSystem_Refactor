import styles from "./styles.module.scss";

const Checkbox = ({
	label,
	toggle,
	active,
}: {
	label: string;
	active: boolean;
	toggle: () => void;
}) => {
	return (
		<div className={styles.select}>
			<div className={styles.info}>
				<div onClick={toggle} style={{ padding: "0.75rem 0" }}>
					<div
						className={[
							styles.checkbox,
							active ? styles.checked : "",
						].join(" ")}
					></div>
					<div>{label}</div>
				</div>
			</div>
		</div>
	);
};

export default Checkbox;
