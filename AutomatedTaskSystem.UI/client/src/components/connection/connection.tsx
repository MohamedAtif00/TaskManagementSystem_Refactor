// SignalRContext.tsx
import React, { createContext, useContext, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  connectionState: "connected" | "connecting" | "disconnected";
  pendingNumber: number;
}


export const SignalRContext = createContext<SignalRContextType>({
  connection: null,
  connectionState: "disconnected",
  pendingNumber:0
});

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalR must be used within SignalRProvider');
  }
  return context;
};