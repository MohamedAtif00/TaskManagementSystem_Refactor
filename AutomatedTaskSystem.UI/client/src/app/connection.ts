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

if (typeof window !== "undefined") {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${url}/userhub`, {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => {
        return localStorage.getItem("access-token") || "";
      },
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
}

export default connection;
