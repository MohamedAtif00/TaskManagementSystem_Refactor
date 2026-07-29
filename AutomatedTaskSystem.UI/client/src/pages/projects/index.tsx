import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/router";
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import Loader from "../../components/loader";
import ConfirmDeleteModal from "../../components/ConfirmDeleteModal";
import {
    CURRICULUM_CHILD_LEVELS,
    CURRICULUM_STRUCTURE_PATH,
    curriculumLevelLabel,
} from "../../lib/curriculumHierarchy";

type TreeNode = {
    /** Unique across levels (ids can collide between tables). */
    key: number;
    /** Real database id, used for API calls. */
    id: number;
    name: string;
    level: number;
    path?: string;
    nodeType?: string;
    parentKey: number | null;
};

type YearListItem = {
    id: number;
    name: string;
};

type SetupSubject = {
    id: number;
    name: string;
    description?: string;
    folderId: number;
    status?: number;
};

const STEP_TITLES = ["Create Year", "Setup Structure"] as const;
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

const EmptyFolderIllustration = () => (
    <div className="relative mx-auto mb-6 flex h-40 w-40 items-center justify-center rounded-full bg-slate-100">
        <svg width="96" height="96" viewBox="0 0 96 96" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <path
                d="M18 34c0-4.418 3.582-8 8-8h14l6 6h24c4.418 0 8 3.582 8 8v30c0 4.418-3.582 8-8 8H26c-4.418 0-8-3.582-8-8V34z"
                fill="#93C5FD"
            />
            <path
                d="M18 42h60v30c0 4.418-3.582 8-8 8H26c-4.418 0-8-3.582-8-8V42z"
                fill="#60A5FA"
            />
            <circle cx="48" cy="28" r="6" fill="#FBBF24" />
            <path d="M45 24h6v2h-6v-2zm1 3h4l1 8h-6l1-8z" fill="#1E3A8A" />
            <path d="M62 22l2 4h4l-3 3 1 4-4-2-4 2 1-4-3-3h4l2-4z" fill="#F59E0B" />
            <path d="M34 20c0-2 1.5-3.5 3.5-3.5S41 18 41 20v2h-7v-2z" fill="#1E40AF" />
            <path d="M33 22h9v3c0 2.5-2 4.5-4.5 4.5S33 27.5 33 25v-3z" fill="#1D4ED8" />
        </svg>
    </div>
);

