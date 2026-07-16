import PROJECTS from "./Projects";
import RESOURCES from "./Resouces";
import SCHEMAS from "./Schemas";
import TASKS from "./tasks";
import DASHBOARDS from "./dashboard";
import SPRINTS from "./sprints";
import LEAVE from "./Leave";
import PERMISSION from "./Permission";
import WORK_FROM_HOME from "./workFromHome";
import NOTIFICATIONS from "./Notifications";
import DAILY_REPORT from "./dailyReport";


// export const url = "/api";
export const url = "http://localhost:5238";
//  export const url = process.env.REACT_APP_API_URL || "/api";

export interface BasicInfo {
    id: number;
    name: string;
}

export interface CommonResponse {
    error: boolean;
    info: string;
}

export interface ErrorResponse {
    error: true;
    info: string;
}

const API = {
    RESOURCES,
    SCHEMAS,
    PROJECTS,
    TASKS,
    DASHBOARDS,
    SPRINTS,
    LEAVE,
    PERMISSION,
	    WORK_FROM_HOME,
	    NOTIFICATIONS,
    DAILY_REPORT
};

export default API;
