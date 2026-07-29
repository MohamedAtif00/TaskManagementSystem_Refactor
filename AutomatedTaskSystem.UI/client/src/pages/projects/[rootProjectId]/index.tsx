import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/router";
import Link from "next/link";
import { useAppSelector } from "../../../app/hooks";
import API from "../../../lib/API";
import Loader from "../../../components/loader";
import ConfirmDeleteModal from "../../../components/ConfirmDeleteModal";
import { CURRICULUM_CHILD_LEVELS, curriculumLevelLabel } from "../../../lib/curriculumHierarchy";
import {
    loadProjectTreeState,
    persistProjectTreeUiState,
    restoreProjectTreeUiState,
} from "../../../lib/projectTreeState";

type TreeNode = {
    /** Unique across levels (ids can collide between tables). */
    key: number;
    /** Real database id, used for API calls and navigation. */
    id: number;
    name: string;
    level: number;
    path?: string;
    nodeType?: string;
    parentKey: number | null;
};

const FOLDER_LEVEL_COUNT = CURRICULUM_CHILD_LEVELS.length;

const nodeKey = (level: number, id: number) => level * 10_000_000 + id;

const parentTypeForDepth = (depth: number): "year" | "project" | "term" => {
    if (depth <= 0) return "year";
    if (depth === 1) return "project";
    return "term";
};

const deleteNodeTypeForLevel = (level: number): "year" | "project" | "term" | "subjectGroup" => {
    if (level <= 0) return "year";
    if (level <= 1) return "project";
    if (level === 2) return "term";
    return "subjectGroup";
};

const nodeTypeForLevel = (level: number): "project" | "term" | "subjectGroup" => {
    if (level <= 1) return "project";
    if (level === 2) return "term";
    return "subjectGroup";
};

const EditIcon = () => (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
        <path
            d="M0 12.7928V15.0858C0 15.2184 0.0526785 15.3456 0.146447 15.4393C0.240215 15.5331 0.367392 15.5858 0.5 15.5858H2.798C2.93035 15.5858 3.05729 15.5333 3.151 15.4398L12.599 5.99179L9.599 2.99179L0.147 12.4398C0.0531646 12.5333 0.000293383 12.6603 0 12.7928ZM10.837 1.75279L13.837 4.75279L15.297 3.29279C15.4845 3.10526 15.5898 2.85095 15.5898 2.58579C15.5898 2.32062 15.4845 2.06631 15.297 1.87879L13.712 0.292786C13.5245 0.105315 13.2702 0 13.005 0C12.7398 0 12.4855 0.105315 12.298 0.292786L10.837 1.75279Z"
            fill="currentColor"
        />
    </svg>
);

