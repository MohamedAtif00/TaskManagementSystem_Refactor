import Head from "next/head";
import Loader from "../../../../components/loader";
import { useEffect } from "react";
import { useRouter } from "next/router";

/**
 * This page acts as a redirector. When a user navigates to a sprint,
 * it checks their preferred view ('board' or 'sheet') from localStorage
 * and redirects them accordingly. It shows a loader while this happens.
 */
const SprintRedirectPage = () => {
    const router = useRouter();
    const { sprintId, loid } = router.query;
    useEffect(() => {
        // The router is not ready on the first render, so we wait.
        // sprintId will be undefined until the router is ready.
        if (!router.isReady) {
            return;
        }

        // Ensure sprintId is a valid string before creating the redirect URL.
        if (typeof sprintId !== "string") {
            // Optional: handle invalid sprintId, e.g., redirect to a list of sprints.
            // router.replace("/tasks");
            return;
        }

        const preferredView = localStorage.getItem("tasks:view");
            const query = loid ? { loid } : {};
        if (preferredView === "sheet") {
            router.replace(`/tasks/sprint/${sprintId}/sheet`);
        } else {
            router.replace(`/tasks/sprint/${sprintId}/board`);
        }
    }, [router, sprintId]);

    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Loading</title>
            </Head>
            <Loader />
        </div>
    );
};

export default SprintRedirectPage;
