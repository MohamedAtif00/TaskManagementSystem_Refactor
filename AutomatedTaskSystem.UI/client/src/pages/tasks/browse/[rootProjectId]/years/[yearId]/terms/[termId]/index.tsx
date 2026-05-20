import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import Head from "next/head";
import API from "../../../../../../../../lib/API";
import Loader from "../../../../../../../../components/loader";
import TaskIcon from "../../../../../../../../assets/Icons/Task";
import CurriculumBreadcrumb from "../../../../../../../../components/curriculum/CurriculumBreadcrumb";
import TaskSubjectsDataGrid from "../../../../../../../../components/tasks/TaskSubjectsDataGrid";
import { formatDisplayDate } from "../../../../../../../../lib/formatDate";

type IdName = { id: number; name: string };

const TaskTermSubjectsPage = () => {
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rawYear = router.query.yearId;
    const rawTerm = router.query.termId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const yearId = rawYear ? Number(Array.isArray(rawYear) ? rawYear[0] : rawYear) : NaN;
    const termId = rawTerm ? Number(Array.isArray(rawTerm) ? rawTerm[0] : rawTerm) : NaN;

    const [rootName, setRootName] = useState("");
    const [yearName, setYearName] = useState("");
    const [termName, setTermName] = useState("");
    const [termStartDate, setTermStartDate] = useState<string | null>(null);
    const [termEndDate, setTermEndDate] = useState<string | null>(null);
    const [subjects, setSubjects] = useState<IProject[]>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!router.isReady || Number.isNaN(termId)) return;
        setLoading(true);
        (async () => {
            const rootsRes = await API.PROJECTS.ROOT.LIST();
            if (
                rootsRes &&
                typeof rootsRes === "object" &&
                "error" in rootsRes &&
                !rootsRes.error &&
                "data" in rootsRes
            ) {
                const r = (rootsRes as { data: IdName[] }).data.find((x) => x.id === rootProjectId);
                if (r) setRootName(r.name);
            }
            const yearsRes = await API.PROJECTS.ROOT.YEARS(rootProjectId);
            if (
                yearsRes &&
                typeof yearsRes === "object" &&
                "error" in yearsRes &&
                !yearsRes.error &&
                "data" in yearsRes
            ) {
                const y = (yearsRes as { data: IdName[] }).data.find((x) => x.id === yearId);
                if (y) setYearName(y.name);
            }
            const termRes = await API.PROJECTS.ROOT.GET_TERM(termId);
            if (
                termRes &&
                typeof termRes === "object" &&
                "error" in termRes &&
                !termRes.error &&
                "data" in termRes
            ) {
                const t = (termRes as {
                    data: { name: string; startDate?: string | null; endDate?: string | null };
                }).data;
                setTermName(t.name);
                setTermStartDate(t.startDate ?? null);
                setTermEndDate(t.endDate ?? null);
            }
            const res = await API.PROJECTS.GET_BY_TERM(termId);
            if (res && !res.error) {
                setSubjects(res.data);
            } else {
                setSubjects([]);
            }
            setLoading(false);
        })();
    }, [router.isReady, rootProjectId, yearId, termId, router.asPath]);

    if (!router.isReady || loading || subjects === undefined) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || Number.isNaN(yearId) || Number.isNaN(termId)) {
        return <div className="p-6">Invalid URL.</div>;
    }

    const browseBase = `/tasks/browse/${rootProjectId}`;
    const yearHref = `${browseBase}/years/${yearId}`;

    return (
        <div className="mx-auto relative max-h-screen w-full min-w-0 overflow-y-auto overflow-x-hidden px-2 sm:px-4 box-border">
            <Head>
                <title>ATS - Subjects — {termName}</title>
            </Head>
            <div className="pt-4 px-2">
                <CurriculumBreadcrumb
                    items={[
                        { label: "Tasks", href: "/tasks" },
                        { label: rootName, href: browseBase },
                        { label: yearName, href: yearHref },
                        { label: termName },
                    ]}
                />
            </div>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 min-h-20 sticky top-0 left-0 right-0 flex items-center justify-between gap-4 py-3">
                <div className="flex gap-2 items-center min-w-0">
                    <TaskIcon className="stroke-black shrink-0" />
                    <div className="min-w-0">
                        <h1 className="font-bold text-2xl truncate">Subjects</h1>
                        <p className="text-sm text-slate-600 truncate">
                            {termName}
                            {(termStartDate || termEndDate) && (
                                <>
                                    {" "}
                                    · {formatDisplayDate(termStartDate)} – {formatDisplayDate(termEndDate)}
                                </>
                            )}
                        </p>
                    </div>
                </div>
            </div>
            <TaskSubjectsDataGrid subjects={subjects} />
        </div>
    );
};

export default TaskTermSubjectsPage;
