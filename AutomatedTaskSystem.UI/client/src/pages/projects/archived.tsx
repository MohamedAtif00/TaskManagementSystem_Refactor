import { useCallback, useEffect, useMemo, useState } from "react";
import Head from "next/head";
import Link from "next/link";
import Loader from "../../components/loader";
import API from "../../lib/API";
import {
    ArchivedCurriculumTreeNode,
    ArchivedSubjectSummary,
    CurriculumNodeType,
    curriculumLevelLabel,
} from "../../lib/curriculumHierarchy";

const FolderIcon = ({ muted = false }: { muted?: boolean }) => (
    <svg
        width="16"
        height="16"
        viewBox="0 0 16 16"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
        className={muted ? "opacity-50" : undefined}
    >
        <rect x="1" y="1" width="7" height="3" rx="1.5" fill={muted ? "#94A3B8" : "#10B981"} />
        <rect x="1" y="5" width="14" height="10" rx="2" fill={muted ? "#94A3B8" : "#10B981"} />
    </svg>
);

const levelLabelForNode = (node: ArchivedCurriculumTreeNode) => {
    if (node.nodeType === "year") return curriculumLevelLabel(0);
    if (node.nodeType === "project") return curriculumLevelLabel(1);
    if (node.nodeType === "term") return curriculumLevelLabel(2);
    return curriculumLevelLabel(3);
};

const collectSubjectsInBranch = (node: ArchivedCurriculumTreeNode): ArchivedSubjectSummary[] => [
    ...node.subjects,
    ...node.children.flatMap(collectSubjectsInBranch),
];

type RestoreModalState = {
    nodeType: CurriculumNodeType;
    id: number;
    name: string;
    label: string;
    subjects: ArchivedSubjectSummary[];
};

const collectExpandedKeys = (nodes: ArchivedCurriculumTreeNode[]): string[] =>
    nodes.flatMap((node) => [node.path, ...collectExpandedKeys(node.children)]);

