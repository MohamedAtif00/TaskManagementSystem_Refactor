import { url } from ".";
import authService from "../Auth";

const dashboard = {
    GET_PM_DB: async () => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/dashboards/project-manager`, {
                headers: {
                    ...authHeader,
                },
            });
            const data: ResponseService<ProjectManagerDashboard> =
                await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
};

export default dashboard;
