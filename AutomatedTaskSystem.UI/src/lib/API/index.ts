import PROJECTS from "./Projects";
import RESOURCES from "./Resouces";
import SCHEMAS from "./Schemas";
import TASKS from "./Tasks";

export const url = "http://localhost:5238";

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
};

export default API;
