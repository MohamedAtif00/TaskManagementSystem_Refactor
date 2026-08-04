import { useCallback, useEffect, useMemo, useState } from "react";
import Head from "next/head";
import Link from "next/link";
import { useRouter } from "next/router";
import { useAppSelector } from "../../../../../app/hooks";
import API from "../../../../../lib/API";
import { markProjectTreeSubjectGroup } from "../../../../../lib/projectTreeState";
import Loader from "../../../../../components/loader";
import ProgressDonut from "../../../../../components/charts/ProgressDonut";

type FolderDto = {
    id: number;
    name: string;
    path?: string;
};

const statusLabel = (status: number) => {
    if (status === 1) return "Closed";
    if (status === 2) return "On Hold";
    return "Active";
};

const statusClass = (status: number) => {
    if (status === 1) return "bg-slate-200 text-slate-700";
    if (status === 2) return "bg-amber-100 text-amber-800";
    return "bg-emerald-100 text-emerald-800";
};

const LoProgressBar = ({ percent }: { percent: number }) => (
    <div className="flex items-center gap-2 min-w-[120px]">
        <div className="flex-1 h-2 bg-slate-200 rounded overflow-hidden">
            <div
                className="h-full bg-slate-800 rounded transition-all"
                style={{ width: `${Math.min(100, Math.max(0, percent))}%` }}
            />
        </div>
        <span className="text-xs font-medium text-slate-700 w-9 text-right">{percent}%</span>
    </div>
);

const nodeStatusClass = (status: string, isComplete: boolean) => {
    if (isComplete || status === "Complete") return "bg-emerald-100 text-emerald-800 border-emerald-200";
    if (status === "In Progress") return "bg-amber-100 text-amber-800 border-amber-200";
    return "bg-slate-100 text-slate-600 border-slate-200";
};

