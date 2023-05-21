import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import styles2 from "../../components/styles.module.scss";
import styles from "../../styles/resources.module.scss";
import Header from "../../components/header/header";
import API from "../../lib/API";
import { load } from "../../slices/projectSlice";
import Link from "next/link";

interface Props {
	id: number;
	name: string;
	description: string;
}

const ProjectItem = ({ id, name, description }: Props) => {
	return (
		<Link href={`/tasks/${id}`}>
			<div className={styles2.GridItem} style={{ padding: "0.5rem 1rem" }}>
				<div>
					<div>{name}</div>
					<div className={styles2.info}>{description}</div>
				</div>
			</div>
		</Link>
	);
};

const Projects = () => {
	const projects = useAppSelector((states) => states.projectSlice);
	const dispatch = useAppDispatch();

	useEffect(() => {
		API.TASKS.PROJECTS().then((res) => {
			if (res && !res.error) {
				dispatch(load(res.data));
			}
		});
	}, [dispatch]);

	return (
		<div className="container">
			<Header text="Projects" icon="Project" />
			<div className={styles.container}>
				{projects.map((p) => {
					return (
						<ProjectItem
							id={p.id}
							description={p.description}
							name={p.name}
							key={p.id}
						/>
					);
				})}
			</div>
		</div>
	);
};

export default Projects;
