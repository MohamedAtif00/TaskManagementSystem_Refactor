import styles from "./styles.module.scss";
import HomeIcon from "../../assets/Icons/Home";
import ResourcesIcon from "../../assets/Icons/Resources";
import SchemaIcon from "../../assets/Icons/Schema";
import Link from "next/link";
import ProjectIcon from "../../assets/Icons/Project";
import TaskIcon from "../../assets/Icons/Task";
import { useState } from "react";

const Navlink = ({
    icon,
    text,
    to,
    activeCondition,
}: {
    icon: "Home" | "Resources" | "Schema" | "Project" | "Task" | "None";
    text: string;
    to: string;
    activeCondition: boolean;
}) => {
    const [hover, setHover] = useState(false);
    const color = hover || activeCondition ? "#fff" : "#97a6ba";
    return (
        <Link href={to}>
            <div
                className={[
                    styles.navlink,
                    activeCondition ? styles.active : "",
                ].join(" ")}
                onMouseEnter={() => setHover(true)}
                onMouseLeave={() => setHover(false)}
            >
                <div className="flex h-5 w-5">
                    {icon === "Home" ? (
                        <HomeIcon color={color} />
                    ) : icon === "Resources" ? (
                        <ResourcesIcon color={color} />
                    ) : icon === "Schema" ? (
                        <SchemaIcon color={color} />
                    ) : icon === "Project" ? (
                        <ProjectIcon color={color} />
                    ) : icon === "Task" ? (
                        <TaskIcon color={color} />
                    ) : (
                        <></>
                    )}
                </div>
                <div>{text}</div>
            </div>
        </Link>
    );
};

export default Navlink;
