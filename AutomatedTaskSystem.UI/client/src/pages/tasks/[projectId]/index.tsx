import Head from "next/head";
import Loader from "../../../components/loader";
import { useEffect, useState } from "react";
import { useRouter } from "next/router";

const TaskSheet = () => {
    const [done, setDone] = useState(false);
    const router = useRouter();

    useEffect(() => {
        const preferredView = localStorage.getItem("tasks:view");

        if (preferredView !== null && preferredView === "sheet") {
            router.replace(`/tasks/${router.query.projectId}/sheet`);
        } else {
            router.replace(`/tasks/${router.query.projectId}/board`);
        }

        setDone(true); // Don't return this – it's not a cleanup function.
    }, [router]); // Include 'router' to fix the ESLint warning

    if (done) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>TMS - Select a view</title>
                </Head>
                <div>
                    {/* Maybe display some "Redirecting..." message or similar */}
                </div>
            </div>
        );
    }

    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>TMS - Loading</title>
            </Head>
            <Loader />
        </div>
    );
};

export default TaskSheet;
