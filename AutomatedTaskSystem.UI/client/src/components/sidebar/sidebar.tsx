import Navlink from "./navlink";
import styles from "./styles.module.scss";
import { useRouter } from "next/router";
import authService from "../../lib/Auth";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import { logout } from "../../slices/authSlice";
import NavList from "./navlist";
import ChartIcon from "../../assets/Icons/Chart";
import { ReactElement, useContext, useEffect } from "react";
import { SignalRContext } from "../connection/connectionProvider";

const ROLE_LABELS: Record<UserRole, string> = {
    0: "Project Manager",
    1: "Section Head",
    2: "Team Leader",
    3: "Member",
    4: "Owner",
};

const Sidebar = () => {
    const dispatch = useAppDispatch();
    const user = useAppSelector((s) => s.authSlice);
    const path = useRouter().pathname;
    const auth = useAppSelector((s) => s.authSlice); // auth.role will be of type UserRole if authSlice correctly types it
    const router = useRouter()

    const { connection, connectionState, pendingNumber, unreadNotificationCount } = useContext(SignalRContext);

    const logoutHandler = () => {
        authService.logout().then(() => {
            dispatch(logout());
        });
    };

    // Build the NavList items conditionally
    const leavesNavItems: ReactElement[] = [];

    if (auth.role !== 3) {
        leavesNavItems.push(
            <Navlink
                key="calendar"
                activeCondition={path.includes("/calendar")}
                to="/calendar"
                icon="Schema"
                text={
                    <span className="flex items-center gap-1">
                        Calendar
                        {pendingNumber !== 0 && (
                            <span className="ml-1 bg-red-500 text-white text-xs font-semibold px-2 py-0.5 rounded-full">
                                {pendingNumber}
                            </span>
                        )}
                    </span>
                }
            />
        );
    }

    if (auth.role === 4) {
        leavesNavItems.push(
            <Navlink
                key="members-leaves"
                activeCondition={path.includes("/members-leaves")}
                to="/members-leaves"
                icon="Resources"
                text="Members Leaves"
            />
        );
    }

    if (auth.role !== 3) { // My Leaves will only be inside this list if auth.role is NOT 3
        leavesNavItems.push(
            <Navlink
                key="my-leaves-in-list"
                activeCondition={path.includes("/myleave")}
                to="/myleave"
                icon="Schema"
                text="My Leaves"
            />
        );
    }


    return (
        <div id={styles.sidebar}>
            <h3>TMS</h3>
            <div className={styles.profile}>
                <div className="cursor-pointer" onClick={() => router.push(`/resources/users/${auth.id}`)}>{user.name}</div>
                <div className={styles.profileInfo}>
                    {user.group ? <span>{user.group}</span> : null}
                    {user.group && <span aria-hidden="true">·</span>}
                    <span>{ROLE_LABELS[auth.role]}</span>
                </div>
            </div>
            <div className={styles.navlinks}>
                <Navlink
                    activeCondition={path === "/"}
                    to="/"
                    icon="Home"
                    text="Dashboard"
                />
                {(auth.role === 4 || auth.role === 0) ? (
                    <>
                        <Navlink
                            activeCondition={path.includes("/resources")}
                            to="/resources"
                            icon="Resources"
                            text="User Management"
                        />
                        
                        <Navlink
                            activeCondition={path.includes("/schemas")}
                            to="/schemas"
                            icon="Schema"
                            text="Work Flow"
                        />
                        <Navlink
                            activeCondition={path.includes("/projects") || path.includes("/subjects")}
                            to="/projects"
                            icon="Project"
                            text="Projects"
                        />
                        {/* <NavList label="Reports" icon={ChartIcon}>
                            <Navlink
                                activeCondition={path.includes("/project-overview")}
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
                            <>
                                {(auth.role === 4) && (
                                    <Navlink
                                        activeCondition={path.includes("/advancedReport")}
                                        to="/advancedReport"
                                        icon="Task"
                                        text="Advanced Report"
                                    />
                                )}
                            </>
                        </NavList> */}
                       
                    </>
                ) : null}
                 <Navlink
                            // Modified activeCondition for Sprints
                            activeCondition={path === "/sprints" || path.startsWith("/sprints/") || path.includes("/tasks/sprint/")}
                            to="/sprints"
                            icon="Sprint"
                            text="Sprints"
                        />
                {(auth.role === 4) && (
                    <Navlink
                        activeCondition={path.includes("/sessions")}
                        to="/sessions"
                        icon="Resources"
                        text="Sessions"
                    />
                )}
                <Navlink
                    activeCondition={path.includes("/tasks") && !path.includes("/tasks/sprint/")}
                    to="/tasks"
                    icon="Task"
                    text="Kanban"
                />
                <Navlink
                    activeCondition={path.includes("/daily-report")}
                    to="/daily-report"
                    icon="Task"
                    text="Daily Report"
                />
                <Navlink
                    activeCondition={path.includes("/task-logger")}
                    to="/task-logger"
                    icon="Task"
                    text="Task Logger"
                />
                {(auth.role < 3 || auth.role === 4) && (<Navlink
                            activeCondition={path.includes("/user-tasks")}
                            to="/user-tasks"
                            icon="Resources"
                            text="User Tasks"
                        />)}
                {/* Conditional rendering for My Leaves based on role */}
                {auth.role === 3 ? (
                    // If role is 3, render My Leaves separately
                    <Navlink
                        key="my-leaves-standalone"
                        activeCondition={path.includes("/myleave")}
                        to="/myleave"
                        icon="Schema"
                        text="My Leaves"
                    />
                ) : (
                    // For other roles, render the NavList for Leaves, passing the prepared items
                    <NavList
                        icon={ChartIcon}
                        label={
                            <span className="flex items-center gap-2">
                                Leaves
                                {pendingNumber !== 0 && (
                                    <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse"></span>
                                )}
                            </span>
                        }
                    >
                        {leavesNavItems} 
                    </NavList>
                )}
                 <Navlink
	                    activeCondition={path === "/notifications" || path.startsWith("/notifications/")}
	                    to="/notifications"
	                    icon="Notification"
	                    text={
                            <span className="flex items-center gap-1">
                                Notifications
                                {unreadNotificationCount > 0 && (
                                    <span className="ml-1 bg-red-500 text-white text-xs font-semibold px-2 py-0.5 rounded-full">
                                        {unreadNotificationCount}
                                    </span>
                                )}
                            </span>
                        }
	                />

                

            </div>
            <div className={styles.logoutButton} onClick={logoutHandler}>
                <div>Logout</div>
            </div>
        </div>
    );
};

export default Sidebar;