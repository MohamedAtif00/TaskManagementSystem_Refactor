import Head from "next/head";
import Loader from "../../../components/loader";
import { useEffect, useState } from "react";
import { useRouter } from "next/router";

const TaskSheet = () => {
    const [done, setDone] = useState(false);
    const router = useRouter();

    useEffect(() => {
        const preferredView = localStorage.getItem("tasks:view");

        if (preferredView !== null && preferredView === "sheet") 
            router.push(`/tasks/${router.query.projectId}/sheet`)
        else
            router.push(`/tasks/${router.query.projectId}/board`)

        return setDone(true);
    }, [])

    if (done) {
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Select a view</title>
            </Head>
            <div>
            </div>
        </div>
    }

    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Loading</title>
            </Head>
            <Loader />
        </div>
    );
}

export default TaskSheet;
