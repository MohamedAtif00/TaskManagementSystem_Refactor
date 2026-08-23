import { useEffect } from "react";
import { useRouter } from "next/router";
import Loader from "../../../../../components/loader";

const LegacyYearRouteRedirect = () => {
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;

    useEffect(() => {
        if (!router.isReady || Number.isNaN(rootProjectId)) return;
        router.replace(`/projects/${rootProjectId}`);
    }, [router, rootProjectId]);

    return (
        <div className="flex items-center justify-center mx-auto h-full">
            <Loader />
        </div>
    );
};

export default LegacyYearRouteRedirect;
