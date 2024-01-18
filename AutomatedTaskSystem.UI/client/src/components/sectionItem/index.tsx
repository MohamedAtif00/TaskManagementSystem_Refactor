import Link from "next/link";
import styles from "../styles.module.scss";

interface Props {
	id: number;
	name: string;
	description?: string;
}

const SectionItem = ({ id, name, description = "" }: Props) => {
	return (
		<Link href={`/resources/sections/${id}`}>
			<div className={styles.GridItem} style={{ padding: "0.5rem 1rem" }}>
				<div>
					<div>{name}</div>
					<div className={styles.info}>{description}</div>
				</div>
			</div>
		</Link>
	);
};

export default SectionItem;
