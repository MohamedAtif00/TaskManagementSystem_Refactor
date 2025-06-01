import React, {
  ReactNode,
  useEffect,
  useRef,
  useState,
  createContext,
} from "react";
import * as signalR from "@microsoft/signalr";
import { useAppSelector } from "../../app/hooks";
import { url } from "../../lib/API";
import { toast } from "react-toastify";
import Link from "next/link";

// Define this component OUTSIDE of your main functional component to prevent unnecessary re-renders.
// It receives closeToast from react-toastify and leaveRequestId from your SignalR data.
const NewLeaveRequestToast = ({ closeToast, leaveRequestId }: { closeToast: () => void, leaveRequestId: number }) => {
    // Determine the path, similar to your commonActionsRenderer
    // Assuming new leave requests are always 'vacancy' type for this routing.
    // If you have different types of "new requests" (e.g., permissions),
    // you'd need to send the 'type' (e.g., 'vacancy' or 'permission') from the server too.
    const path = 'vacancy'; // Or 'permission' if your backend sends type
    const href = `/calendar/${path}/${leaveRequestId}`;

    return (
        <Link href={href} passHref> {/* passHref is important for Link wrapping custom elements */}
            {/* The <a> tag is the actual clickable element.
                Attach the closeToast handler here. */}
            <a onClick={closeToast} style={{ cursor: 'pointer', textDecoration: 'none', color: 'inherit', display: 'block', padding: '8px' }}>
                There is a new Leave Request! Click to view.
            </a>
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
  const [connectionState, setConnectionState] = useState<
    "connected" | "connecting" | "disconnected"
  >("disconnected");
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
        console.log("OnConnectedMessage received:", data);
        setPendingNumber(data.pendings);
      }
    });

    connection.onclose(() => {
      if (isMounted) setConnectionState("disconnected");
    });
  }

  const startConnection = async () => {
    try {
      if (
        connectionRef.current?.state === signalR.HubConnectionState.Disconnected
      ) {
        setConnectionState("connecting");
        await connectionRef.current.start();
        if (isMounted) setConnectionState("connected");
      }
    } catch (err) {
      if (isMounted) {
        console.error("❌ SignalR connection failed:", err);
        setConnectionState("disconnected");
        setTimeout(startConnection, 5000); // Retry
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

        const onLeaveRequestOpinion = (data: any) => {
            console.log("LeaveRequestOpinion received:", data);
            if (data.isApproved) {
                toast.success("Leave request Approved");
            } else {
                toast.warning("Leave request rejected");
            }
        };

        const onUpdatePendings = (data: any) => {
            console.log("UpdatePendings received:", data);
            if (data.pendings !== undefined) {
                setPendingNumber(data.pendings);
            }

            if (data.isNewRequest && data.newLeaveRequestId) {
                toast.info(
                    // This function receives closeToast from react-toastify
                    ({ closeToast }) => (
                        <NewLeaveRequestToast
                            leaveRequestId={data.newLeaveRequestId}
                            closeToast={closeToast} // Pass the received closeToast function
                        />
                    ),
                    {
                        autoClose: false, // Keep toast open until clicked
                        closeOnClick: false, // Prevent toast from closing on generic click; we control it
                        // You might want an ID here if you plan to dismiss specific toasts programmatically
                        // toastId: `new-leave-${data.newLeaveRequestId}`
                    }

                );
            } else if (data.isNewRequest) {
                // Fallback for general new request toast if no ID is provided for some reason
                toast.info(data.message || "There is a new Leave Request!");
            }
        };

        const onReveiveError = (data:any)=>{
            debugger
            console.log("ReceiveError received:", data);
            
            toast.error(data.message);

        }

        // ... (rest of your useEffect with connection.on/off calls)
        connection.on("LeaveRequestOpinion", onLeaveRequestOpinion);
        connection.on("OnConnectedMessage", onUpdatePendings);
        connection.on("UpdatePendings", onUpdatePendings);
        connection.on("ReceiveError", onReveiveError);

        return () => {
            connection.off("LeaveRequestOpinion", onLeaveRequestOpinion);
            connection.off("OnConnectedMessage", onUpdatePendings);
            connection.off("UpdatePendings", onUpdatePendings);
            connection.off("ReceiveError", onReveiveError);
        };
    }, [connectionState]); // No 'navigate' in dependency array for Next.js Link




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
