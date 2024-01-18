import Link from "next/link";
import styles from "./styles.module.scss";

const GroupItem = ({
	id,
	name,
	color,
	members,
}: {
	id: number;
	name: string;
	members: number;
	color: string;
}) => {
	return (
		<Link href={`/resources/groups?form=editGroup&groupId=${id}`}>
			<div className={styles.groupItem}>
				<div
					className={styles.color}
					style={{ background: color }}
				></div>
				<div>
					<div>{name}</div>
					<div className={styles.info}>{members} Members</div>
				</div>
			</div>
		</Link>
	);
};

export default GroupItem;
