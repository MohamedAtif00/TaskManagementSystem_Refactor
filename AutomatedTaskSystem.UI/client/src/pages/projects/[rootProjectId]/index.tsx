import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/router";
import { useAppSelector } from "../../../app/hooks";
import API from "../../../lib/API";
import Loader from "../../../components/loader";

type FolderDto = {
    id: number;
    name: string;
    parentFolderId?: number | null;
    level?: number;
    path?: string;
    levelNames?: string[];
};

const levelStorageKey = (rootId: number) => `project-level-names:${rootId}`;

const cleanLevelNames = (levels: unknown): string[] => {
    if (!Array.isArray(levels)) return [];
    return levels
        .map((x) => String(x ?? "").trim())
        .filter((x) => x.length > 0);
};

const singularLevelName = (levelName: string) =>
    levelName.endsWith("s") ? levelName.slice(0, -1) : levelName;

const addActionColor = (depth: number) =>
    ["text-blue-600 hover:text-blue-700", "text-purple-600 hover:text-purple-700", "text-emerald-600 hover:text-emerald-700"][
        depth
    ] ?? "text-blue-600 hover:text-blue-700";

const FolderIcon = () => (
    <svg
        width="16"
        height="16"
        viewBox="0 0 16 16"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
    >
        <rect x="1" y="1" width="7" height="3" rx="1.5" fill="#10B981" />
        <rect x="1" y="5" width="14" height="10" rx="2" fill="#10B981" />
    </svg>
);

