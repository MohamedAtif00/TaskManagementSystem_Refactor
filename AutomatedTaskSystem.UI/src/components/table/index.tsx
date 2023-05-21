import styles from "./styles.module.scss";

interface Props {
	title: string;
	option?: JSX.Element;
	children?: JSX.Element | JSX.Element[];
}

const Table = ({ title, children, option }: Props) => {
	return (
		<div className={styles.table}>
			<div className={styles.info}>
				<div className={styles.title}>{title}</div>
				{option ? <div>{option}</div> : ""}
			</div>
			{children ? <div className={styles.content}>{children}</div> : ""}
		</div>
	);
};

export default Table;