const ProjectsIndex = () => {
    const auth = useAppSelector((s) => s.authSlice);
    const router = useRouter();

    const [loading, setLoading] = useState(true);
    const [isCreating, setIsCreating] = useState(false);
    const [step, setStep] = useState(1);
    const [existingRoots, setExistingRoots] = useState<YearListItem[]>([]);

    const [projectName, setProjectName] = useState("2026/2027");
    const [description, setDescription] = useState("");
    const [rootNode, setRootNode] = useState<TreeNode | null>(null);
    const [selectedKey, setSelectedKey] = useState<number | null>(null);
    const [childrenByParentKey, setChildrenByParentKey] = useState<Record<number, TreeNode[]>>({});
    const [nodeByKey, setNodeByKey] = useState<Record<number, TreeNode>>({});
    const [subjectCountByGroupId, setSubjectCountByGroupId] = useState<Record<number, number>>({});
    const [subjectsByGroupId, setSubjectsByGroupId] = useState<Record<number, SetupSubject[]>>({});
    const [newSubjectName, setNewSubjectName] = useState("");
    const [newSubjectDescription, setNewSubjectDescription] = useState("");
    const [showAddSubjectBox, setShowAddSubjectBox] = useState(false);
    const [busy, setBusy] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");
    const [expandedKeys, setExpandedKeys] = useState<Set<number>>(new Set());
    const [addModal, setAddModal] = useState<{ parentKey: number; label: string } | null>(null);
    const [addName, setAddName] = useState("");
    const [addSubmitting, setAddSubmitting] = useState(false);
    const [editModal, setEditModal] = useState<{ nodeKey: number; label: string; isYear?: boolean } | null>(null);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [editSubmitting, setEditSubmitting] = useState(false);
    const [yearEditModal, setYearEditModal] = useState<{
        id: number;
        name: string;
        description: string;
    } | null>(null);
    const [deleteSubmitting, setDeleteSubmitting] = useState(false);
    const [deleteModal, setDeleteModal] = useState<{
        nodeKey: number;
        label: string;
        name: string;
        description: string;
    } | null>(null);
    const [yearDeleteModal, setYearDeleteModal] = useState<{
        id: number;
        name: string;
        description: string;
    } | null>(null);

    useEffect(() => {
        if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4)) {
            router.replace("/");
            return;
        }
        setLoading(false);
    }, [auth.isAuth, auth.role, router]);

    useEffect(() => {
        if (loading) return;
        API.PROJECTS.ROOT.LIST().then((res) => {
            if (res && !res.error && Array.isArray(res.data)) {
                setExistingRoots(res.data as YearListItem[]);
            } else {
                setExistingRoots([]);
            }
        });
    }, [loading, isCreating]);

    const selectedNode = selectedKey != null ? nodeByKey[selectedKey] ?? null : null;
    const selectedDepth = selectedNode?.level ?? 0;

    const folderPath = useMemo(() => {
        if (!selectedNode) return "";
        const parts: string[] = [];
        let current: TreeNode | null = selectedNode;
        while (current) {
            parts.push(current.name);
            current = current.parentKey != null ? nodeByKey[current.parentKey] ?? null : null;
        }
        return parts.reverse().join(" > ");
    }, [nodeByKey, selectedNode]);

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
    };

    const refreshSubjectsCount = async () => {
        const all = await API.PROJECTS.GET_ALL();
        if (!all || all.error || !Array.isArray(all.data)) return;
        const counts: Record<number, number> = {};
        for (const subject of all.data as any[]) {
            if (subject.status !== 0 && subject.status !== 3) continue;
            const groupId = Number(subject.folderId);
            if (!Number.isNaN(groupId)) {
                counts[groupId] = (counts[groupId] ?? 0) + 1;
            }
        }
        setSubjectCountByGroupId(counts);
    };

    const loadSubjectsForGroup = async (groupId: number) => {
        const res = await API.PROJECTS.GET_BY_FOLDER(groupId, { includeInactive: true });
        if (!res || res.error || !Array.isArray(res.data)) {
            setSubjectsByGroupId((prev) => ({ ...prev, [groupId]: [] }));
            return [] as SetupSubject[];
        }
        const subjects = (res.data as SetupSubject[])
            .filter((s) => s.status === undefined || s.status === 0 || s.status === 3)
            .sort((a, b) => a.name.localeCompare(b.name));
        setSubjectsByGroupId((prev) => ({ ...prev, [groupId]: subjects }));
        return subjects;
    };

    const goToSetupFolders = async () => {
        if (!projectName.trim()) return;
        setBusy(true);
        setErrorMessage("");
        try {
            let root = rootNode;
            if (!root) {
                const existing = existingRoots.find(
                    (item) => item.name.trim().toLowerCase() === projectName.trim().toLowerCase()
                );
                if (existing) {
                    root = {
                        key: nodeKey(0, existing.id),
                        id: existing.id,
                        name: existing.name,
                        level: 0,
                        nodeType: "year",
                        parentKey: null,
                    };
                }

                if (!root) {
                    const createRes = await API.PROJECTS.ROOT.CREATE(projectName.trim(), description.trim());
                    if (createRes && !createRes.error && createRes.data) {
                        root = {
                            key: nodeKey(0, createRes.data.id),
                            id: createRes.data.id,
                            name: createRes.data.name,
                            level: 0,
                            path: createRes.data.path,
                            nodeType: "year",
                            parentKey: null,
                        };
                    } else {
                        setErrorMessage(createRes?.message ?? "Could not create year. Please try again.");
                        return;
                    }
                }
            }
            if (!root) {
                setErrorMessage("Could not prepare the year hierarchy.");
                return;
            }
            setRootNode(root);
            await API.PROJECTS.ROOT.UPDATE(root.id, root.name, description.trim());
            setSelectedKey(root.key);
            setExpandedKeys(new Set([root.key]));
            await loadTree(root);
            await refreshSubjectsCount();
            setStep(2);
        } finally {
            setBusy(false);
        }
    };

    const openAddModal = (parentKey: number, label: string) => {
        setAddName("");
        setAddModal({ parentKey, label });
    };

    const closeAddModal = () => {
        if (addSubmitting) return;
        setAddModal(null);
        setAddName("");
    };

    const openEditModal = async (node: TreeNode, label: string) => {
        setEditName(node.name);
        setEditDescription("");
        if (node.level === 0) {
            const res = await API.PROJECTS.ROOT.GET(node.id);
            if (res && !res.error && res.data) {
                setEditDescription(String(res.data.description ?? ""));
            }
        }
        setEditModal({ nodeKey: node.key, label, isYear: node.level === 0 });
    };

    const closeEditModal = () => {
        if (editSubmitting) return;
        setEditModal(null);
        setEditName("");
        setEditDescription("");
    };

    const addFolderFromModal = async () => {
        if (!addModal || !addName.trim() || !rootNode) return;
        const parent = nodeByKey[addModal.parentKey];
        if (!parent) return;
        setAddSubmitting(true);
        try {
            const res = await API.PROJECTS.ROOT.CREATE_TERM(parent.id, {
                name: addName.trim(),
                parentType: parentTypeForDepth(parent.level),
            });
            if (res && !res.error) {
                await loadTree(rootNode);
                setExpandedKeys((prev) => new Set(prev).add(parent.key));
                setSelectedKey(parent.key);
                setAddModal(null);
                setAddName("");
            }
        } finally {
            setAddSubmitting(false);
        }
    };

    const saveEditedFolder = async () => {
        if (!editModal || !editName.trim() || !rootNode) return;
        const node = nodeByKey[editModal.nodeKey];
        if (!node) return;
        setEditSubmitting(true);
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
                    setProjectName(editName.trim());
                    setDescription(editDescription.trim());
                    setRootNode({ ...rootNode, name: editName.trim() });
                }
                setEditModal(null);
                setEditName("");
                setEditDescription("");
            }
        } finally {
            setEditSubmitting(false);
        }
    };

    const levelLabelForNode = (node: TreeNode) =>
        singularLevelName(curriculumLevelLabel(node.level) ?? "Folder");

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
        setErrorMessage("");
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
        setErrorMessage("");
        try {
            const res = await API.PROJECTS.ROOT.DELETE_TERM(node.id, {
                nodeType: deleteNodeTypeForLevel(node.level),
            });
            if (res && !res.error) {
                if (node.level === 0) {
                    setDeleteModal(null);
                    setIsCreating(false);
                    setRootNode(null);
                    setStep(1);
                    const listRes = await API.PROJECTS.ROOT.LIST();
                    if (listRes && !listRes.error && Array.isArray(listRes.data)) {
                        setExistingRoots(listRes.data as YearListItem[]);
                    }
                    return;
                }
                const parentKey = node.parentKey;
                await loadTree(rootNode);
                if (parentKey != null) setSelectedKey(parentKey);
                await refreshSubjectsCount();
                setDeleteModal(null);
            } else {
                setErrorMessage(res?.message ?? "Could not delete folder.");
                setDeleteModal(null);
            }
        } finally {
            setDeleteSubmitting(false);
        }
    };

    const openEditYearFromList = async (year: YearListItem) => {
        const res = await API.PROJECTS.ROOT.GET(year.id);
        const description =
            res && !res.error && res.data ? String(res.data.description ?? "") : "";
        setYearEditModal({ id: year.id, name: year.name, description });
    };

    const closeYearEditModal = () => {
        if (editSubmitting) return;
        setYearEditModal(null);
    };

    const saveYearEditFromList = async () => {
        if (!yearEditModal || !yearEditModal.name.trim()) return;
        setEditSubmitting(true);
        setErrorMessage("");
        try {
            const res = await API.PROJECTS.ROOT.UPDATE(
                yearEditModal.id,
                yearEditModal.name.trim(),
                yearEditModal.description.trim()
            );
            if (res && !res.error) {
                setYearEditModal(null);
                const listRes = await API.PROJECTS.ROOT.LIST();
                if (listRes && !listRes.error && Array.isArray(listRes.data)) {
                    setExistingRoots(listRes.data as YearListItem[]);
                }
            } else {
                setErrorMessage(res?.message ?? "Could not update year.");
            }
        } finally {
            setEditSubmitting(false);
        }
    };

    const openDeleteYearFromList = (year: YearListItem) => {
        setYearDeleteModal({
            id: year.id,
            name: year.name,
            description: `Are you sure you want to delete "${year.name}"? All projects, terms, subject groups, and subjects in this year will also be archived. You can restore them later from Archived Folders.`,
        });
    };

    const closeYearDeleteModal = () => {
        if (deleteSubmitting) return;
        setYearDeleteModal(null);
    };

    const confirmDeleteYearFromList = async () => {
        if (!yearDeleteModal) return;
        setDeleteSubmitting(true);
        setErrorMessage("");
        try {
            const res = await API.PROJECTS.ROOT.DELETE_TERM(yearDeleteModal.id, { nodeType: "year" });
            if (res && !res.error) {
                setYearDeleteModal(null);
                const listRes = await API.PROJECTS.ROOT.LIST();
                if (listRes && !listRes.error && Array.isArray(listRes.data)) {
                    setExistingRoots(listRes.data as YearListItem[]);
                } else {
                    setExistingRoots([]);
                }
            } else {
                setErrorMessage(res?.message ?? "Could not delete year.");
                setYearDeleteModal(null);
            }
        } finally {
            setDeleteSubmitting(false);
        }
    };

    const selectFolderInSetup = async (node: TreeNode) => {
        setSelectedKey(node.key);
        setShowAddSubjectBox(false);
        setExpandedKeys((prev) => new Set(prev).add(node.key));
        if (node.level >= FOLDER_LEVEL_COUNT) {
            await loadSubjectsForGroup(node.id);
        }
    };

    const addSubjectToLeaf = async () => {
        if (!selectedNode || selectedNode.level < FOLDER_LEVEL_COUNT || !newSubjectName.trim()) return;
        setBusy(true);
        try {
            const res = await API.PROJECTS.CREATE({
                name: newSubjectName.trim(),
                description: newSubjectDescription.trim(),
                folderId: selectedNode.id,
            });
            if (res && !res.error) {
                setNewSubjectName("");
                setNewSubjectDescription("");
                setShowAddSubjectBox(false);
                await refreshSubjectsCount();
                await loadSubjectsForGroup(selectedNode.id);
            }
        } finally {
            setBusy(false);
        }
    };

    const persistRootAsync = async () => {
        if (rootNode) {
            await API.PROJECTS.ROOT.UPDATE(rootNode.id, rootNode.name, description.trim());
        }
    };

    const refreshProjectRoots = async () => {
        const res = await API.PROJECTS.ROOT.LIST();
        if (res && !res.error && Array.isArray(res.data)) {
            setExistingRoots(res.data as YearListItem[]);
        }
    };

    const saveProjectDraft = async () => {
        await persistRootAsync();
        await refreshProjectRoots();
        setIsCreating(false);
        setStep(1);
    };

    const finishProjectSetup = async () => {
        await persistRootAsync();
        if (rootNode) {
            router.push(`/projects/${rootNode.id}`);
            return;
        }
        setIsCreating(false);
        setStep(1);
    };

    const renderEmptyLevelScaffold = (parentKey: number, depth: number, canAddAtThisLevel = true) => {
        if (depth >= FOLDER_LEVEL_COUNT) return null;

        const levelName = curriculumLevelLabel(depth + 1);
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
                    onClick={(e) => {
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

    const renderFolderNode = (node: TreeNode, depth: number) => {
        const isSelected = selectedKey === node.key;
        const children = childrenByParentKey[node.key] ?? [];
        const badgeCount =
            depth >= FOLDER_LEVEL_COUNT
                ? subjectCountByGroupId[node.id] ?? 0
                : children.length;
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
                            : "hover:bg-slate-100"
                    }`}
                    style={{ paddingLeft: `${depth * 14 + 10}px` }}
                    onClick={async () => {
                        setSelectedKey(node.key);
                        setExpandedKeys((prev) => {
                            const next = new Set(prev);
                            if (next.has(node.key)) next.delete(node.key);
                            else next.add(node.key);
                            return next;
                        });
                        if (depth >= FOLDER_LEVEL_COUNT) {
                            await loadSubjectsForGroup(node.id);
                        }
                    }}
                >
                    <span className="flex items-center gap-2">
                        <span className="shrink-0">
                            <FolderIcon />
                        </span>
                        <span className="truncate">{node.name}</span>
                    </span>
                    <span className="text-xs bg-slate-200 rounded-full px-2 py-0.5">{badgeCount}</span>
                </button>
                {isExpanded && children.length > 0 && canAddChild && (
                    <div
                        className="mt-2 border-l-2 border-slate-200 pl-4"
                        style={{ marginLeft: `${depth === 0 ? 24 : 14}px` }}
                    >
                        <div className="text-xs uppercase tracking-wide text-slate-400 mb-1">
                            {childLevelName}
                        </div>
                        {children.map((child) => renderFolderNode(child, depth + 1))}
                        <button
                            type="button"
                            className={`text-sm mt-1 ${addActionColor(depth)}`}
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

    if (!auth.isAuth || (auth.role !== 0 && auth.role !== 4) || loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    if (!isCreating) {
        return (
            <div className="w-full max-h-screen overflow-y-auto">
                <div className="bg-white border border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10 flex items-center justify-between">
                    <h1 className="font-bold text-3xl">Projects</h1>
                    <div className="flex items-center gap-2">
                        <Link href="/projects/archived">
                            <button
                                type="button"
                                className="bg-gray-700 hover:bg-gray-800 text-white px-5 py-2 rounded-lg"
                            >
                                Archived Folders
                            </button>
                        </Link>
                        <button
                            type="button"
                            onClick={() => {
                                setErrorMessage("");
                                setIsCreating(true);
                            }}
                            className="bg-blue-500 hover:bg-blue-600 text-white px-5 py-2 rounded-lg"
                        >
                            Create Year
                        </button>
                    </div>
                </div>

                <div className="p-6">
                    <div className="bg-white border border-slate-200 rounded-xl p-4">
                        <h2 className="text-xl font-semibold mb-3">Existing Years</h2>
                        {existingRoots.length === 0 ? (
                            <p className="text-slate-500">No years found. Click Create Year to start.</p>
                        ) : (
                            <div className="space-y-2">
                                {existingRoots.map((root) => (
                                    <div
                                        key={root.id}
                                        className="flex items-center justify-between gap-3 border border-slate-200 rounded-lg px-3 py-2 hover:bg-slate-50"
                                    >
                                        <button
                                            type="button"
                                            className="min-w-0 flex-1 text-left"
                                            onClick={() => router.push(`/projects/${root.id}`)}
                                        >
                                            <div className="font-medium">{root.name}</div>
                                        </button>
                                        <button
                                            type="button"
                                            className="shrink-0 rounded-md p-2 text-blue-600 hover:bg-blue-50"
                                            aria-label={`Edit ${root.name}`}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                void openEditYearFromList(root);
                                            }}
                                            disabled={editSubmitting}
                                        >
                                            <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                                                <path d="M0 12.7928V15.0858C0 15.2184 0.0526785 15.3456 0.146447 15.4393C0.240215 15.5331 0.367392 15.5858 0.5 15.5858H2.798C2.93035 15.5858 3.05729 15.5333 3.151 15.4398L12.599 5.99179L9.599 2.99179L0.147 12.4398C0.0531646 12.5333 0.000293383 12.6603 0 12.7928ZM10.837 1.75279L13.837 4.75279L15.297 3.29279C15.4845 3.10526 15.5898 2.85095 15.5898 2.58579C15.5898 2.32062 15.4845 2.06631 15.297 1.87879L13.712 0.292786C13.5245 0.105315 13.2702 0 13.005 0C12.7398 0 12.4855 0.105315 12.298 0.292786L10.837 1.75279Z" fill="currentColor"/>
                                            </svg>
                                        </button>
                                        <button
                                            type="button"
                                            className="shrink-0 rounded-md p-2 text-red-600 hover:bg-red-50"
                                            aria-label={`Delete ${root.name}`}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                void openDeleteYearFromList(root);
                                            }}
                                            disabled={deleteSubmitting}
                                        >
                                            <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                                                <path d="M5.5 1.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1V2h3.25a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1 0-1.5H5.5V1.5zM3.5 4.5v8.75A2.25 2.25 0 0 0 5.75 15.5h4.5a2.25 2.25 0 0 0 2.25-2.25V4.5H3.5zm2.25 1.5a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75zm3.5 0a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75z" fill="currentColor"/>
                                            </svg>
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
                {yearEditModal && (
                    <div
                        className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="edit-year-list-title"
                        onClick={closeYearEditModal}
                    >
                        <form
                            className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
                            onClick={(e) => e.stopPropagation()}
                            onSubmit={async (e) => {
                                e.preventDefault();
                                await saveYearEditFromList();
                            }}
                        >
                            <h2 id="edit-year-list-title" className="text-xl font-bold text-slate-900">
                                Edit Year
                            </h2>
                            <p className="mt-1 text-sm text-slate-500">
                                Update the year name and description.
                            </p>
                            <label className="mt-5 block text-sm font-semibold text-slate-700">Year Name</label>
                            <input
                                autoFocus
                                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                                value={yearEditModal.name}
                                onChange={(e) =>
                                    setYearEditModal({ ...yearEditModal, name: e.target.value })
                                }
                            />
                            <label className="mt-4 block text-sm font-semibold text-slate-700">Description</label>
                            <textarea
                                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 min-h-[90px] outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                                value={yearEditModal.description}
                                onChange={(e) =>
                                    setYearEditModal({ ...yearEditModal, description: e.target.value })
                                }
                            />
                            <div className="mt-6 flex justify-end gap-3">
                                <button
                                    type="button"
                                    className="rounded-lg border border-slate-300 px-4 py-2 text-slate-700 hover:bg-slate-50"
                                    onClick={closeYearEditModal}
                                    disabled={editSubmitting}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-60"
                                    disabled={editSubmitting || !yearEditModal.name.trim()}
                                >
                                    {editSubmitting ? "Saving..." : "Save"}
                                </button>
                            </div>
                        </form>
                    </div>
                )}
                <ConfirmDeleteModal
                    open={yearDeleteModal != null}
                    title="Delete year?"
                    description={yearDeleteModal?.description ?? ""}
                    submitting={deleteSubmitting}
                    onClose={closeYearDeleteModal}
                    onConfirm={confirmDeleteYearFromList}
                />
            </div>
        );
    }

    const canAddFolder = selectedDepth < FOLDER_LEVEL_COUNT;
    const canAddSubject = selectedDepth === FOLDER_LEVEL_COUNT;
    const nextLevelName = curriculumLevelLabel(selectedDepth + 1);
    const nextSingularLevelName = singularLevelName(nextLevelName);
    const currentLevelLabel = curriculumLevelLabel(selectedDepth);
    const currentSingularLabel = singularLevelName(currentLevelLabel);
    const selectedChildren = selectedNode ? childrenByParentKey[selectedNode.key] ?? [] : [];
    const isFolderEmpty = !!selectedNode && selectedChildren.length === 0;
    const selectedSubjects = selectedNode && canAddSubject ? subjectsByGroupId[selectedNode.id] ?? [] : [];
    const isSubjectsEmpty = canAddSubject && selectedSubjects.length === 0;
    const emptyStateTitle = canAddSubject ? "No Subjects Found" : `No ${nextLevelName} Found`;
    const emptyStateMessage = canAddSubject
        ? `This ${currentSingularLabel.toLowerCase()} doesn't have any subjects yet. Start by adding the first subject to your academic calendar.`
        : `This ${currentSingularLabel.toLowerCase()} doesn't have any ${nextLevelName.toLowerCase()} yet. Start by adding the first ${nextSingularLevelName.toLowerCase()} to your academic calendar.`;

    return (
        <div className="w-full max-h-screen overflow-y-auto">
            <div className="bg-white border border-gray-300 rounded-b-md px-6 py-4 sticky top-0 z-10">
                <h1 className="font-bold text-3xl">Projects</h1>
            </div>

            <div className="bg-white min-h-[calc(100vh-110px)] p-8">
                <div className="w-full">
                    <div className="flex items-center justify-center gap-12 mb-8 text-slate-700">
                        {STEP_TITLES.map((title, idx) => {
                            const s = idx + 1;
                            const active = s === step;
                            const done = s < step;
                            return (
                                <div key={title} className="flex items-center gap-2">
                                    <span
                                        className={`w-8 h-8 rounded-full inline-flex items-center justify-center text-sm ${
                                            active || done ? "bg-blue-500 text-white" : "bg-white text-black"
                                        }`}
                                    >
                                        {done ? "✓" : s}
                                    </span>
                                    <span className={active || done ? "text-slate-900" : "text-slate-400"}>{title}</span>
                                </div>
                            );
                        })}
                    </div>
                    {errorMessage && (
                        <div className="mb-4 mx-auto max-w-4xl rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                            {errorMessage}
                        </div>
                    )}

                    {step === 1 && (
                        <div className="bg-white rounded-2xl shadow p-8 w-full max-w-4xl mx-auto">
                                    <h2 className="font-bold text-4xl mb-3">Start a New Year</h2>
                                    <p className="text-slate-500 mb-6">
                                        A year is the top of the hierarchy. Projects, terms, and subject groups are organized beneath it.
                                    </p>
                                    <div className="mb-6 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600">
                                        <span className="font-semibold text-slate-800">Structure: </span>
                                        {CURRICULUM_STRUCTURE_PATH}
                                    </div>
                                    <div className="space-y-4">
                                        <div>
                                            <label className="block text-sm font-semibold mb-1">Year Name</label>
                                            <input
                                                className="w-full border rounded-lg px-3 py-2"
                                                value={projectName}
                                                onChange={(e) => setProjectName(e.target.value)}
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-semibold mb-1">Description</label>
                                            <textarea
                                                className="w-full border rounded-lg px-3 py-2 min-h-[90px]"
                                                value={description}
                                                onChange={(e) => setDescription(e.target.value)}
                                            />
                                        </div>
                                    </div>
                                    <div className="flex justify-end mt-8">
                                        <button
                                            type="button"
                                            onClick={() => {
                                                setIsCreating(false);
                                                setStep(1);
                                            }}
                                            className="px-5 py-2 rounded-lg border border-slate-300 mr-2"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            type="button"
                                            disabled={busy || !projectName.trim()}
                                            onClick={goToSetupFolders}
                                            className="bg-blue-500 hover:bg-blue-600 disabled:opacity-60 text-white px-5 py-2 rounded-lg"
                                        >
                                            Save & Continue
                                        </button>
                                    </div>
                        </div>
                    )}

                    {step === 2 && (
                        <div className="bg-white rounded-xl overflow-hidden shadow">
                            <div className="grid grid-cols-12 min-h-[560px]">
                                <div className="col-span-4 border-r p-4 bg-slate-50 overflow-auto">
                                    <h3 className="font-semibold mb-3">{projectName}</h3>
                                    <div className="min-w-max pr-4">
                                        {rootNode ? renderFolderNode(rootNode, 0) : <div>No root folder</div>}
                                    </div>
                                </div>
                                <div className="col-span-8 p-6">
                                    <div className="mb-5 flex items-start justify-between gap-4">
                                        <div>
                                            <div className="text-sm text-slate-500 mb-1">
                                                {(folderPath || projectName)
                                                    .split(" > ")
                                                    .map((part, index, parts) => (
                                                        <span key={`${part}-${index}`}>
                                                            {index > 0 && <span className="text-slate-400"> &gt; </span>}
                                                            <span
                                                                className={
                                                                    index === parts.length - 1
                                                                        ? "text-blue-600 font-medium"
                                                                        : undefined
                                                                }
                                                            >
                                                                {part}
                                                            </span>
                                                        </span>
                                                    ))}
                                            </div>
                                            <h2 className="text-4xl font-bold">{selectedNode?.name ?? "Select a folder"}</h2>
                                        </div>
                                        {selectedNode && selectedNode.level === 0 && (
                                            <button
                                                type="button"
                                                className="border border-slate-300 text-slate-700 hover:bg-slate-50 px-4 py-2 mt-10 rounded-lg whitespace-nowrap"
                                                onClick={() => void openEditModal(selectedNode, "Year")}
                                            >
                                                Edit Year
                                            </button>
                                        )}
                                        {selectedNode && canAddFolder && (
                                            <button
                                                type="button"
                                                className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 mt-10 rounded-lg whitespace-nowrap"
                                                onClick={() => openAddModal(selectedNode.key, nextSingularLevelName)}
                                            >
                                                Add {nextSingularLevelName} +
                                            </button>
                                        )}
                                        {selectedNode && canAddSubject && (
                                            <button
                                                type="button"
                                                className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 mt-10 rounded-lg whitespace-nowrap"
                                                onClick={() => setShowAddSubjectBox(true)}
                                            >
                                                Add Subject +
                                            </button>
                                        )}
                                    </div>
                                    <div className="border-t border-slate-200 pt-6">
                                        {selectedNode && (
                                            <div className="space-y-6">
                                                {isFolderEmpty && canAddFolder && (
                                                    <div className="flex flex-col items-center justify-center text-center py-10">
                                                        <EmptyFolderIllustration />
                                                        <h3 className="text-2xl font-bold text-slate-800 mb-2">{emptyStateTitle}</h3>
                                                        <p className="text-slate-500 max-w-md mb-6">{emptyStateMessage}</p>
                                                        <button
                                                            type="button"
                                                            className="bg-blue-500 hover:bg-blue-600 text-white px-5 py-2 rounded-lg"
                                                            onClick={() => openAddModal(selectedNode.key, nextSingularLevelName)}
                                                        >
                                                            Add {nextSingularLevelName} +
                                                        </button>
                                                    </div>
                                                )}

                                                {!isFolderEmpty && canAddFolder && (
                                                    <div className="space-y-3">
                                                        {selectedChildren.map((child) => {
                                                            const subjectCount = countSubjectsUnder(child);
                                                            return (
                                                                <div
                                                                    key={child.key}
                                                                    className="flex items-center justify-between gap-4 rounded-2xl border border-slate-200 bg-white px-5 py-4 shadow-sm hover:border-slate-300"
                                                                >
                                                                    <button
                                                                        type="button"
                                                                        className="min-w-0 text-left"
                                                                        onClick={() => selectFolderInSetup(child)}
                                                                    >
                                                                        <div className="font-semibold text-slate-900 truncate text-lg">
                                                                            {child.name}
                                                                        </div>
                                                                    </button>
                                                                    <span className="shrink-0 rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-600">
                                                                        {subjectCount}{" "}
                                                                        {subjectCount === 1 ? "subject" : "subjects"}
                                                                    </span>
                                                                    <button
                                                                        type="button"
                                                                        className="shrink-0 rounded-md p-2 text-blue-600 hover:bg-blue-50"
                                                                        aria-label={`Edit ${child.name}`}
                                                                        onClick={(e) => {
                                                                            e.stopPropagation();
                                                                            openEditModal(child, nextSingularLevelName);
                                                                        }}
                                                                        disabled={editSubmitting || deleteSubmitting}
                                                                    >
                                                                        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                            <path d="M0 12.7928V15.0858C0 15.2184 0.0526785 15.3456 0.146447 15.4393C0.240215 15.5331 0.367392 15.5858 0.5 15.5858H2.798C2.93035 15.5858 3.05729 15.5333 3.151 15.4398L12.599 5.99179L9.599 2.99179L0.147 12.4398C0.0531646 12.5333 0.000293383 12.6603 0 12.7928ZM10.837 1.75279L13.837 4.75279L15.297 3.29279C15.4845 3.10526 15.5898 2.85095 15.5898 2.58579C15.5898 2.32062 15.4845 2.06631 15.297 1.87879L13.712 0.292786C13.5245 0.105315 13.2702 0 13.005 0C12.7398 0 12.4855 0.105315 12.298 0.292786L10.837 1.75279Z" fill="#3B82F6"/>
                                                                        </svg>
                                                                    </button>
                                                                    <button
                                                                        type="button"
                                                                        className="shrink-0 rounded-md p-2 text-red-600 hover:bg-red-50"
                                                                        aria-label={`Delete ${child.name}`}
                                                                        onClick={(e) => {
                                                                            e.stopPropagation();
                                                                            void openDeleteModal(child);
                                                                        }}
                                                                        disabled={editSubmitting || deleteSubmitting}
                                                                    >
                                                                        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                                                                            <path d="M5.5 1.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1V2h3.25a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1 0-1.5H5.5V1.5zM3.5 4.5v8.75A2.25 2.25 0 0 0 5.75 15.5h4.5a2.25 2.25 0 0 0 2.25-2.25V4.5H3.5zm2.25 1.5a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75zm3.5 0a.75.75 0 0 1 .75.75v5.5a.75.75 0 0 1-1.5 0v-5.5a.75.75 0 0 1 .75-.75z" fill="currentColor"/>
                                                                        </svg>
                                                                    </button>
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                )}

                                                {canAddSubject && isSubjectsEmpty && !showAddSubjectBox && (
                                                    <div className="flex flex-col items-center justify-center text-center py-10">
                                                        <EmptyFolderIllustration />
                                                        <h3 className="text-2xl font-bold text-slate-800 mb-2">{emptyStateTitle}</h3>
                                                        <p className="text-slate-500 max-w-md mb-6">{emptyStateMessage}</p>
                                                        <button
                                                            type="button"
                                                            className="bg-blue-500 hover:bg-blue-600 text-white px-5 py-2 rounded-lg"
                                                            onClick={() => setShowAddSubjectBox(true)}
                                                        >
                                                            Add Subject +
                                                        </button>
                                                    </div>
                                                )}

                                                {canAddSubject && (showAddSubjectBox || selectedSubjects.length > 0) && (
                                                    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                                                        <h3 className="font-bold text-xl text-slate-900 mb-4">Add Subject</h3>
                                                        <div className="space-y-3">
                                                            <input
                                                                className="w-full border border-slate-300 rounded-lg px-3 py-2"
                                                                placeholder="Subject name"
                                                                value={newSubjectName}
                                                                onChange={(e) => setNewSubjectName(e.target.value)}
                                                            />
                                                            <textarea
                                                                className="w-full border border-slate-300 rounded-lg px-3 py-2 min-h-[90px]"
                                                                placeholder="Subject description"
                                                                value={newSubjectDescription}
                                                                onChange={(e) => setNewSubjectDescription(e.target.value)}
                                                            />
                                                            <button
                                                                type="button"
                                                                onClick={addSubjectToLeaf}
                                                                disabled={busy || !newSubjectName.trim()}
                                                                className="bg-green-600 hover:bg-green-700 disabled:opacity-60 text-white px-4 py-2 rounded-lg"
                                                            >
                                                                Add Subject
                                                            </button>
                                                        </div>
                                                    </div>
                                                )}

                                                {canAddSubject && selectedSubjects.length > 0 && (
                                                    <div className="space-y-3">
                                                        {selectedSubjects.map((subject) => (
                                                            <div
                                                                key={subject.id}
                                                                className="flex items-center justify-between gap-4 rounded-2xl border border-slate-200 bg-white px-5 py-4 shadow-sm"
                                                            >
                                                                <div className="min-w-0 flex-1">
                                                                    <div className="font-semibold text-slate-900 truncate text-lg">
                                                                        {subject.name}
                                                                    </div>
                                                                    {subject.description ? (
                                                                        <div className="text-sm text-slate-500 mt-0.5 truncate">
                                                                            {subject.description}
                                                                        </div>
                                                                    ) : null}
                                                                </div>
                                                            </div>
                                                        ))}
                                                    </div>
                                                )}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                            <div className="border-t px-6 py-4 flex justify-between">
                                <button
                                    type="button"
                                    onClick={() => setStep(1)}
                                    className="px-5 py-2 rounded-lg border border-slate-300"
                                >
                                    Back
                                </button>
                                <div className="flex gap-3">
                                    <button
                                        type="button"
                                        disabled={busy}
                                        className="bg-slate-100 hover:bg-slate-200 disabled:opacity-60 text-slate-700 px-5 py-2 rounded-lg"
                                        onClick={saveProjectDraft}
                                    >
                                        Save as Draft
                                    </button>
                                    <button
                                        type="button"
                                        disabled={busy}
                                        className="bg-blue-500 hover:bg-blue-600 disabled:opacity-60 text-white px-5 py-2 rounded-lg"
                                        onClick={finishProjectSetup}
                                    >
                                        Finish Setup
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
            {addModal && (
                <div
                    className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="setup-add-folder-title"
                    onClick={closeAddModal}
                >
                    <form
                        className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
                        onClick={(e) => e.stopPropagation()}
                        onSubmit={async (e) => {
                            e.preventDefault();
                            await addFolderFromModal();
                        }}
                    >
                        <div className="flex items-start justify-between gap-4">
                            <div>
                                <h2 id="setup-add-folder-title" className="text-xl font-bold text-slate-900">
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
                    aria-labelledby="setup-edit-folder-title"
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
                                <h2 id="setup-edit-folder-title" className="text-xl font-bold text-slate-900">
                                    {editModal.isYear ? "Edit Year" : `Edit ${editModal.label}`}
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

export default ProjectsIndex;