const ArchivedProjects = () => {
    const [isLoading, setIsLoading] = useState(true);
    const [forest, setForest] = useState<ArchivedCurriculumTreeNode[]>([]);
    const [expandedKeys, setExpandedKeys] = useState<Set<string>>(new Set());
    const [busyKey, setBusyKey] = useState<string | null>(null);
    const [restoreModal, setRestoreModal] = useState<RestoreModalState | null>(null);
    const [selectedSubjectIds, setSelectedSubjectIds] = useState<Set<number>>(new Set());
    const [errorMessage, setErrorMessage] = useState("");

    const nodeKey = (node: ArchivedCurriculumTreeNode) => node.path;

    const loadArchived = useCallback(async () => {
        setIsLoading(true);
        setErrorMessage("");
        try {
            const res = await API.PROJECTS.ROOT.ARCHIVED_LIST();
            const items: ArchivedCurriculumTreeNode[] =
                res && !res.error && Array.isArray(res.data) ? res.data : [];
            setForest(items);
            setExpandedKeys(new Set(collectExpandedKeys(items)));
        } catch {
            setErrorMessage("Failed to load archived folders.");
            setForest([]);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        void loadArchived();
    }, [loadArchived]);

    const toggleExpanded = (key: string) => {
        setExpandedKeys((prev) => {
            const next = new Set(prev);
            if (next.has(key)) next.delete(key);
            else next.add(key);
            return next;
        });
    };

    const openRestoreModal = (node: ArchivedCurriculumTreeNode) => {
        const subjects = collectSubjectsInBranch(node);
        setRestoreModal({
            nodeType: node.nodeType,
            id: node.id,
            name: node.name,
            label: levelLabelForNode(node),
            subjects,
        });
        setSelectedSubjectIds(new Set(subjects.map((s) => s.id)));
    };

    const closeRestoreModal = () => {
        if (busyKey) return;
        setRestoreModal(null);
        setSelectedSubjectIds(new Set());
    };

    const toggleSubject = (subjectId: number) => {
        setSelectedSubjectIds((prev) => {
            const next = new Set(prev);
            if (next.has(subjectId)) next.delete(subjectId);
            else next.add(subjectId);
            return next;
        });
    };

    const allSubjectsSelected = useMemo(() => {
        if (!restoreModal || restoreModal.subjects.length === 0) return true;
        return restoreModal.subjects.every((s) => selectedSubjectIds.has(s.id));
    }, [restoreModal, selectedSubjectIds]);

    const confirmRestore = async () => {
        if (!restoreModal) return;
        if (restoreModal.subjects.length > 0 && selectedSubjectIds.size === 0) {
            setErrorMessage("Select at least one subject to restore.");
            return;
        }
        const key = `${restoreModal.nodeType}-${restoreModal.id}`;
        setBusyKey(key);
        setErrorMessage("");
        try {
            const subjectIds = Array.from(selectedSubjectIds);
            const res = await API.PROJECTS.ROOT.RESTORE(
                restoreModal.nodeType,
                restoreModal.id,
                subjectIds
            );
            if (res && !res.error) {
                await loadArchived();
                setRestoreModal(null);
                setSelectedSubjectIds(new Set());
            } else {
                setErrorMessage(res?.message ?? "Could not restore folder.");
            }
        } finally {
            setBusyKey(null);
        }
    };

    const renderNode = (node: ArchivedCurriculumTreeNode, depth: number, parentArchived: boolean) => {
        const key = nodeKey(node);
        const isExpanded = expandedKeys.has(key);
        const hasChildren = node.children.length > 0;
        const hasSubjects = node.subjects.length > 0;
        const showRestore =
            (node.archived || node.subjects.length > 0) && !parentArchived;
        const isBusy = busyKey === key;
        const canExpand = hasChildren || hasSubjects;

        return (
            <div key={key} className="mb-1">
                <div
                    className={`flex items-center gap-2 rounded-lg px-2 py-2 ${
                        node.archived ? "bg-amber-50/60 border border-amber-100" : "bg-slate-50"
                    }`}
                    style={{ marginLeft: `${depth * 16}px` }}
                >
                    {canExpand ? (
                        <button
                            type="button"
                            className="shrink-0 w-5 h-5 text-slate-500 hover:text-slate-800"
                            onClick={() => toggleExpanded(key)}
                            aria-label={isExpanded ? "Collapse" : "Expand"}
                        >
                            {isExpanded ? "▾" : "▸"}
                        </button>
                    ) : (
                        <span className="w-5 shrink-0" />
                    )}
                    <FolderIcon muted={!node.archived} />
                    <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                            <span
                                className={`font-medium truncate ${
                                    node.archived ? "text-slate-900" : "text-slate-500"
                                }`}
                            >
                                {node.name}
                            </span>
                            <span className="text-xs text-slate-500 shrink-0">
                                {levelLabelForNode(node)}
                            </span>
                            {node.archived && (
                                <span className="text-xs rounded-full bg-amber-100 text-amber-800 px-2 py-0.5 shrink-0">
                                    Archived
                                </span>
                            )}
                        </div>
                    </div>
                    {showRestore && (
                        <button
                            type="button"
                            className="shrink-0 text-xs rounded-lg bg-green-600 text-white px-3 py-1.5 hover:bg-green-700 disabled:opacity-50"
                            disabled={isBusy}
                            onClick={() => openRestoreModal(node)}
                        >
                            {isBusy ? "Restoring..." : "Restore"}
                        </button>
                    )}
                </div>

                {isExpanded && hasSubjects && (
                    <ul
                        className="mt-1 mb-2 space-y-1 border-l-2 border-slate-200"
                        style={{ marginLeft: `${depth * 16 + 28}px`, paddingLeft: "12px" }}
                    >
                        {node.subjects.map((subject) => (
                            <li
                                key={subject.id}
                                className="text-sm text-slate-700 flex items-center gap-2 py-0.5"
                            >
                                <span className="text-slate-400">•</span>
                                <span className="truncate">{subject.name}</span>
                                <span className="text-xs text-slate-400 shrink-0">Subject</span>
                            </li>
                        ))}
                    </ul>
                )}

                {isExpanded &&
                    hasChildren &&
                    node.children.map((child) =>
                        renderNode(child, depth + 1, node.archived || parentArchived)
                    )}
            </div>
        );
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full min-h-[50vh]">
                <Head>
                    <title>TMS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    return (
        <>
            <Head>
                <title>TMS - Archived Folders</title>
            </Head>
            <div className="w-full max-h-screen overflow-y-auto">
                <div className="bg-white border border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                    <h1 className="font-bold text-3xl">Archived Folders</h1>
                    <Link href="/projects">
                        <button
                            type="button"
                            className="px-5 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700"
                        >
                            Back to Projects
                        </button>
                    </Link>
                </div>

                <div className="p-6">
                    {errorMessage && (
                        <p className="mb-4 text-sm text-red-600" role="alert">
                            {errorMessage}
                        </p>
                    )}
                    <div className="bg-white border border-slate-200 rounded-xl p-4 min-h-[200px]">
                        {forest.length === 0 ? (
                            <p className="text-slate-500 py-8 text-center">No archived folders found.</p>
                        ) : (
                            <div className="min-w-0">{forest.map((root) => renderNode(root, 0, false))}</div>
                        )}
                    </div>
                </div>
            </div>

            {restoreModal && (
                <div
                    className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                    role="dialog"
                    aria-modal="true"
                    onClick={busyKey ? undefined : closeRestoreModal}
                >
                    <div
                        className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <h2 className="text-xl font-bold text-slate-900">
                            Restore {restoreModal.label}?
                        </h2>
                        <p className="mt-2 text-sm text-slate-600">
                            Restore &quot;{restoreModal.name}&quot; and choose which subjects to bring
                            back. Unselected subjects stay archived.
                        </p>

                        {restoreModal.subjects.length > 0 ? (
                            <div className="mt-4">
                                <div className="mb-2 flex items-center justify-between">
                                    <span className="text-sm font-medium text-slate-700">Subjects</span>
                                    <button
                                        type="button"
                                        className="text-xs text-blue-600 hover:text-blue-800"
                                        onClick={() =>
                                            setSelectedSubjectIds(
                                                allSubjectsSelected
                                                    ? new Set()
                                                    : new Set(restoreModal.subjects.map((s) => s.id))
                                            )
                                        }
                                    >
                                        {allSubjectsSelected ? "Deselect all" : "Select all"}
                                    </button>
                                </div>
                                <ul className="max-h-56 overflow-y-auto rounded-lg border border-slate-200 divide-y">
                                    {restoreModal.subjects.map((subject) => (
                                        <li key={subject.id}>
                                            <label className="flex items-center gap-3 px-3 py-2 text-sm cursor-pointer hover:bg-slate-50">
                                                <input
                                                    type="checkbox"
                                                    checked={selectedSubjectIds.has(subject.id)}
                                                    onChange={() => toggleSubject(subject.id)}
                                                />
                                                <span className="text-slate-800">{subject.name}</span>
                                            </label>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        ) : (
                            <p className="mt-4 text-sm text-slate-500">
                                This folder has no subjects to restore. The folder structure will be
                                restored empty.
                            </p>
                        )}

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                type="button"
                                className="rounded-lg border border-slate-300 px-4 py-2 text-slate-700 hover:bg-slate-50"
                                onClick={closeRestoreModal}
                                disabled={busyKey !== null}
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                className="rounded-lg bg-green-600 px-4 py-2 text-white hover:bg-green-700 disabled:opacity-60"
                                onClick={() => void confirmRestore()}
                                disabled={busyKey !== null}
                            >
                                {busyKey ? "Restoring..." : "Restore"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
};

export default ArchivedProjects;
