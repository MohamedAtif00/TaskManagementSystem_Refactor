import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import PlusIcon from "../../assets/Icons/Plus";
import styles from "../../styles/resources.module.scss";
import QueryButton from "../../components/button/queryButton";
import AddProject from "../../components/forms/projects/addProject";
import Header from "../../components/header/header";
import ProjectItem from "../../components/projectItem";
import API from "../../lib/API";
import { load } from "../../slices/projectSlice";
import { useRouter } from "next/router";

const Projects = () => {
	const projects = useAppSelector((states) => states.projectSlice);
	const dispatch = useAppDispatch();
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) {
		router.replace("/");
	}

	useEffect(() => {
		API.PROJECTS.GET_ALL().then((res) => {
			if (res && !res.error) {
				dispatch(load(res.data));
			}
		});
	}, [dispatch]);

	return (
		<div className="container">
			<Header text="Projects" icon="Project">
				<QueryButton
					icon={<PlusIcon />}
					iconLeft
					iconRight={false}
					text="Add"
					url={{
						pathname: "/projects",
						query: {
							form: "project",
						},
					}}
				/>
			</Header>
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
			<AddProject />
		</div>
	);
};

export default Projects;
