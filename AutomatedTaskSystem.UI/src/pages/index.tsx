import type { NextPage } from "next";
import Head from "next/head";
import { useAppSelector } from "../app/hooks";
import ProjectManagerDashboard from "../components/dashboardComponents/ProjectMangerDashboards";

const Home: NextPage = () => {
    const user = useAppSelector((s) => s.authSlice);

    return (
        <div className="grow px-8 py-4">
            <Head>
                <title>ATS</title>
                <meta
                    name="description"
                    content="Selah El Telmeez Project Management System"
                />
                <link rel="icon" href="/favicon.ico" />
            </Head>
            {user.role === 1 ? <ProjectManagerDashboard /> : ""}
        </div>
    );
};

export default Home;
