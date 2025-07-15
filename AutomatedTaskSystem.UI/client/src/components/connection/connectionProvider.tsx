import React, { ReactNode, useEffect, useRef, useState, createContext } from "react";
import * as signalR from "@microsoft/signalr";
import { useAppSelector } from "../../app/hooks";
import { url } from "../../lib/API";
import { toast } from "react-toastify";
import Link from "next/link";

// Toast components
const NewLeaveRequestToast = ({ closeToast, leaveRequestId }: { closeToast: () => void, leaveRequestId: number }) => {
    const href = `/calendar/vacancy/${leaveRequestId}`;
    return (
        <Link 
            href={href}
            onClick={closeToast}
            style={{ cursor: 'pointer', textDecoration: 'none', color: 'inherit', display: 'block', padding: '8px' }}
        >
            There is a new Leave Request! Click to view.
        </Link>
    );
};

const NewPermissionRequestToast = ({ closeToast, permissionId }: { closeToast: () => void, permissionId: number }) => {
    const href = `/calendar/permission/${permissionId}`;
    return (
        <Link 
            href={href}
            onClick={closeToast}
            style={{ cursor: 'pointer', textDecoration: 'none', color: 'inherit', display: 'block', padding: '8px' }}
        >
            There is a new Permission Request! Click to view.
        </Link>
    );
};

const NewWorkFromHomeRequestToast = ({ closeToast, workFromHomeId }: { closeToast: () => void, workFromHomeId: number }) => {
    const href = `/calendar/workFromHome/${workFromHomeId}`;
    return (
        <Link 
            href={href}
            onClick={closeToast}
            style={{ cursor: 'pointer', textDecoration: 'none', color: 'inherit', display: 'block', padding: '8px' }}
        >
            There is a new Work From Home Request! Click to view.
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

        // Register all handlers
        connection.on("LeaveRequestOpinion", (data) => onRequestOpinion(data, 'leave'));
        connection.on("PermissionRequestOpinion", (data) => onRequestOpinion(data, 'permission'));
        connection.on("WorkFromHomeOpinion", (data) => onRequestOpinion(data, 'workFromHome')); // Added listener for WFH opinions
        connection.on("UpdatePendings", onUpdatePendings);
        connection.on("ReceiveError", onReceiveError);

        return () => {
            connection.off("LeaveRequestOpinion");
            connection.off("PermissionRequestOpinion");
            connection.off("WorkFromHomeOpinion"); // Unregister WFH opinion listener
            connection.off("UpdatePendings");
            connection.off("ReceiveError");
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