import Navlink from "./navlink";
import styles from "./styles.module.scss";
import { useRouter } from "next/router";
import authService from "../../lib/Auth";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import { logout } from "../../slices/authSlice";
import NavList from "./navlist";
import ChartIcon from "../../assets/Icons/Chart";

const Sidebar = () => {
    const dispatch = useAppDispatch();
    const user = useAppSelector((s) => s.authSlice);
    const path = useRouter().pathname;
    const auth = useAppSelector((s) => s.authSlice);

    const logoutHandler = () => {
        authService.logout().then(() => {
            dispatch(logout());
        });
    };

    return (
        <div id={styles.sidebar}>
            <h3>ATS</h3>
            <div className={styles.profile}>
                <div>{user.name}</div>
                <div className={styles.profileInfo}>
                    <div>{user.group}</div>
                </div>
            </div>
            <div className={styles.navlinks}>
                <Navlink
                    activeCondition={path === "/"}
                    to="/"
                    icon="Home"
                    text="Home"
                />
                {auth.role == 0 ? (
                    <>
                        <Navlink
                            activeCondition={path.includes("/resources")}
                            to="/resources"
                            icon="Resources"
                            text="Resources"
                        />
                        <Navlink
                            activeCondition={path.includes("/schemas")}
                            to="/schemas"
                            icon="Schema"
                            text="Schemas"
                        />
                        <Navlink
                            activeCondition={path.includes("/projects")}
                            to="/projects"
                            icon="Project"
                            text="Projects"
                        />
                        <NavList label="Reports" icon={ChartIcon}>
                            <Navlink
                                activeCondition={path.includes(
                                    "/project-overview"
                                )}
                                to="/project-overview"
                                icon="Project"
                                text="Projects Overview"
                            />
                            <Navlink
                                activeCondition={path.includes("/summaries")}
                                to="/summaries"
                                icon="Project"
                                text="Summaries"
                            />
                        </NavList>
                    </>
                ) : (
                    ""
                )}
                <Navlink
                    activeCondition={path.includes("/tasks")}
                    to="/tasks"
                    icon="Task"
                    text="Tasks"
                />
            </div>
            <div className={styles.logoutButton} onClick={logoutHandler}>
                <div>Logout</div>
            </div>
        </div>
    );
};

export default Sidebar;
