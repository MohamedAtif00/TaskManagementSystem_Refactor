import React, { ReactNode, useCallback, useEffect, useRef, useState, createContext } from "react";
import * as signalR from "@microsoft/signalr";
import { useAppSelector } from "../../app/hooks";
import API, { url } from "../../lib/API";
import { toast } from "react-toastify";
import Link from "next/link";
import { useRouter } from "next/router";
import FlagNotificationContent from "../notifications/FlagNotificationContent";

		// Toast components
		const NewLeaveRequestToast = ({ closeToast, leaveRequestId }: { closeToast: () => void; leaveRequestId: number }) => {
		    const href = `/calendar/vacancy/${leaveRequestId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            There is a new Leave Request! Click to view.
		        </Link>
		    );
		};
		
		const NewPermissionRequestToast = ({ closeToast, permissionId }: { closeToast: () => void; permissionId: number }) => {
		    const href = `/calendar/permission/${permissionId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            There is a new Permission Request! Click to view.
		        </Link>
		    );
		};
		
		const NewWorkFromHomeRequestToast = ({ closeToast, workFromHomeId }: { closeToast: () => void; workFromHomeId: number }) => {
		    const href = `/calendar/workFromHome/${workFromHomeId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            There is a new Work From Home Request! Click to view.
		        </Link>
		    );
		};

		const TaskAssignedToast = ({ closeToast, taskId, projectId, taskName, projectName }: { closeToast: () => void; taskId: number; projectId: number; taskName: string; projectName: string }) => {
		    const href = `/tasks/${projectId}/board?taskId=${taskId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            New task assigned to you: <strong>{taskName}</strong> ({projectName}). Click to view.
		        </Link>
		    );
		};

		const TaskFlaggedToast = ({
		    closeToast,
		    taskId,
		    projectId,
		    taskName,
		    learningObjectiveName,
		    projectName,
		    flaggedByUserName,
		    comment,
		    canView = false,
		}: {
		    closeToast: () => void;
		    taskId: number;
		    projectId: number;
		    taskName: string;
		    learningObjectiveName?: string;
		    projectName: string;
		    flaggedByUserName?: string;
		    comment?: string;
		    canView?: boolean;
		}) => {
		    const href = `/tasks/${projectId}/board?taskId=${taskId}`;
		    const content = (
		        <div className="rounded-lg border-l-4 border-red-500 bg-gradient-to-r from-red-50 to-rose-50 p-3 -m-2">
		            <FlagNotificationContent
		                compact
		                data={{
		                    taskName,
		                    learningObjectiveName,
		                    projectName,
		                    flaggedByUserName,
		                    comment,
		                    projectId,
		                    canView,
		                }}
		            />
		            {canView && (
		                <p className="mt-2 text-xs font-semibold text-red-600 pl-12">
		                    Click to view task →
		                </p>
		            )}
		        </div>
		    );

		    if (!canView) {
		        return content;
		    }

		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            className="block no-underline text-inherit"
		        >
		            {content}
		        </Link>
		    );
		};

		const ProjectAssignedToast = ({ closeToast, projectId, projectName}: 
			{ closeToast: () => void; projectId: number; projectName: string }) => {
			const href = `/tasks/${projectId}`;

		    return (
		        <Link
				href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            You have been assigned to project: ({projectName}). Click to view.
		        </Link>
		    );
		};

		const ProjectCompletedToast = ({
		    closeToast,
		    projectId,
		    projectName,
		    totalTasks,
		    completedTasks,
		    remainingTasks,
		}: {
		    closeToast: () => void;
		    projectId: number;
		    projectName: string;
		    totalTasks?: number;
		    completedTasks?: number;
		    remainingTasks?: number;
		}) => {
		    const href = `/subjects/${projectId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            <div>
		                <div>
		                    Project <strong>{projectName}</strong> has been completed.
		                </div>
		                {typeof totalTasks === "number" && typeof completedTasks === "number" && (
		                    <div>
		                        Tasks: {completedTasks}/{totalTasks}
		                        {typeof remainingTasks === "number" && remainingTasks > 0 ? ` (${remainingTasks} remaining)` : ""}.
		                    </div>
		                )}
		                <div>Click to view project details.</div>
		            </div>
		        </Link>
		    );
		};

		const ProjectClosedToast = ({
		    closeToast,
		    projectId,
		    projectName,
		    closedManually,
		    yearName,
		}: {
		    closeToast: () => void;
		    projectId: number;
		    projectName: string;
		    closedManually?: boolean;
		    yearName?: string;
		}) => {
		    const href = `/subjects/${projectId}`;
		    const closedText = closedManually ? "has been closed manually." : "has been closed.";
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{ cursor: "pointer", textDecoration: "none", color: "inherit", display: "block", padding: "8px" }}
		        >
		            <div>
		                <div>
		                    Project <strong>{projectName}</strong> {closedText}
		                </div>
		                {yearName && <div>Year: {yearName}</div>}
		                <div>Click to view project details.</div>
		            </div>
		        </Link>
		    );
		};

export interface SignalRContextType {
    connection: signalR.HubConnection | null;
    connectionState: "connected" | "connecting" | "disconnected";
    pendingNumber: number;
    unreadNotificationCount: number;
    refreshUnreadNotificationCount: () => Promise<void>;
}

export const SignalRContext = createContext<SignalRContextType>({
    connection: null,
    connectionState: "disconnected",
    pendingNumber: 0,
    unreadNotificationCount: 0,
    refreshUnreadNotificationCount: async () => {},
});

const SignalRProvider = ({ children }: { children: ReactNode }) => {
    const auth = useAppSelector((state) => state.authSlice);
    const router = useRouter();
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const [connectionState, setConnectionState] = useState<"connected" | "connecting" | "disconnected">("disconnected");
    const [pendingNumber, setPendingNumber] = useState(0);
    const [unreadNotificationCount, setUnreadNotificationCount] = useState(0);

    const refreshUnreadNotificationCount = useCallback(async () => {
        if (!auth.isAuth) {
            setUnreadNotificationCount(0);
            return;
        }

        const res = await API.NOTIFICATIONS.GET_UNREAD_COUNT();
        if (!res.error && res.data !== undefined) {
            setUnreadNotificationCount(res.data);
        }
    }, [auth.isAuth]);

		    useEffect(() => {
        let isMounted = true;

        if (!auth.isAuth) {
            connectionRef.current?.stop();
            connectionRef.current = null;
            setConnectionState("disconnected");
            setUnreadNotificationCount(0);
            return;
        }

        refreshUnreadNotificationCount();

        if (!connectionRef.current) {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(`${url}/userhub`, {
                    accessTokenFactory: () => localStorage.getItem("access-token") || "",
                    skipNegotiation: true,
                    transport: signalR.HttpTransportType.WebSockets,
                })
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            connectionRef.current = connection;

            connection.on("OnConnectedMessage", (data) => {
                if (isMounted && data?.pendings !== undefined) {
                    // console.log("OnConnectedMessage received:", data);
                    setPendingNumber(data.pendings);
                }
            });

            connection.onclose(() => {
                if (isMounted) setConnectionState("disconnected");
            });
        }

        const startConnection = async () => {
            try {
                if (connectionRef.current?.state === signalR.HubConnectionState.Disconnected) {
                    setConnectionState("connecting");
                    await connectionRef.current.start();
                    if (isMounted) setConnectionState("connected");
                }
            } catch (err) {
                if (isMounted) {
                    console.error("❌ SignalR connection failed:", err);
                    setConnectionState("disconnected");
                    setTimeout(startConnection, 5000);
                }
            }
        };

        startConnection();

        return () => {
            isMounted = false;
            connectionRef.current?.stop();
            connectionRef.current = null;
        };
    }, [auth.isAuth, refreshUnreadNotificationCount]);

    useEffect(() => {
        if (!auth.isAuth) return;
        refreshUnreadNotificationCount();
    }, [auth.isAuth, router.pathname, refreshUnreadNotificationCount]);

		    useEffect(() => {
        const connection = connectionRef.current;
        if (!connection || connectionState !== "connected") return;

        // Combined handlers
         const onRequestOpinion = (data: any, type: 'leave' | 'permission' | 'workFromHome') => {
            refreshUnreadNotificationCount();
            let requestTypeString = '';
            switch(type) {
                case 'leave':
                    requestTypeString = 'Leave';
                    break;
                case 'permission':
                    requestTypeString = 'Permission';
                    break;
                case 'workFromHome':
                    requestTypeString = 'Work From Home';
                    break;
                default:
                    requestTypeString = 'Request';
            }
            // console.log(`${requestTypeString} request opinion received:`, data);
            if (data.isApproved) {
                toast.success(`${requestTypeString} request has been Approved.`);
            } else {
                toast.warning(`${requestTypeString} request has been Rejected.`);
            }
        };

	        const onUpdatePendings = (data: any) => {
            // console.log("UpdatePendings received:", data);
			console.log("UpdatePendings received:", data);
            if (data.pendings !== undefined) {
                setPendingNumber(data.pendings);
            }

            if (data.isNewRequest) {
                refreshUnreadNotificationCount();
                if (data.newLeaveRequestId) {
                    toast.info(
                        ({ closeToast }) => <NewLeaveRequestToast leaveRequestId={data.newLeaveRequestId} closeToast={closeToast} />,
                        { autoClose: false, closeOnClick: false }
                    );
                } 
                else if (data.newPermissionId) {
                    toast.info(
                        ({ closeToast }) => <NewPermissionRequestToast permissionId={data.newPermissionId} closeToast={closeToast} />,
                        { autoClose: false, closeOnClick: false }
                    );
                }
                else if (data.newWorkFromHomeRequestId) {
                    toast.info(
                        ({ closeToast }) => <NewWorkFromHomeRequestToast workFromHomeId={data.newWorkFromHomeRequestId}  closeToast={closeToast} />,
                        { autoClose: false, closeOnClick: false }
                    );
                }
                else {
                    toast.info(data.message || "There is a new request!");
                }
            }
        };

	        const onReceiveError = (data: any) => {
            // console.log("ReceiveError received:", data);
            toast.error(data.message);
        };

		const onTaskAssigned = (data: any) => {
		
			if (!data) return;
			console.log("TaskAssigned received:", data);
			const { taskId, projectId, taskName, projectName } = data;
			if (!taskId || !projectId) return;
            refreshUnreadNotificationCount();
			toast.info(
				({ closeToast }) => (
					<TaskAssignedToast
						closeToast={closeToast}
						taskId={taskId}
						projectId={projectId}
						taskName={taskName}
						projectName={projectName}
					/>
				),
				{ autoClose: 4000, closeOnClick: false }
			);
		};

		const onTaskFlagged = (data: any) => {
			if (!data) return;
			const { taskId, projectId, taskName, learningObjectiveName, projectName, flaggedByUserName, comment, canView } = data;
			if (!taskId || !projectId) return;
			refreshUnreadNotificationCount();
			toast(
				({ closeToast }) => (
					<TaskFlaggedToast
						closeToast={closeToast}
						taskId={taskId}
						projectId={projectId}
						taskName={taskName}
						learningObjectiveName={learningObjectiveName}
						projectName={projectName}
						flaggedByUserName={flaggedByUserName}
						comment={comment}
						canView={canView === true}
					/>
				),
				{
					autoClose: 6000,
					closeOnClick: false,
					className: "!bg-white !border !border-red-200 !shadow-lg !rounded-xl",
					bodyClassName: "!p-3",
					progressClassName: "!bg-red-500",
				}
			);
		};

		const onProjectAssignedd = (data: any) => {
	            if (!data || !data.projectId) return;
				console.log("ProjectAssigned received:", data);
	            const { projectId, projectName, description, assignedByUserId, assignedByUserName } = data;
                refreshUnreadNotificationCount();
	            toast.info(
	                ({ closeToast }) => (
	                    <ProjectAssignedToast
	                        closeToast={closeToast}
	                        projectId={projectId}
	                        projectName={projectName}

	                    />
	                ),
	                { autoClose: 4000, closeOnClick: false }
	            );
	        };

	        const onProjectCompleted = (data: any) => {
	            if (!data || !data.projectId) return;
				console.log("ProjectCompleted received:", data);
	            const { projectId, projectName, totalTasks, completedTasks, remainingTasks } = data;
                refreshUnreadNotificationCount();
	            toast.info(
	                ({ closeToast }) => (
	                    <ProjectCompletedToast
	                        closeToast={closeToast}
	                        projectId={projectId}
	                        projectName={projectName}
	                        totalTasks={totalTasks}
	                        completedTasks={completedTasks}
	                        remainingTasks={remainingTasks}
	                    />
	                ),
	                { autoClose: false, closeOnClick: false }
	            );
	        };

			 

	        const onProjectClosed = (data: any) => {
	            if (!data || !data.projectId) return;
				console.log("ProjectClosed received:", data);
	            const { projectId, projectName, closedManually, yearName } = data;
                refreshUnreadNotificationCount();
	            toast.info(
	                ({ closeToast }) => (
	                    <ProjectClosedToast
	                        closeToast={closeToast}
	                        projectId={projectId}
	                        projectName={projectName}
	                        closedManually={closedManually}
	                        yearName={yearName}
	                    />
	                ),
	                { autoClose: false, closeOnClick: false }
	            );
	        };

	        // Register all handlers
			connection.on("LeaveRequestOpinion", (data) => onRequestOpinion(data, 'leave'));
			connection.on("PermissionRequestOpinion", (data) => onRequestOpinion(data, 'permission'));
			connection.on("WorkFromHomeOpinion", (data) => onRequestOpinion(data, 'workFromHome')); // Added listener for WFH opinions
			connection.on("UpdatePendings",(data)=> onUpdatePendings(data));
			connection.on("ReceiveError", onReceiveError);
			connection.on("TaskAssigned",(data)=> onTaskAssigned(data));
			connection.on("TaskFlagged", (data) => onTaskFlagged(data));
			connection.on("ProjectAssigned",(data)=> onProjectAssignedd(data));
			connection.on("ProjectCompleted", onProjectCompleted);
			connection.on("ProjectClosed", onProjectClosed);
	
			return () => {
				connection.off("LeaveRequestOpinion");
				connection.off("PermissionRequestOpinion");
				connection.off("WorkFromHomeOpinion"); // Unregister WFH opinion listener
				connection.off("UpdatePendings");
				connection.off("ReceiveError");
				connection.off("TaskAssigned");
				connection.off("TaskFlagged");
				connection.off("ProjectCompleted");
				connection.off("ProjectClosed");
			};
    }, [connectionState, refreshUnreadNotificationCount]);

    return (
        <SignalRContext.Provider
            value={{
                connection: connectionRef.current,
                connectionState,
                pendingNumber,
                unreadNotificationCount,
                refreshUnreadNotificationCount,
            }}
        >
            {children}
        </SignalRContext.Provider>
    );
};

export default SignalRProvider;