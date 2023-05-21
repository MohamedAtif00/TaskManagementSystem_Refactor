import HomeIcon from "../../assets/Icons/Home";
import ProjectIcon from "../../assets/Icons/Project";
import ResourcesIcon from "../../assets/Icons/Resources";
import SchemaIcon from "../../assets/Icons/Schema";
import TaskIcon from "../../assets/Icons/Task";
import styles from "./styles.module.scss";

const Header = ({
	icon,
	text,
	children,
}: {
	text: string;
	icon: "Home" | "Resources" | "Schema" | "Project" | "Task" | "None";
	children?: JSX.Element[] | JSX.Element;
}) => {
	return (
		<div className={styles.pageheader}>
			<div>
				<div style={{ display: "flex" }}>
					{icon === "Home" ? (
						<HomeIcon color={"#29313d"} />
					) : icon === "Resources" ? (
						<ResourcesIcon color={"#29313d"} />
					) : icon === "Schema" ? (
						<SchemaIcon color={"#29313d"} />
					) : icon === "Project" ? (
						<ProjectIcon color={"#29313d"} />
					) : icon === "Task" ? (
						<TaskIcon color={"#29313d"} />
					) : (
						<></>
					)}
				</div>
				<div>{text}</div>
			</div>
			<div>{children}</div>
		</div>
	);
};

export default Header;
