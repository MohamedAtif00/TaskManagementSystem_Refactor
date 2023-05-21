import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import Header from "../../../components/header/header";
import styles from "../../../styles/resources.module.scss";
import API from "../../../lib/API";

interface Props { }

const Section = (props: Props) => {
	const router = useRouter();
	const [section, setSection] = useState<ISection | false>(false);
	// const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);

	useEffect(() => {
		const id = router.query.sectionId;
		if (id !== undefined)
			API.RESOURCES.SECTIONS.GET_ONE(id).then((res) => {
				if (res && !res.error) {
					setSection(res.data);
				}
			});
	}, [setSection, router.query.sectionId]);

	// const sectionGroups: IGroup[] = [];

	return (
		<div className="container">
			<Header text={section ? section.name : ""} icon="Resources">
				{/* <QueryButton
					icon={<PlusIcon />}
					iconLeft
					iconRight={false}
					text="Add"
					to="/resources/groups?form=group"
				/> */}
			</Header>
			{section ? (
				<div className={styles.container}>
					<h2>Head:{section.head.name}</h2>
					<div>
						Groups:
						<div>
							{section.groups.map((g) => (
								<div key={g.id}>{g.name}</div>
							))}
						</div>
					</div>
				</div>
			) : (
				"Loading"
			)}
		</div>
	);
};

export default Section;