const ProjectTreePage = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();
    const raw = router.query.rootProjectId;
    const rootProjectId = raw ? Number(Array.isArray(raw) ? raw[0] : raw) : NaN;

    const [loading, setLoading] = useState(true);
    const [rootFolder, setRootFolder] = useState<FolderDto | null>(null);
    const [folderById, setFolderById] = useState<Record<number, FolderDto>>({});
    const [parentById, setParentById] = useState<Record<number, number | null>>({});
    const [childrenByParent, setChildrenByParent] = useState<Record<number, FolderDto[]>>({});
    const [subjectCountByFolder, setSubjectCountByFolder] = useState<Record<number, number>>({});
    const [levelNames, setLevelNames] = useState<string[]>([]);
    const [selectedFolderId, setSelectedFolderId] = useState<number | null>(null);
    const [expandedFolderIds, setExpandedFolderIds] = useState<Set<number>>(new Set());
    const [addModal, setAddModal] = useState<{ parentFolderId: number; label: string } | null>(null);
    const [addName, setAddName] = useState("");
    const [addSubmitting, setAddSubmitting] = useState(false);

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    const loadChildren = async (parentId: number) => {
        const res = await API.PROJECTS.ROOT.TERMS(parentId);
        if (!res || res.error || !Array.isArray(res.data)) return;
        const children = (res.data as FolderDto[]).sort((a, b) => a.name.localeCompare(b.name));
        setChildrenByParent((prev) => ({ ...prev, [parentId]: children }));
        setFolderById((prev) => {
            const next = { ...prev };
            for (const child of children) next[child.id] = child;
            return next;
        });
        setParentById((prev) => {
            const next = { ...prev };
            for (const child of children) next[child.id] = parentId;
            return next;
        });
    };

    const loadDescendants = async (rootId: number, root: FolderDto) => {
        const nextChildrenByParent: Record<number, FolderDto[]> = {};
        const nextFolderById: Record<number, FolderDto> = { [root.id]: root };
        const nextParentById: Record<number, number | null> = { [root.id]: null };
        const queue = [rootId];

        while (queue.length > 0) {
            const parentId = queue.shift()!;
            const res = await API.PROJECTS.ROOT.TERMS(parentId);
            const children =
                res && !res.error && Array.isArray(res.data)
                    ? (res.data as FolderDto[]).sort((a, b) => a.name.localeCompare(b.name))
                    : [];

            nextChildrenByParent[parentId] = children;
            for (const child of children) {
                nextFolderById[child.id] = child;
                nextParentById[child.id] = parentId;
                queue.push(child.id);
            }
        }

        setChildrenByParent(nextChildrenByParent);
        setFolderById(nextFolderById);
        setParentById(nextParentById);
    };

    useEffect(() => {
        if (!router.isReady || Number.isNaN(rootProjectId)) return;
        setLoading(true);
        (async () => {
            const rootRes = await API.PROJECTS.ROOT.GET(rootProjectId);
            if (!rootRes || rootRes.error || !rootRes.data) {
                setLoading(false);
                return;
            }
            const root = rootRes.data as FolderDto;
            setRootFolder(root);
            if (Array.isArray(root.levelNames) && root.levelNames.length > 0) {
                setLevelNames(cleanLevelNames(root.levelNames));
            }
            setFolderById({ [root.id]: root });
            setParentById({ [root.id]: null });
            setSelectedFolderId(root.id);
            await loadDescendants(root.id, root);
            setExpandedFolderIds(new Set([root.id]));

            const allSubjects = await API.PROJECTS.GET_ALL();
            if (allSubjects && !allSubjects.error && Array.isArray(allSubjects.data)) {
                const counts: Record<number, number> = {};
                for (const s of allSubjects.data as any[]) {
                    if (s.status !== 0 && s.status !== 3) continue;
                    const folderId = Number(s.folderId);
                    if (!Number.isNaN(folderId)) counts[folderId] = (counts[folderId] ?? 0) + 1;
                }
                setSubjectCountByFolder(counts);
            }

            if ((!Array.isArray(root.levelNames) || root.levelNames.length === 0) && typeof window !== "undefined") {
                const rawLevels = window.localStorage.getItem(levelStorageKey(rootProjectId));
                if (rawLevels) {
                    try {
                        const parsed = JSON.parse(rawLevels);
                        if (Array.isArray(parsed) && parsed.length > 0) {
                            setLevelNames(cleanLevelNames(parsed));
                        }
                    } catch {
                        // ignore corrupt local storage value
                    }
                }
            }

            setLoading(false);
        })();
    }, [router.isReady, rootProjectId]);

    const goToFolder = async (folder: FolderDto) => {
        setSelectedFolderId(folder.id);
        let children = childrenByParent[folder.id];
        if (children === undefined) {
            const res = await API.PROJECTS.ROOT.TERMS(folder.id);
            if (res && !res.error && Array.isArray(res.data)) {
                children = (res.data as FolderDto[]).sort((a, b) => a.name.localeCompare(b.name));
                setChildrenByParent((prev) => ({ ...prev, [folder.id]: children! }));
                setFolderById((prev) => {
                    const next = { ...prev };
                    for (const child of children!) next[child.id] = child;
                    return next;
                });
                setParentById((prev) => {
                    const next = { ...prev };
                    for (const child of children!) next[child.id] = folder.id;
                    return next;
                });
            } else {
                children = [];
                setChildrenByParent((prev) => ({ ...prev, [folder.id]: [] }));
            }
        }

        let folderDepth = 0;
        let current = folder.id;
        while (parentById[current] != null) {
            const parentId = parentById[current];
            if (!parentId) break;
            folderDepth += 1;
            current = parentId;
        }

        if ((children ?? []).length === 0 && folderDepth >= levelNames.length) {
            router.push(`/projects/${rootProjectId}/folders/${folder.id}`);
            return;
        }

        setExpandedFolderIds((prev) => {
            const next = new Set(prev);
            if (next.has(folder.id)) {
                next.delete(folder.id);
            } else {
                next.add(folder.id);
            }
            return next;
        });
    };

    const selectedDepth = useMemo(() => {
        if (!selectedFolderId) return 0;
        let depth = 0;
        let current = selectedFolderId;
        while (parentById[current] != null) {
            const parentId = parentById[current];
            if (!parentId) break;
            depth += 1;
            current = parentId;
        }
        return depth;
    }, [parentById, selectedFolderId]);

    const selectedFolder = selectedFolderId ? folderById[selectedFolderId] ?? null : null;
    const selectedPathIds = useMemo(() => {
        const ids = new Set<number>();
        if (!selectedFolderId) return ids;
        let current: number | null = selectedFolderId;
        while (current != null) {
            ids.add(current);
            current = parentById[current] ?? null;
        }
        return ids;
    }, [parentById, selectedFolderId]);

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4) || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || !rootFolder) {
        return <div className="p-6">Invalid project.</div>;
    }

    const openAddModal = (parentFolderId: number, label: string) => {
        setAddName("");
        setAddModal({ parentFolderId, label });
    };

    const closeAddModal = () => {
        if (addSubmitting) return;
        setAddModal(null);
        setAddName("");
    };

    const addFolder = async () => {
        if (!addModal || !addName.trim()) return;
        setAddSubmitting(true);
        try {
            const parentFolderId = addModal.parentFolderId;
            const res = await API.PROJECTS.ROOT.CREATE_TERM(parentFolderId, {
                name: addName.trim(),
            });
            if (res && !res.error) {
                await loadChildren(parentFolderId);
                setExpandedFolderIds((prev) => new Set(prev).add(parentFolderId));
                setAddModal(null);
                setAddName("");
            }
        } finally {
            setAddSubmitting(false);
        }
    };

    const renderEmptyLevelScaffold = (parentFolderId: number, depth: number, canAddAtThisLevel = true) => {
        if (depth >= levelNames.length) return null;

        const levelName = levelNames[depth]?.trim();
        if (!levelName) return null;

        const singularName = singularLevelName(levelName);

        return (
            <div
                key={`empty-${parentFolderId}-${depth}`}
                className="mt-2 border-l-2 border-slate-200 pl-4"
                style={{ marginLeft: `${depth === 0 ? 24 : 14}px` }}
            >
                <div className="text-xs uppercase tracking-wide text-slate-400 mb-1">
                    {levelName}
                </div>
                {renderEmptyLevelScaffold(parentFolderId, depth + 1, false)}
                <button
                    type="button"
                    disabled={!canAddAtThisLevel}
                    title={!canAddAtThisLevel ? `Add ${singularName} after creating the parent level` : undefined}
                    className={`text-sm mt-1 ${
                        canAddAtThisLevel
                            ? addActionColor(depth)
                            : "text-slate-300 cursor-not-allowed"
                    }`}
                    onClick={async (e) => {
                        e.stopPropagation();
                        if (!canAddAtThisLevel) return;
                        openAddModal(parentFolderId, singularName);
                    }}
                >
                    + Add {singularName}
                </button>
            </div>
        );
    };

    const renderNode = (folder: FolderDto, depth: number) => {
        const children = childrenByParent[folder.id] ?? [];
        const badge = children.length > 0 ? children.length : subjectCountByFolder[folder.id] ?? 0;
        const isSelected = selectedFolderId === folder.id;
        const isInSelectedPath = selectedPathIds.has(folder.id);
        const isExpanded = expandedFolderIds.has(folder.id);
        const childLevelName = levelNames[depth] ?? `Level ${depth + 1}`;
        const childSingularName = singularLevelName(childLevelName);
        const canAddChild = depth < levelNames.length;
        return (
            <div key={folder.id} className="mb-2">
                <button
                    type="button"
                    aria-current={isSelected ? "true" : "false"}
                    className={`w-full text-left rounded px-2 py-2 flex items-center justify-between ${
                        isSelected
                            ? "bg-blue-200 text-blue-900 border border-blue-400 font-semibold"
                            : isInSelectedPath
                            ? "bg-blue-50 text-blue-700"
                            : "hover:bg-slate-100"
                    }`}
                    style={{ paddingLeft: `${depth * 14 + 10}px` }}
                    onClick={async () => {
                        await goToFolder(folder);
                    }}
                >
                    <span className="flex items-center gap-2">
                        <span className="shrink-0">
                            <FolderIcon />
                        </span>
                        <span>{folder.name}</span>
                    </span>
                    <span className="text-xs bg-slate-200 rounded-full px-2 py-0.5">{badge}</span>
                </button>
                {isExpanded && children.length > 0 && canAddChild && (
                    <div
                        className="mt-2 border-l-2 border-slate-200 pl-4"
                        style={{ marginLeft: `${depth === 0 ? 24 : 14}px` }}
                    >
                        <div className="text-xs uppercase tracking-wide text-slate-400 mb-1">
                            {childLevelName}
                        </div>
                        {children.map((child) => renderNode(child, depth + 1))}
                        <button
                            type="button"
                            className="text-sm text-blue-600 hover:text-blue-700 mt-1"
                            onClick={async (e) => {
                                e.stopPropagation();
                                openAddModal(folder.id, childSingularName || "Folder");
                            }}
                        >
                            + Add {childSingularName}
                        </button>
                    </div>
                )}
                {isExpanded && children.length === 0 && canAddChild && renderEmptyLevelScaffold(folder.id, depth)}
            </div>
        );
    };

    return (
        <div className="w-full max-h-screen overflow-y-auto">
            <div className="bg-white border w-full border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                <h1 className="font-bold text-3xl">{rootFolder.name}</h1>
                <button
                    type="button"
                    className="px-4 py-2 border border-slate-300 rounded-lg"
                    onClick={() => router.push("/projects")}
                >
                    Back to Projects
                </button>
            </div>
            <div className="p-6 w-full">
                <div className="bg-white border w-full border-slate-200 rounded-xl overflow-hidden">
                    <div className="grid grid-cols-12 min-h-[560px]">
                        <div className="col-span-4 border-r bg-slate-50 p-4 overflow-auto">
                            <div className="min-w-max pr-4">
                                {renderNode(rootFolder, 0)}
                            </div>
                        </div>
                        <div className="col-span-8 p-6">
                            <h2 className="text-2xl font-semibold mb-2">{selectedFolder?.name ?? rootFolder.name}</h2>
                            <p className="text-slate-500">
                                Level: {selectedDepth === 0 ? "Project" : levelNames[selectedDepth - 1] ?? `Level ${selectedDepth}`}
                            </p>
                            <p className="text-slate-500 mt-2">
                                Children: {selectedFolder ? (childrenByParent[selectedFolder.id]?.length ?? 0) : 0}
                            </p>

                            <div className="mt-6 border border-slate-200 rounded-lg overflow-hidden">
                                <div className="px-4 py-3 bg-slate-50 border-b border-slate-200 font-medium">
                                    Next nested folders
                                </div>
                                {!selectedFolder ? (
                                    <div className="p-4 text-slate-500">Select a folder from the tree.</div>
                                ) : (childrenByParent[selectedFolder.id] ?? []).length === 0 ? (
                                    <div className="p-4 text-slate-500">
                                        This is the last nested folder. Opening subjects list...
                                    </div>
                                ) : (
                                    <div className="divide-y divide-slate-100">
                                        {(childrenByParent[selectedFolder.id] ?? []).map((child) => (
                                            <button
                                                key={child.id}
                                                type="button"
                                                className="w-full px-4 py-3 text-left hover:bg-slate-50 flex items-center justify-between"
                                                onClick={() => goToFolder(child)}
                                            >
                                                <span className="flex items-center gap-2">
                                                    <FolderIcon />
                                                    <span className="font-medium">{child.name}</span>
                                                </span>
                                                <span className="text-sm text-slate-500">Open</span>
                                            </button>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            {addModal && (
                <div
                    className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="add-folder-title"
                    onClick={closeAddModal}
                >
                    <form
                        className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
                        onClick={(e) => e.stopPropagation()}
                        onSubmit={async (e) => {
                            e.preventDefault();
                            await addFolder();
                        }}
                    >
                        <div className="flex items-start justify-between gap-4">
                            <div>
                                <h2 id="add-folder-title" className="text-xl font-bold text-slate-900">
                                    Add {addModal.label}
                                </h2>
                                <p className="mt-1 text-sm text-slate-500">
                                    Create a new {addModal.label.toLowerCase()} in this hierarchy.
                                </p>
                            </div>
                            <button
                                type="button"
                                className="rounded-md px-2 py-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                                onClick={closeAddModal}
                                aria-label="Close"
                            >
                                x
                            </button>
                        </div>

                        <label className="mt-5 block text-sm font-semibold text-slate-700">
                            {addModal.label} Name
                        </label>
                        <input
                            autoFocus
                            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                            placeholder={`Enter ${addModal.label} name`}
                            value={addName}
                            onChange={(e) => setAddName(e.target.value)}
                        />

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                type="button"
                                className="rounded-lg border border-slate-300 px-4 py-2 text-slate-700 hover:bg-slate-50"
                                onClick={closeAddModal}
                                disabled={addSubmitting}
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-60"
                                disabled={addSubmitting || !addName.trim()}
                            >
                                {addSubmitting ? "Saving..." : `Add ${addModal.label}`}
                            </button>
                        </div>
                    </form>
                </div>
            )}
        </div>
    );
};

export default ProjectTreePage;
