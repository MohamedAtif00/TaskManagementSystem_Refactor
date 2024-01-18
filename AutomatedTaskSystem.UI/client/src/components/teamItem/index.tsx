import Link from "next/link";
import styles from "../styles.module.scss";

const TeamItem = ({
	name,
	members,
	id,
}: {
	name: string;
	members: number;
	id: number;
}) => {
	return (
		<Link href={`/resources/teams/${id}`}>
			<div className={styles.GridItem} style={{ padding: "0.5rem 1rem" }}>
				<div>
					<div>{name}</div>
					<div className={styles.info}>{members} Members</div>
				</div>
			</div>
		</Link>
	);
};

export default TeamItem;
