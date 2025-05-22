import type { NextPage } from "next";
import Head from "next/head";
import { useAppSelector } from "../app/hooks";
import ProjectManagerDashboard from "../components/dashboardComponents/ProjectMangerDashboards";
import TeamLeaderDashboard from "../components/dashboardComponents/TeamLeaderDashboard";

const Home: NextPage = () => {
    const { role } = useAppSelector((s) => s.authSlice);

    return (
        <div className="grow overflow-y-auto">
            <Head>
                <title>ATS - Home</title>
                <meta
                    name="description"
                    content="Selah El Telmeez Project Management System"
                />
                <link rel="icon" href="/favicon.ico" />
            </Head>
            {role === 0 || role === 4? (
                <ProjectManagerDashboard />
            ) : role === 2 ? (
                <TeamLeaderDashboard />
            ) : (
                ""
            )}
        </div>
    );
};

export default Home;
