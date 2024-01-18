import styles from "./styles.module.scss";

const FormField = ({
	label,
	value,
	onChange,
}: {
	label: string;
	value: string;
	onChange: (s: string) => void;
}) => {
	return (
		<div className={styles.field}>
			<label className={styles.label}>
				{label}:
				<div>
					<input
						type="text"
						value={value}
						onChange={(e) => {
							const targetValue = e.target.value;
							if (targetValue === " ") {
								return onChange("");
							}
							const len = targetValue.length;
							if (
								targetValue[len - 1] === " " &&
								targetValue[len - 2] === " "
							) {
								return;
							}
							onChange(targetValue);
						}}
					/>
				</div>
			</label>
		</div>
	);
};

export default FormField;
