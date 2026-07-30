import type { NextPage } from "next";
import Head from "next/head";
import { useAppSelector } from "../app/hooks";
import ProjectManagerDashboard from "../components/dashboardComponents/ProjectMangerDashboards";
import TeamLeaderDashboard from "../components/dashboardComponents/TeamLeaderDashboard";
import SectionHeadDashboard from "../components/dashboardComponents/SectionHeadDashboard";
import MemberDashboard from "../components/dashboardComponents/MemberDashboard";

const Home: NextPage = () => {
    const { role } = useAppSelector((s) => s.authSlice);

    return (
        <div className="grow overflow-y-auto">
            <Head>
                <title>TMS - Home</title>
                <meta
                    name="description"
                    content="Selah El Telmeez Project Management System"
                />
                <link rel="icon" href="/favicon.ico" />
            </Head>
            {role === 0 || role === 4? (
                <ProjectManagerDashboard />
            ) : role === 1 ? (
                <SectionHeadDashboard />
            ) : role === 2? (
                <TeamLeaderDashboard />
            ) : role === 3 ? (
                <MemberDashboard />
            ) : (
                ""
            )}
        </div>
    );
};

export default Home;
