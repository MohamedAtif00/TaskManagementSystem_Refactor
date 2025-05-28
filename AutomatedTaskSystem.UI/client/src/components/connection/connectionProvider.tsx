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

    const onGetrequestCount = (data: any) => {
      console.log("OnConnectedMessage received:", data);
      // You can update pendingNumber here if relevant
    
      if (data.pendings) {
        setPendingNumber(data.pendings);
        
      }
    };

    connection.on("LeaveRequestOpinion", onLeaveRequestOpinion);
    connection.on("OnConnectedMessage", onGetrequestCount);

    return () => {
      connection.off("LeaveRequestOpinion", onLeaveRequestOpinion);
      connection.off("OnConnectedMessage", onGetrequestCount);
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
