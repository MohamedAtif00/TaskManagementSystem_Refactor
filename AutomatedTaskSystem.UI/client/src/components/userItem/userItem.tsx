import styles from "../styles.module.scss";

const UserItem = ({ name, group }: { name: string; group: string }) => {
	return (
		<div className={styles.GridItem} style={{ padding: "0.5rem 1rem" }}>
			<div>
				<div>{name}</div>
				<div className={styles.info}>{group}</div>
			</div>
		</div>
	);
};

export default UserItem;
