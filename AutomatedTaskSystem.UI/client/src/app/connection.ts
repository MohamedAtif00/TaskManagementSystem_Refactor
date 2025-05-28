// import * as signalR from "@microsoft/signalr";
// import { url } from "../lib/API";
// import authService from "../lib/Auth";

import * as signalR from "@microsoft/signalr";
import { url } from "../lib/API";
// const connection = new signalR.HubConnectionBuilder()
//   .withUrl(`${url}/userhub`, {
//     skipNegotiation: true,
//     transport: signalR.HttpTransportType.WebSockets,
//     accessTokenFactory: () => {
//         return localStorage.getItem("access-token") || "";
//       }
//   })
//   .withAutomaticReconnect()
//   .configureLogging(signalR.LogLevel.Information)
//   .build();

// export default connection;


let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection | null {
  if (!connection && typeof window !== "undefined") {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${url}/userhub`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => localStorage.getItem("access-token") || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
  }
  return connection;
}

export default getConnection();


