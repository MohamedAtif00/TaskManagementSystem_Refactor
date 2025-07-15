import Head from "next/head";
import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import Loader from "../../../../components/loader"; // Adjust path if needed

const TaskSheet = () => {
    const [done, setDone] = useState(false);
    const router = useRouter();
    // Destructure both dynamic route parameters
    const { sprintId, learingObjectId } = router.query;

    useEffect(() => {
        // 1. Essential Check: Wait for the router to be ready and for both IDs to be string.
        //    'router.isReady' ensures router.query is populated.
        //    Type guards (typeof === 'string') ensure we're using single string IDs.
        if (!router.isReady || typeof sprintId !== 'string' || typeof learingObjectId !== 'string') {
            return; // Exit the effect if not ready or IDs are not in the expected format
        }

        const preferredView = localStorage.getItem("tasks:view");
        // console.log('Redirecting for Sprint:', sprintId, 'Learning Objective:', learingObjectId); // Improved log

        // Construct the base path using the now-guaranteed string IDs
        const basePath = `/sprints/${sprintId}/${learingObjectId}`;

        if (preferredView !== null && preferredView === "sheet") {
            router.replace(`${basePath}/sheet`);
        } else {
            router.replace(`${basePath}/board`);
        }

        // Set done to true after initiating the redirect.
        // This component acts purely as a redirector; it will be unmounted
        // as Next.js navigates to the new page.
        setDone(true);
    }, [router.isReady, sprintId, learingObjectId, router]); // 4. Corrected dependencies

    // This component will only show a loader briefly until the redirect happens.
    // Since `router.replace` causes a navigation that replaces the current page,
    // this component will be unmounted. Therefore, it just needs a loading state
    // while it waits for the IDs to be available and the redirect to occur.
    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Head>
                <title>ATS - Loading View</title> {/* Clarified title */}
            </Head>
            <Loader />
        </div>
    );
    // Removed the `if (done)` block, as it's not needed for a redirector component.
    // The `router.replace` handles the new page rendering.
};

export default TaskSheet;