const TrashIcon = () => (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
        <path
            d="M5.5 1.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1V2h3.25a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1 0-1.5H5.5V1.5zM3.5 4.5v8.75A2.25 2.25 0 0 0 5.75 15.5h4.5a2.25 2.25 0 0 0 2.25-2.25V4.5H3.5zm2.25 1.5a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75zm3.5 0a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75z"
            fill="currentColor"
        />
    </svg>
);

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
    const [rootNode, setRootNode] = useState<TreeNode | null>(null);
    const [nodeByKey, setNodeByKey] = useState<Record<number, TreeNode>>({});
    const [childrenByParentKey, setChildrenByParentKey] = useState<Record<number, TreeNode[]>>({});
    const [subjectCountByGroupId, setSubjectCountByGroupId] = useState<Record<number, number>>({});
    const [selectedKey, setSelectedKey] = useState<number | null>(null);
    const [expandedKeys, setExpandedKeys] = useState<Set<number>>(new Set());
    const [addModal, setAddModal] = useState<{ parentKey: number; label: string } | null>(null);
    const [addName, setAddName] = useState("");
    const [addSubmitting, setAddSubmitting] = useState(false);
    const [editModal, setEditModal] = useState<{ nodeKey: number; label: string; isYear?: boolean } | null>(null);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [editSubmitting, setEditSubmitting] = useState(false);
    const [deleteSubmitting, setDeleteSubmitting] = useState(false);
    const [deleteModal, setDeleteModal] = useState<{
        nodeKey: number;
        label: string;
        name: string;
        description: string;
    } | null>(null);
    const [actionError, setActionError] = useState<string | null>(null);
    const treeScrollRef = useRef<HTMLDivElement | null>(null);
    const skipPersistRef = useRef(true);

    const refreshSubjectCounts = async () => {
        const allSubjects = await API.PROJECTS.GET_ALL();
        if (allSubjects && !allSubjects.error && Array.isArray(allSubjects.data)) {
            const counts: Record<number, number> = {};
            for (const s of allSubjects.data as any[]) {
                if (s.status !== 0 && s.status !== 3) continue;
                const groupId = Number(s.folderId);
                if (!Number.isNaN(groupId)) counts[groupId] = (counts[groupId] ?? 0) + 1;
            }
            setSubjectCountByGroupId(counts);
        }
    };

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
        }
    }, [auth.isAuth, auth.role, router]);

    /** Loads the whole year hierarchy with a single request. */
    const loadTree = async (root: TreeNode) => {
        const res = await API.PROJECTS.ROOT.TREE(root.id);
        const flat: any[] = res && !res.error && Array.isArray(res.data) ? res.data : [];

        const nextNodeByKey: Record<number, TreeNode> = { [root.key]: root };
        const nextChildren: Record<number, TreeNode[]> = { [root.key]: [] };

        const nodes = flat
            .map((n) => ({
                key: nodeKey(n.depth, n.id),
                id: n.id,
                name: String(n.name ?? ""),
                level: Number(n.depth ?? 0),
                path: n.path,
                nodeType: n.nodeType,
                parentKey:
                    n.depth === 1
                        ? root.key
                        : n.parentId != null
                          ? nodeKey(n.depth - 1, n.parentId)
                          : null,
            }))
            .sort((a, b) => a.level - b.level || a.name.localeCompare(b.name));

        for (const node of nodes) {
            nextNodeByKey[node.key] = node;
            if (node.level < FOLDER_LEVEL_COUNT) nextChildren[node.key] ??= [];
            if (node.parentKey != null) {
                nextChildren[node.parentKey] ??= [];
                nextChildren[node.parentKey].push(node);
            }
        }

        setNodeByKey(nextNodeByKey);
        setChildrenByParentKey(nextChildren);
        return nextNodeByKey;
    };

    useEffect(() => {
        if (!router.isReady || Number.isNaN(rootProjectId)) return;
        skipPersistRef.current = true;
        setLoading(true);
        (async () => {
            const rootRes = await API.PROJECTS.ROOT.GET(rootProjectId);
            if (!rootRes || rootRes.error || !rootRes.data) {
                setLoading(false);
                return;
            }
            const root: TreeNode = {
                key: nodeKey(0, rootRes.data.id),
                id: rootRes.data.id,
                name: rootRes.data.name,
                level: 0,
                path: rootRes.data.path,
                nodeType: "year",
                parentKey: null,
            };
            setRootNode(root);
            const nodeMap = await loadTree(root);
            const restored = restoreProjectTreeUiState(rootProjectId, nodeMap);
            setSelectedKey(restored.selectedKey);
            setExpandedKeys(restored.expandedKeys);
            await refreshSubjectCounts();
            setLoading(false);
            skipPersistRef.current = false;
            requestAnimationFrame(() => {
                const savedScroll = loadProjectTreeState(rootProjectId)?.scrollTop;
                if (treeScrollRef.current && typeof savedScroll === "number") {
                    treeScrollRef.current.scrollTop = savedScroll;
                }
            });
        })();
    }, [router.isReady, rootProjectId]);

    useEffect(() => {
        if (skipPersistRef.current || loading || !rootNode || Number.isNaN(rootProjectId)) return;
        persistProjectTreeUiState(
            rootProjectId,
            selectedKey,
            expandedKeys,
            nodeByKey,
            treeScrollRef.current?.scrollTop
        );
    }, [selectedKey, expandedKeys, nodeByKey, rootProjectId, loading, rootNode]);

    const goToFolder = (node: TreeNode) => {
        setSelectedKey(node.key);
        const children = childrenByParentKey[node.key] ?? [];

        if (children.length === 0 && node.level >= FOLDER_LEVEL_COUNT) {
            const nextExpanded = new Set(expandedKeys);
            let current: TreeNode | null = node;
            while (current) {
                nextExpanded.add(current.key);
                current = current.parentKey != null ? nodeByKey[current.parentKey] ?? null : null;
            }
            persistProjectTreeUiState(
                rootProjectId,
                node.key,
                nextExpanded,
                nodeByKey,
                treeScrollRef.current?.scrollTop
            );
            router.push(`/projects/${rootProjectId}/folders/${node.id}`);
            return;
        }

        setExpandedKeys((prev) => {
            const next = new Set(prev);
            if (next.has(node.key)) {
                next.delete(node.key);
            } else {
                next.add(node.key);
            }
            return next;
        });
    };

    const selectedNode = selectedKey != null ? nodeByKey[selectedKey] ?? null : null;
    const selectedDepth = selectedNode?.level ?? 0;
    const selectedPathKeys = useMemo(() => {
        const keys = new Set<number>();
        let current = selectedNode;
        while (current) {
            keys.add(current.key);
            current = current.parentKey != null ? nodeByKey[current.parentKey] ?? null : null;
        }
        return keys;
    }, [nodeByKey, selectedNode]);

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4) || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (Number.isNaN(rootProjectId) || !rootNode) {
        return <div className="p-6">Invalid project.</div>;
    }

    const openAddModal = (parentKey: number, label: string) => {
        setAddName("");
        setAddModal({ parentKey, label });
    };

    const closeAddModal = () => {
        if (addSubmitting) return;
        setAddModal(null);
        setAddName("");
    };

    const addFolder = async () => {
        if (!addModal || !addName.trim()) return;
        const parent = nodeByKey[addModal.parentKey];
        if (!parent) return;
        setAddSubmitting(true);
        setActionError(null);
        try {
            const res = await API.PROJECTS.ROOT.CREATE_TERM(parent.id, {
                name: addName.trim(),
                parentType: parentTypeForDepth(parent.level),
            });
            if (res && !res.error) {
                await loadTree(rootNode);
                setExpandedKeys((prev) => new Set(prev).add(parent.key));
                setAddModal(null);
                setAddName("");
            } else {
                setActionError(res?.message ?? "Could not create folder.");
            }
        } finally {
            setAddSubmitting(false);
        }
    };

    const levelLabelForNode = (node: TreeNode) =>
        singularLevelName(curriculumLevelLabel(node.level) ?? "Folder");

    const openEditModal = async (node: TreeNode) => {
        setActionError(null);
        setEditName(node.name);
        setEditDescription("");
        if (node.level === 0) {
            const res = await API.PROJECTS.ROOT.GET(node.id);
            if (res && !res.error && res.data) {
                setEditDescription(String(res.data.description ?? ""));
            }
        }
        setEditModal({ nodeKey: node.key, label: levelLabelForNode(node), isYear: node.level === 0 });
    };

    const closeEditModal = () => {
        if (editSubmitting) return;
        setEditModal(null);
        setEditName("");
        setEditDescription("");
    };

    const saveEditedFolder = async () => {
        if (!editModal || !editName.trim() || !rootNode) return;
        const node = nodeByKey[editModal.nodeKey];
        if (!node) return;
        setEditSubmitting(true);
        setActionError(null);
        try {
            const res =
                node.level === 0
                    ? await API.PROJECTS.ROOT.UPDATE(node.id, editName.trim(), editDescription.trim())
                    : await API.PROJECTS.ROOT.UPDATE_TERM(node.id, {
                          name: editName.trim(),
                          nodeType: nodeTypeForLevel(node.level),
                      });
            if (res && !res.error) {
                await loadTree(rootNode);
                if (node.level === 0) {
                    setRootNode({ ...rootNode, name: editName.trim() });
                }
                setEditModal(null);
                setEditName("");
            } else {
                setActionError(res?.message ?? "Could not update folder.");
            }
        } finally {
            setEditSubmitting(false);
        }
    };

    const countSubjectsUnder = (node: TreeNode): number => {
        if (node.level >= FOLDER_LEVEL_COUNT) {
            return subjectCountByGroupId[node.id] ?? 0;
        }
        let total = 0;
        for (const child of childrenByParentKey[node.key] ?? []) {
            total += countSubjectsUnder(child);
        }
        return total;
    };

    const cascadeDeleteDescription = (node: TreeNode) => {
        const nestedFolders = (childrenByParentKey[node.key] ?? []).length;
        const subjects = countSubjectsUnder(node);
        const parts = [`Are you sure you want to delete "${node.name}"?`];
        if (node.level === 0) {
            parts.push(
                "All projects, terms, subject groups, and subjects in this year will also be archived."
            );
        } else {
            parts.push("All nested folders and subjects will also be archived.");
        }
        if (nestedFolders > 0 || subjects > 0) {
            const details: string[] = [];
            if (nestedFolders > 0) {
                details.push(`${nestedFolders} nested folder${nestedFolders === 1 ? "" : "s"}`);
            }
            if (subjects > 0) {
                details.push(`${subjects} subject${subjects === 1 ? "" : "s"}`);
            }
            parts.push(`This includes ${details.join(" and ")}.`);
        }
        parts.push("You can restore them later from Archived Folders.");
        return parts.join(" ");
    };

    const openDeleteModal = (node: TreeNode) => {
        const label = levelLabelForNode(node);
        setActionError(null);
        setDeleteModal({
            nodeKey: node.key,
            label,
            name: node.name,
            description: cascadeDeleteDescription(node),
        });
    };

    const closeDeleteModal = () => {
        if (deleteSubmitting) return;
        setDeleteModal(null);
    };

    const confirmDeleteFolder = async () => {
        if (!deleteModal || !rootNode) return;
        const node = nodeByKey[deleteModal.nodeKey];
        if (!node) return;

        setDeleteSubmitting(true);
        setActionError(null);
        try {
            const res = await API.PROJECTS.ROOT.DELETE_TERM(node.id, {
                nodeType: deleteNodeTypeForLevel(node.level),
            });
            if (res && !res.error) {
                if (node.level === 0) {
                    setDeleteModal(null);
                    router.push("/projects");
                    return;
                }
                const parentKey = node.parentKey;
                await loadTree(rootNode);
                if (parentKey != null) setSelectedKey(parentKey);
                await refreshSubjectCounts();
                setDeleteModal(null);
            } else {
                setActionError(res?.message ?? "Could not delete folder.");
                setDeleteModal(null);
            }
        } finally {
            setDeleteSubmitting(false);
        }
    };

    const renderEmptyLevelScaffold = (parentKey: number, depth: number, canAddAtThisLevel = true) => {
        if (depth >= FOLDER_LEVEL_COUNT) return null;

        const levelName = curriculumLevelLabel(depth + 1);
        if (!levelName) return null;

        const singularName = singularLevelName(levelName);

        return (
            <div
                key={`empty-${parentKey}-${depth}`}
                className="mt-2 border-l-2 border-slate-200 pl-4"
                style={{ marginLeft: `${depth === 0 ? 24 : 14}px` }}
            >
                <div className="text-xs uppercase tracking-wide text-slate-400 mb-1">
                    {levelName}
                </div>
                {renderEmptyLevelScaffold(parentKey, depth + 1, false)}
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
                        openAddModal(parentKey, singularName);
                    }}
                >
                    + Add {singularName}
                </button>
            </div>
        );
    };

    const renderNode = (node: TreeNode, depth: number) => {
        const children = childrenByParentKey[node.key] ?? [];
        const badge =
            depth >= FOLDER_LEVEL_COUNT
                ? subjectCountByGroupId[node.id] ?? 0
                : children.length;
        const isSelected = selectedKey === node.key;
        const isInSelectedPath = selectedPathKeys.has(node.key);
        const isExpanded = expandedKeys.has(node.key);
        const childLevelName = curriculumLevelLabel(depth + 1);
        const childSingularName = singularLevelName(childLevelName);
        const canAddChild = depth < FOLDER_LEVEL_COUNT;
        return (
            <div key={node.key} className="mb-2">
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
                    onClick={() => goToFolder(node)}
                >
                    <span className="flex items-center gap-2">
                        <span className="shrink-0">
                            <FolderIcon />
                        </span>
                        <span>{node.name}</span>
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
                            onClick={(e) => {
                                e.stopPropagation();
                                openAddModal(node.key, childSingularName || "Folder");
                            }}
                        >
                            + Add {childSingularName}
                        </button>
                    </div>
                )}
                {isExpanded && children.length === 0 && canAddChild && renderEmptyLevelScaffold(node.key, depth)}
            </div>
        );
    };

    return (
        <div className="w-full max-h-screen overflow-y-auto">
            <div className="bg-white border w-full border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                <h1 className="font-bold text-3xl">{rootNode.name}</h1>
                <div className="flex items-center gap-2">
                    <Link href="/projects/archived">
                        <button type="button" className="px-4 py-2 rounded-lg bg-gray-700 text-white hover:bg-gray-800">
                            Archived Folders
                        </button>
                    </Link>
                    <button
                        type="button"
                        className="px-4 py-2 border border-slate-300 rounded-lg"
                        onClick={() => router.push("/projects")}
                    >
                        Back to Projects
                    </button>
                </div>
            </div>
            <div className="p-6 w-full">
                <div className="bg-white border w-full border-slate-200 rounded-xl overflow-hidden">
                    <div className="grid grid-cols-12 min-h-[560px]">
                        <div ref={treeScrollRef} className="col-span-4 border-r bg-slate-50 p-4 overflow-auto">
                            <div className="min-w-max pr-4">
                                {renderNode(rootNode, 0)}
                            </div>
                        </div>
                        <div className="col-span-8 p-6">
                            <div className="flex items-start justify-between gap-4 mb-2">
                                <h2 className="text-2xl font-semibold">{selectedNode?.name ?? rootNode.name}</h2>
                                {selectedNode && (
                                    <div className="flex items-center gap-2 shrink-0">
                                        <button
                                            type="button"
                                            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-700 hover:bg-slate-50"
                                            onClick={() => openEditModal(selectedNode)}
                                            disabled={editSubmitting || deleteSubmitting}
                                        >
                                            <EditIcon />
                                            {selectedNode.level === 0 ? "Edit" : "Rename"}
                                        </button>
                                        {selectedNode && (
                                            <button
                                                type="button"
                                                className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm text-red-600 hover:bg-red-50 disabled:opacity-60"
                                                onClick={() => openDeleteModal(selectedNode)}
                                                disabled={editSubmitting || deleteSubmitting}
                                            >
                                                <TrashIcon />
                                                {deleteSubmitting ? "Deleting..." : "Delete"}
                                            </button>
                                        )}
                                    </div>
                                )}
                            </div>
                            <p className="text-slate-500">
                                Level: {curriculumLevelLabel(selectedDepth)}
                            </p>
                            <p className="text-slate-500 mt-2">
                                Children: {selectedNode ? (childrenByParentKey[selectedNode.key]?.length ?? 0) : 0}
                            </p>
                            {actionError && (
                                <p className="mt-3 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                                    {actionError}
                                </p>
                            )}

                            <div className="mt-6 border border-slate-200 rounded-lg overflow-hidden">
                                <div className="px-4 py-3 bg-slate-50 border-b border-slate-200 font-medium">
                                    Next nested folders
                                </div>
                                {!selectedNode ? (
                                    <div className="p-4 text-slate-500">Select a folder from the tree.</div>
                                ) : (childrenByParentKey[selectedNode.key] ?? []).length === 0 ? (
                                    <div className="p-4 text-slate-500">
                                        This is the last nested folder. Opening subjects list...
                                    </div>
                                ) : (
                                    <div className="divide-y divide-slate-100">
                                        {(childrenByParentKey[selectedNode.key] ?? []).map((child) => (
                                            <div
                                                key={child.key}
                                                className="flex items-center justify-between gap-3 px-4 py-3 hover:bg-slate-50"
                                            >
                                                <button
                                                    type="button"
                                                    className="min-w-0 flex-1 text-left flex items-center gap-2"
                                                    onClick={() => goToFolder(child)}
                                                >
                                                    <FolderIcon />
                                                    <span className="font-medium truncate">{child.name}</span>
                                                </button>
                                                <div className="flex items-center gap-1 shrink-0">
                                                    <button
                                                        type="button"
                                                        className="rounded-md p-2 text-blue-600 hover:bg-blue-50"
                                                        aria-label={`Rename ${child.name}`}
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            openEditModal(child);
                                                        }}
                                                        disabled={editSubmitting || deleteSubmitting}
                                                    >
                                                        <EditIcon />
                                                    </button>
                                                    <button
                                                        type="button"
                                                        className="rounded-md p-2 text-red-600 hover:bg-red-50"
                                                        aria-label={`Delete ${child.name}`}
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            void openDeleteModal(child);
                                                        }}
                                                        disabled={editSubmitting || deleteSubmitting}
                                                    >
                                                        <TrashIcon />
                                                    </button>
                                                </div>
                                            </div>
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
            {editModal && (
                <div
                    className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="edit-folder-title"
                    onClick={closeEditModal}
                >
                    <form
                        className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
                        onClick={(e) => e.stopPropagation()}
                        onSubmit={async (e) => {
                            e.preventDefault();
                            await saveEditedFolder();
                        }}
                    >
                        <div className="flex items-start justify-between gap-4">
                            <div>
                                <h2 id="edit-folder-title" className="text-xl font-bold text-slate-900">
                                    {editModal.isYear ? "Edit Year" : `Rename ${editModal.label}`}
                                </h2>
                                <p className="mt-1 text-sm text-slate-500">
                                    {editModal.isYear
                                        ? "Update the year name and description."
                                        : `Update the ${editModal.label.toLowerCase()} name.`}
                                </p>
                            </div>
                            <button
                                type="button"
                                className="rounded-md px-2 py-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                                onClick={closeEditModal}
                                aria-label="Close"
                            >
                                x
                            </button>
                        </div>

                        <label className="mt-5 block text-sm font-semibold text-slate-700">
                            {editModal.label} Name
                        </label>
                        <input
                            autoFocus
                            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                            placeholder={`Enter ${editModal.label} name`}
                            value={editName}
                            onChange={(e) => setEditName(e.target.value)}
                        />

                        {editModal.isYear && (
                            <>
                                <label className="mt-4 block text-sm font-semibold text-slate-700">
                                    Description
                                </label>
                                <textarea
                                    className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 min-h-[90px] outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                                    placeholder="Enter year description"
                                    value={editDescription}
                                    onChange={(e) => setEditDescription(e.target.value)}
                                />
                            </>
                        )}

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                type="button"
                                className="rounded-lg border border-slate-300 px-4 py-2 text-slate-700 hover:bg-slate-50"
                                onClick={closeEditModal}
                                disabled={editSubmitting}
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-60"
                                disabled={editSubmitting || !editName.trim()}
                            >
                                {editSubmitting ? "Saving..." : "Save"}
                            </button>
                        </div>
                    </form>
                </div>
            )}
            <ConfirmDeleteModal
                open={deleteModal != null}
                title={`Delete ${deleteModal?.label ?? "folder"}?`}
                description={deleteModal?.description ?? ""}
                submitting={deleteSubmitting}
                onClose={closeDeleteModal}
                onConfirm={confirmDeleteFolder}
            />
        </div>
    );
};

export default ProjectTreePage;
