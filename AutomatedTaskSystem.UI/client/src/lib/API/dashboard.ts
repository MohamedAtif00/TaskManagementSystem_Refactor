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
    GET_TL_DB: async () => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/dashboards/team-leader`, {
                headers: {
                    ...authHeader,
                },
            });
            const data: ResponseService<TeamLeaderDashboard> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_SECTION_HEAD_DB: async () => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/dashboards/section-head`, {
                headers: {
                    ...authHeader,
                },
            });
            const data: ResponseService<SectionHeadDashboard> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    GET_MEMBER_DB: async () => {
        try {
            const authHeader = authService.authHeader();
            const res = await fetch(`${url}/dashboards/member`, {
                headers: {
                    ...authHeader,
                },
            });
            const data: ResponseService<MemberDashboard> = await res.json();
            return data;
        } catch (err) {
            console.error(err);
            return false;
        }
    },
};

export default dashboard;
