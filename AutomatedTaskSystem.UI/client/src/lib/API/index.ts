import PROJECTS from "./Projects";
import RESOURCES from "./Resouces";
import SCHEMAS from "./Schemas";
import TASKS from "./tasks";
import DASHBOARDS from "./dashboard";

export const url = "/api";

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
};

export default API;
