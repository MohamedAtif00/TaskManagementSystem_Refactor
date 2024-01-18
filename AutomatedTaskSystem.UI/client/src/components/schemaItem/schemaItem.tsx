import Link from "next/link";
import styles from "../styles.module.scss";

const SchemaItem = ({
	name,
	tasks,
	id,
}: {
	name: string;
	tasks: number;
	id: number;
}) => {
	return (
		<Link href={`/schemas/${id}`}>
			<div className={styles.GridItem} style={{ padding: "0.5rem 1rem" }}>
				<div>
					<div>{name}</div>
					<div className={styles.info}>{tasks} Tasks</div>
				</div>
			</div>
		</Link>
	);
};

export default SchemaItem;
