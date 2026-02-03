import React, { ReactNode, useEffect, useRef, useState, createContext } from "react";
import * as signalR from "@microsoft/signalr";
import { useAppSelector } from "../../app/hooks";
import { url } from "../../lib/API";
import { ToastContainer, ToastContentProps, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './toastify-custom.css';
import Link from "next/link";

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
		    const href = `/projects/${projectId}`;
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
		    const href = `/projects/${projectId}`;
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

		const RollbackToast = ({
		    closeToast,
		    fromUserName,
		    toTaskId,
		    toTaskName,
		    projectId,
		    projectName,
			toUserName,
		    loName,
			critical=false
		}: {
		    closeToast: () => void;
		    fromUserName: string;
		    toTaskId: number;
		    toTaskName: string;
			toUserName: string;
		    projectId: number;
		    projectName: string;
		    loName: string;
			critical?: boolean;
		}) => {
		    const href = `/tasks/${projectId}/board?taskId=${toTaskId}`;
		    return (
		        <Link
		            href={href}
		            onClick={closeToast}
		            style={{textDecoration: "none", color: "inherit"}}
		        >
		            <div style={{width:"381px",height:"56px",display:"flex", alignItems:"center", gap:"12px", padding: "12px 5px"}}>
						<div className="rollback-icon">
							{
								critical ?
								<svg width="24" height="22" viewBox="0 0 24 22" fill="none" xmlns="http://www.w3.org/2000/svg">
									<path d="M4.40076 9.22133C7.56193 3.61508 9.14251 0.8125 11.6461 0.8125C14.1497 0.8125 15.7303 3.61508 18.8914 9.22133L19.2858 9.919C21.9128 14.5773 23.2269 16.9065 22.0396 18.6095C20.8523 20.3125 17.9143 20.3125 12.0404 20.3125H11.2518C5.37793 20.3125 2.43993 20.3125 1.25259 18.6095C0.0652605 16.9065 1.37934 14.5773 4.00643 9.919L4.40076 9.22133Z" stroke="#DC2626" strokeWidth="1.625"/>
									<path d="M11.646 6.22916V11.6458" stroke="#DC2626" strokeWidth="1.625" strokeLinecap="round"/>
									<path d="M11.6463 15.9792C12.2446 15.9792 12.7297 15.4941 12.7297 14.8958C12.7297 14.2975 12.2446 13.8125 11.6463 13.8125C11.048 13.8125 10.563 14.2975 10.563 14.8958C10.563 15.4941 11.048 15.9792 11.6463 15.9792Z" fill="#DC2626"/>
								</svg>

								:
								<svg width="22" height="12" viewBox="0 0 22 12" fill="none" xmlns="http://www.w3.org/2000/svg">
									<path d="M4.10385 6.15075C5.01121 4.61791 6.39715 3.42573 8.04838 2.75767C9.69962 2.0896 11.5246 1.98268 13.2426 2.45335C14.9605 2.92402 16.4762 3.9462 17.5564 5.36258C18.6365 6.77897 19.2213 8.51107 19.2207 10.2923C19.2207 10.5796 19.3348 10.8552 19.538 11.0584C19.7411 11.2615 20.0167 11.3757 20.304 11.3757C20.5913 11.3757 20.8669 11.2615 21.07 11.0584C21.2732 10.8552 21.3874 10.5796 21.3874 10.2923C21.3875 8.11346 20.6961 5.99076 19.4128 4.22993C18.1294 2.4691 16.3203 1.16101 14.2461 0.494038C12.1718 -0.172936 9.93934 -0.164373 7.87024 0.518494C5.80114 1.20136 4.00215 2.52329 2.73235 4.29391L2.1506 0.995163C2.10075 0.712154 1.94052 0.46054 1.70515 0.295672C1.46978 0.130804 1.17857 0.0661878 0.895557 0.116037C0.612549 0.165887 0.360935 0.32612 0.196067 0.561486C0.0311988 0.796852 -0.0334178 1.08807 0.0164318 1.37108L1.14527 7.7725C1.1955 8.05538 1.35605 8.30673 1.5916 8.47125C1.7451 8.57418 1.92044 8.64 2.10376 8.6635C2.28707 8.687 2.47335 8.66755 2.64785 8.60666L8.80118 7.52225C9.08419 7.4724 9.3358 7.31216 9.50067 7.0768C9.66554 6.84143 9.73016 6.55021 9.68031 6.2672C9.63046 5.9842 9.47022 5.73258 9.23486 5.56771C8.99949 5.40285 8.70827 5.33823 8.42527 5.38808L4.10385 6.15075Z" fill="#F59E0B"/>
								</svg>

							}
						</div>
						<div>
							<strong>{fromUserName}</strong> has rolled back task <strong>{toTaskName}</strong> to <strong>{toUserName}</strong>
						</div>
					</div>
		        </Link>
		    );
		};

export interface SignalRContextType {
    connection: signalR.HubConnection | null;
    connectionState: "connected" | "connecting" | "disconnected";
    pendingNumber: number;
}

export const SignalRContext = createContext<SignalRContextType>({
    connection: null,
    connectionState: "disconnected",
    pendingNumber: 0,
});

const SignalRProvider = ({ children }: { children: ReactNode }) => {
    const auth = useAppSelector((state) => state.authSlice);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const [connectionState, setConnectionState] = useState<"connected" | "connecting" | "disconnected">("disconnected");
    const [pendingNumber, setPendingNumber] = useState(0);

		    useEffect(() => {
        let isMounted = true;

        if (!auth.isAuth) {
            connectionRef.current?.stop();
            connectionRef.current = null;
            setConnectionState("disconnected");
            return;
        }

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
    }, [auth.isAuth]);

	useEffect(() => {
        const connection = connectionRef.current;
        if (!connection || connectionState !== "connected") return;

        // Combined handlers
         const onRequestOpinion = (data: any, type: 'leave' | 'permission' | 'workFromHome') => {
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
				{ autoClose: false, closeOnClick: false }
			);
		};

		const onProjectAssignedd = (data: any) => {
	            if (!data || !data.projectId) return;
				console.log("ProjectAssigned received:", data);
	            const { projectId, projectName, description, assignedByUserId, assignedByUserName } = data;
	            toast.info(
	                ({ closeToast }) => (
	                    <ProjectAssignedToast
	                        closeToast={closeToast}
	                        projectId={projectId}
	                        projectName={projectName}

	                    />
	                ),
	                { autoClose: false, closeOnClick: false }
	            );
		};

		const onProjectCompleted = (data: any) => {
			if (!data || !data.projectId) return;
			console.log("ProjectCompleted received:", data);
			const { projectId, projectName, totalTasks, completedTasks, remainingTasks } = data;
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

		const onNormalRollback = (data: any) => {
			if (!data) return;
			console.log("NormalRollback received:", data);

			const {
				fromUserName,
				toTaskId,
				toTaskName,
				projectId,
				projectName,
				toUserName,
				loName,
				critical
			} = data;

			// Only show toast if we have the required data
			if (!toTaskId || !projectId) return;

			toast.warning(
				({ closeToast }) => (
					<RollbackToast
						closeToast={closeToast}
						fromUserName={fromUserName || "Someone"}
						toTaskId={toTaskId}
						toTaskName={toTaskName || "Unknown task"}
						projectId={projectId}
						toUserName={toUserName || "a user"}
						projectName={projectName || "Unknown project"}
						loName={loName || ""}
						critical={critical || false}
					/>
				),
				{
					icon: false,
					autoClose: false,
					closeOnClick: false,
					className: 'rollback-toast'
				}
			);
		};

		

	        // Register all handlers
			connection.on("LeaveRequestOpinion", (data) => onRequestOpinion(data, 'leave'));
			connection.on("PermissionRequestOpinion", (data) => onRequestOpinion(data, 'permission'));
			connection.on("WorkFromHomeOpinion", (data) => onRequestOpinion(data, 'workFromHome')); // Added listener for WFH opinions
			connection.on("UpdatePendings",(data)=> onUpdatePendings(data));
			connection.on("ReceiveError", onReceiveError);
			connection.on("TaskAssigned",(data)=> onTaskAssigned(data));
			connection.on("ProjectAssigned",(data)=> onProjectAssignedd(data));
			connection.on("ProjectCompleted", onProjectCompleted);
			connection.on("ProjectClosed", onProjectClosed);
			connection.on("NormalRollback",(data)=> onNormalRollback(data));
			return () => {
				connection.off("LeaveRequestOpinion");
				connection.off("PermissionRequestOpinion");
				connection.off("WorkFromHomeOpinion"); // Unregister WFH opinion listener
				connection.off("UpdatePendings");
				connection.off("ReceiveError");
				connection.off("TaskAssigned");
				connection.off("ProjectCompleted");
				connection.off("ProjectClosed");
				connection.off("NormalRollback");
			};
    }, [connectionState]);

    return (
        <SignalRContext.Provider
            value={{
                connection: connectionRef.current,
                connectionState,
                pendingNumber,
            }}
        >
            {children}
        </SignalRContext.Provider>
    );
};

export default SignalRProvider;