const StorylineNodeCompletionPanel = ({
    subjects,
}: {
    subjects: SubjectCopyLineageNode[];
}) => (
    <div className="mb-4 border border-slate-200 rounded-lg bg-slate-50 overflow-hidden">
        <div className="px-4 py-2 bg-slate-100 text-xs font-semibold text-slate-600 uppercase tracking-wide border-b border-slate-200">
            Completed schema nodes by subject
        </div>
        <div className="p-4 flex flex-wrap gap-4">
            {subjects.map((subject) => (
                <div
                    key={subject.id}
                    className="min-w-[240px] flex-1 border border-slate-200 rounded-lg bg-white p-3"
                >
                    <div className="font-semibold text-sm text-slate-900 mb-2">{subject.name}</div>
                    {subject.schemaNodes?.length ? (
                        subject.schemaNodes.map((schema) => (
                            <div key={schema.schemaId} className="mb-3 last:mb-0">
                                <div className="text-xs font-medium text-cyan-700 mb-1.5">
                                    {schema.schemaName}
                                </div>
                                <div className="flex flex-col gap-1">
                                    {schema.nodes.map((node) => (
                                        <div
                                            key={node.nodeId}
                                            className={`flex items-center justify-between gap-2 text-xs px-2 py-1 rounded border ${nodeStatusClass(node.status, node.isComplete)}`}
                                        >
                                            <span>{node.nodeName}</span>
                                            <span className="shrink-0 font-medium">
                                                {node.isComplete ? "Done" : node.status}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        ))
                    ) : (
                        <div className="text-xs text-slate-500">No schema nodes tracked.</div>
                    )}
                </div>
            ))}
        </div>
    </div>
);

const taskStatusLabel = (status: TaskStatus) => {
    switch (status) {
        case 1:
            return "To Do";
        case 2:
            return "Doing";
        case 3:
            return "Done";
        case 4:
            return "Roll Back";
        default:
            return "Backlog";
    }
};

const taskStatusClass = (status: TaskStatus, isComplete: boolean) => {
    if (isComplete || status === 3) return "bg-emerald-100 text-emerald-800 border-emerald-200";
    if (status === 2) return "bg-amber-100 text-amber-800 border-amber-200";
    if (status === 4) return "bg-red-100 text-red-800 border-red-200";
    if (status === 1) return "bg-blue-100 text-blue-800 border-blue-200";
    return "bg-slate-100 text-slate-600 border-slate-200";
};

const LoTaskList = ({ tasks }: { tasks: SubjectCopyLineageLoTask[] }) => {
    const completed = tasks.filter((t) => t.isComplete);
    const inProgress = tasks.filter((t) => t.hasTask && !t.isComplete);

    if (tasks.length === 0) {
        return <div className="px-3 py-2 text-xs text-slate-500">No schema steps for this LO.</div>;
    }

    return (
        <div className="px-3 py-2 bg-white border-t border-slate-200">
            <div className="text-[10px] font-semibold text-slate-500 uppercase tracking-wide mb-2">
                Tasks ({completed.length} completed
                {inProgress.length > 0 ? `, ${inProgress.length} in progress` : ""})
            </div>
            <div className="flex flex-col gap-1 max-h-48 overflow-y-auto">
                {tasks.map((task) => (
                    <div
                        key={`${task.order}-${task.stepName}`}
                        className={`flex items-center justify-between gap-2 text-xs px-2 py-1 rounded border ${taskStatusClass(task.status, task.isComplete)}`}
                    >
                        <div className="min-w-0">
                            <span className="font-medium">{task.nodeName}</span>
                            <span className="text-slate-500"> · </span>
                            <span>{task.stepName || "Step"}</span>
                        </div>
                        <span className="shrink-0 font-medium">
                            {task.isComplete
                                ? "Done"
                                : task.hasTask
                                  ? taskStatusLabel(task.status)
                                  : "Not started"}
                        </span>
                    </div>
                ))}
            </div>
        </div>
    );
};

const LoTrackingTable = ({
    subjectId,
    learningObjectives,
}: {
    subjectId: number;
    learningObjectives: SubjectCopyLineageLo[];
}) => {
    const [expandedLoId, setExpandedLoId] = useState<number | null>(null);

    const toggleLo = (loId: number) => {
        setExpandedLoId((prev) => (prev === loId ? null : loId));
    };

    return (
        <div className="border-t border-slate-200">
            <div className="grid grid-cols-[1.2fr_1fr_0.8fr_1fr] gap-2 px-3 py-2 bg-slate-100 text-xs font-semibold text-slate-600 uppercase tracking-wide">
                <div>LO Code</div>
                <div>Title</div>
                <div>Stage</div>
                <div>Progress</div>
            </div>
            {learningObjectives.length === 0 ? (
                <div className="p-3 text-xs text-slate-500">No learning objectives.</div>
            ) : (
                learningObjectives.map((lo) => {
                    const expanded = expandedLoId === lo.id;
                    return (
                        <div key={`${subjectId}-${lo.id}`} className="border-t border-slate-100">
                            <button
                                type="button"
                                onClick={() => toggleLo(lo.id)}
                                className={`w-full grid grid-cols-[1.2fr_1fr_0.8fr_1fr] gap-2 px-3 py-2 text-sm items-center text-left hover:bg-slate-50 transition-colors ${expanded ? "bg-slate-50" : ""}`}
                                aria-expanded={expanded}
                            >
                                <div className="font-mono text-xs text-slate-800 truncate" title={lo.name}>
                                    {lo.name}
                                </div>
                                <div className="text-slate-700 truncate" title={lo.title}>
                                    {lo.title}
                                </div>
                                <div className="text-slate-600 truncate" title={lo.stage}>
                                    {lo.stage}
                                </div>
                                <div className="flex items-center gap-2">
                                    <LoProgressBar percent={lo.progressPercent ?? 0} />
                                    <span className="text-slate-400 text-xs shrink-0">
                                        {expanded ? "▲" : "▼"}
                                    </span>
                                </div>
                            </button>
                            {expanded && <LoTaskList tasks={lo.tasks ?? []} />}
                        </div>
                    );
                })
            )}
        </div>
    );
};

const LineageSubjectNode = ({
    subject,
    rootProjectId,
    expanded,
    onToggle,
}: {
    subject: SubjectCopyLineageNode;
    rootProjectId: number;
    expanded: boolean;
    onToggle: () => void;
}) => {
    const los = subject.learningObjectives ?? [];

    return (
        <div className="min-w-[420px] max-w-[520px] border border-slate-200 rounded-lg bg-slate-50 overflow-hidden">
            <div className="p-4">
                <div className="flex items-start justify-between gap-2 mb-1">
                    <div className="font-semibold text-slate-900 leading-snug">{subject.name}</div>
                    <button
                        type="button"
                        onClick={onToggle}
                        className="shrink-0 text-xs px-2 py-1 rounded border border-slate-300 bg-white hover:bg-slate-100"
                        aria-expanded={expanded}
                    >
                        {expanded ? "Hide LOs" : `LOs (${los.length})`}
                    </button>
                </div>
                <div className="text-xs text-slate-500 mb-2">ID {subject.id}</div>
                <div className="flex items-center justify-between gap-2">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusClass(subject.status)}`}>
                        {statusLabel(subject.status)}
                    </span>
                    <ProgressDonut percentage={subject.progressPercent ?? 0} size={40} />
                </div>
                <Link
                    href={{
                        pathname: `/subjects/${subject.id}`,
                        query: { rootProjectId },
                    }}
                    className="inline-block mt-3 text-xs text-cyan-700 hover:underline"
                >
                    Open subject
                </Link>
            </div>

            {expanded && (
                <LoTrackingTable subjectId={subject.id} learningObjectives={los} />
            )}
        </div>
    );
};

const CopyLineagePage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const rawRoot = router.query.rootProjectId;
    const rawFolder = router.query.folderId;
    const rootProjectId = rawRoot ? Number(Array.isArray(rawRoot) ? rawRoot[0] : rawRoot) : NaN;
    const folderId = rawFolder ? Number(Array.isArray(rawFolder) ? rawFolder[0] : rawFolder) : NaN;

    const [loading, setLoading] = useState(true);
    const [folder, setFolder] = useState<FolderDto | null>(null);
    const [chains, setChains] = useState<SubjectCopyLineageChain[]>([]);
    const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());
    const [expandedChainNodes, setExpandedChainNodes] = useState<Set<number>>(new Set());

    const loadPageData = useCallback(async () => {
        const [folderRes, lineageRes] = await Promise.all([
            API.PROJECTS.ROOT.GET_TERM(folderId, "subjectGroup"),
            API.PROJECTS.GET_COPY_LINEAGE(folderId),
        ]);

        if (folderRes && !folderRes.error && folderRes.data) {
            setFolder(folderRes.data as FolderDto);
        } else {
            setFolder(null);
        }

        if (lineageRes && !lineageRes.error && Array.isArray(lineageRes.data)) {
            setChains(lineageRes.data);
        } else {
            setChains([]);
        }
    }, [folderId]);

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
            return;
        }
        if (!router.isReady || Number.isNaN(folderId)) return;

        setLoading(true);
        (async () => {
            await loadPageData();
            setLoading(false);
        })();
    }, [auth.isAuth, auth.role, router, router.isReady, folderId, loadPageData]);

    useEffect(() => {
        if (Number.isNaN(rootProjectId) || Number.isNaN(folderId)) return;
        markProjectTreeSubjectGroup(rootProjectId, folderId);
    }, [rootProjectId, folderId]);

    const toggleExpanded = (subjectId: number) => {
        setExpandedIds((prev) => {
            const next = new Set(prev);
            if (next.has(subjectId)) next.delete(subjectId);
            else next.add(subjectId);
            return next;
        });
    };

    const toggleChainNodes = (chainIndex: number) => {
        setExpandedChainNodes((prev) => {
            const next = new Set(prev);
            if (next.has(chainIndex)) next.delete(chainIndex);
            else next.add(chainIndex);
            return next;
        });
    };

    const title = useMemo(() => {
        if (folder?.name) return `TMS - Copy Storyline - ${folder.name}`;
        return "TMS - Copy Storyline";
    }, [folder?.name]);

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4) || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || Number.isNaN(folderId)) {
        return <div className="p-6">Invalid folder URL.</div>;
    }

    return (
        <div className="w-full max-h-screen overflow-y-auto">
            <Head>
                <title>{title}</title>
            </Head>
            <div className="bg-white border border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                <div>
                    <h1 className="font-bold text-3xl">Copy Storyline</h1>
                    <p className="text-slate-500 text-sm mt-1">
                        {folder?.path ?? folder?.name ?? "Folder"} — track copy chains and LO node
                        progress
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        type="button"
                        className="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50"
                        onClick={() =>
                            router.push(`/projects/${rootProjectId}/folders/${folderId}`)
                        }
                    >
                        Back to Subjects
                    </button>
                </div>
            </div>

            <div className="p-6">
                {chains.length === 0 ? (
                    <div className="bg-white border border-slate-200 rounded-xl p-8 text-center text-slate-500">
                        No copy lineages in this folder yet. Use <strong>Copy</strong> on a subject
                        to start a storyline.
                    </div>
                ) : (
                    <div className="flex flex-col gap-6">
                        {chains.map((chain, chainIndex) => (
                            <div
                                key={`chain-${chainIndex}`}
                                className="bg-white border border-slate-200 rounded-xl p-5 overflow-x-auto"
                            >
                                <div className="flex items-center justify-between mb-4">
                                    <div className="text-sm text-slate-500">
                                        Storyline {chainIndex + 1}
                                    </div>
                                    <button
                                        type="button"
                                        onClick={() => toggleChainNodes(chainIndex)}
                                        className="text-xs px-3 py-1.5 rounded border border-cyan-600 text-cyan-700 bg-white hover:bg-cyan-50"
                                    >
                                        {expandedChainNodes.has(chainIndex)
                                            ? "Hide completed nodes"
                                            : "Show completed nodes"}
                                    </button>
                                </div>
                                {expandedChainNodes.has(chainIndex) && (
                                    <StorylineNodeCompletionPanel subjects={chain.subjects} />
                                )}
                                <div className="flex items-start gap-3 min-w-max">
                                    {chain.subjects.map((subject, index) => (
                                        <div key={subject.id} className="flex items-start gap-3">
                                            <LineageSubjectNode
                                                subject={subject}
                                                rootProjectId={rootProjectId}
                                                expanded={expandedIds.has(subject.id)}
                                                onToggle={() => toggleExpanded(subject.id)}
                                            />
                                            {index < chain.subjects.length - 1 && (
                                                <div className="flex flex-col items-center text-cyan-600 shrink-0 pt-8">
                                                    <span className="text-2xl">→</span>
                                                    <span className="text-[10px] uppercase tracking-wide">
                                                        copied
                                                    </span>
                                                </div>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default CopyLineagePage;
