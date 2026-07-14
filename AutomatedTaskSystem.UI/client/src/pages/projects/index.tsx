import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/router";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import Loader from "../../components/loader";

type FolderDto = {
    id: number;
    name: string;
    parentFolderId?: number | null;
    level?: number;
    path?: string;
    levelNames?: string[];
};

type SetupSubject = {
    id: number;
    name: string;
    description?: string;
    folderId: number;
    status?: number;
};

const STEP_TITLES = ["Create Project", "Configure Levels", "Setup Folders"] as const;

const DEFAULT_LEVELS = ["Years", "Terms", "Subjects"];
const LEVEL_TITLE_PLACEHOLDER = "Please add level title";
const levelStorageKey = (rootId: number) => `project-level-names:${rootId}`;

const singularLevelName = (levelName: string) => {
    if (levelName === "Years") return "Year";
    if (levelName === "Terms") return "Term";
    if (levelName === "Subjects") return "Subject";
    if (levelName === "Grades") return "Grade";
    return levelName.endsWith("s") ? levelName.slice(0, -1) : levelName;
};

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
    const [existingRoots, setExistingRoots] = useState<FolderDto[]>([]);

    const [projectName, setProjectName] = useState("Selah Eltelmeez");
    const [description, setDescription] = useState("");
    const [numberOfLevels, setNumberOfLevels] = useState("");
    const [levelNames, setLevelNames] = useState<string[]>([]);

    const [rootFolder, setRootFolder] = useState<FolderDto | null>(null);
    const [selectedFolderId, setSelectedFolderId] = useState<number | null>(null);
    const [foldersByParent, setFoldersByParent] = useState<Record<number, FolderDto[]>>({});
    const [folderById, setFolderById] = useState<Record<number, FolderDto>>({});
    const [folderParentById, setFolderParentById] = useState<Record<number, number | null>>({});
    const [subjectCountByFolder, setSubjectCountByFolder] = useState<Record<number, number>>({});
    const [subjectsByFolder, setSubjectsByFolder] = useState<Record<number, SetupSubject[]>>({});
    const [newSubjectName, setNewSubjectName] = useState("");
    const [newSubjectDescription, setNewSubjectDescription] = useState("");
    const [showAddSubjectBox, setShowAddSubjectBox] = useState(false);
    const [busy, setBusy] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");
    const [expandedFolderIds, setExpandedFolderIds] = useState<Set<number>>(new Set());
    const [addModal, setAddModal] = useState<{ parentFolderId: number; label: string } | null>(null);
    const [addName, setAddName] = useState("");
    const [addSubmitting, setAddSubmitting] = useState(false);
    const [editModal, setEditModal] = useState<{ folderId: number; label: string; name: string } | null>(null);
    const [editName, setEditName] = useState("");
    const [editSubmitting, setEditSubmitting] = useState(false);

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
                setExistingRoots((res.data as FolderDto[]).filter((f) => !f.parentFolderId));
            } else {
                setExistingRoots([]);
            }
        });
    }, [loading, isCreating]);

    const selectedFolder = selectedFolderId ? folderById[selectedFolderId] ?? null : null;

    const selectedDepth = useMemo(() => {
        if (!selectedFolderId) return 0;
        let depth = 0;
        let current = selectedFolderId;
        while (folderParentById[current] != null) {
            depth += 1;
            const parentId = folderParentById[current];
            if (!parentId) break;
            current = parentId;
        }
        return depth;
    }, [folderParentById, selectedFolderId]);

    const folderPath = useMemo(() => {
        if (!selectedFolderId) return "";
        const parts: string[] = [];
        let current: number | null = selectedFolderId;
        while (current != null) {
            const folder = folderById[current];
            if (!folder) break;
            parts.push(folder.name);
            current = folderParentById[current] ?? null;
        }
        return parts.reverse().join(" > ");
    }, [folderById, folderParentById, selectedFolderId]);

    const loadChildren = async (parentId: number) => {
        const res = await API.PROJECTS.ROOT.TERMS(parentId);
        if (!res || res.error || !Array.isArray(res.data)) return [] as FolderDto[];
        const children = (res.data as FolderDto[]).sort((a, b) => a.name.localeCompare(b.name));
        setFoldersByParent((prev) => ({ ...prev, [parentId]: children }));
        setFolderById((prev) => {
            const next = { ...prev };
            for (const child of children) next[child.id] = child;
            return next;
        });
        setFolderParentById((prev) => {
            const next = { ...prev };
            for (const child of children) next[child.id] = parentId;
            return next;
        });
        return children;
    };

    const refreshSubjectsCount = async () => {
        const all = await API.PROJECTS.GET_ALL();
        if (!all || all.error || !Array.isArray(all.data)) return;
        const counts: Record<number, number> = {};
        for (const subject of all.data as any[]) {
            if (subject.status !== 0 && subject.status !== 3) continue;
            const folderId = Number(subject.folderId);
            if (!Number.isNaN(folderId)) {
                counts[folderId] = (counts[folderId] ?? 0) + 1;
            }
        }
        setSubjectCountByFolder(counts);
    };

    const loadSubjectsForFolder = async (folderId: number) => {
        const res = await API.PROJECTS.GET_BY_FOLDER(folderId, { includeInactive: true });
        if (!res || res.error || !Array.isArray(res.data)) {
            setSubjectsByFolder((prev) => ({ ...prev, [folderId]: [] }));
            return [] as SetupSubject[];
        }
        const subjects = (res.data as SetupSubject[])
            .filter((s) => s.status === undefined || s.status === 0 || s.status === 3)
            .sort((a, b) => a.name.localeCompare(b.name));
        setSubjectsByFolder((prev) => ({ ...prev, [folderId]: subjects }));
        return subjects;
    };

    const goToStepTwo = async () => {
        if (!projectName.trim()) return;
        const parsedLevelCount = Number(numberOfLevels);
        if (!Number.isInteger(parsedLevelCount) || parsedLevelCount < 1 || parsedLevelCount > 20) {
            setErrorMessage("Please enter a valid number of levels between 1 and 20 before continuing.");
            return;
        }
        setBusy(true);
        setErrorMessage("");
        try {
            let root = rootFolder;
            if (!root) {
                root =
                    existingRoots.find(
                        (existing) =>
                            existing.name.trim().toLowerCase() === projectName.trim().toLowerCase()
                    ) ?? null;

                if (!root) {
                const createRes = await API.PROJECTS.ROOT.CREATE(projectName.trim(), description.trim(), levelNames);
                    if (createRes && !createRes.error && createRes.data) {
                        root = createRes.data as FolderDto;
                    } else {
                        setErrorMessage(createRes?.message ?? "Could not create project. Please try again.");
                        return;
                    }
                }
            }
            if (!root) {
                setErrorMessage("Could not prepare the project hierarchy.");
                return;
            }
            setRootFolder(root);
            if (typeof window !== "undefined") {
                window.localStorage.setItem(levelStorageKey(root.id), JSON.stringify(levelNames));
            }
            await API.PROJECTS.ROOT.UPDATE(root.id, root.name, description.trim(), levelNames);
            setFolderById((prev) => ({ ...prev, [root!.id]: root! }));
            setFolderParentById((prev) => ({ ...prev, [root!.id]: null }));
            setSelectedFolderId(root.id);
            await loadChildren(root.id);
            await refreshSubjectsCount();
            setStep(2);
            API.PROJECTS.ROOT.LIST().then((res) => {
                if (res && !res.error && Array.isArray(res.data)) {
                    setExistingRoots((res.data as FolderDto[]).filter((f) => !f.parentFolderId));
                }
            });
        } finally {
            setBusy(false);
        }
    };

    const goToStepThree = async () => {
        const cleanedLevelNames = levelNames.map((level) => level.trim());
        const expectedLevelCount = Number(numberOfLevels);
        if (
            cleanedLevelNames.length !== expectedLevelCount ||
            cleanedLevelNames.some((level) => !level)
        ) {
            setErrorMessage("Please add a title for each level before continuing.");
            return;
        }
        setErrorMessage("");
        setLevelNames(cleanedLevelNames);
        setStep(3);
        if (rootFolder) {
            setExpandedFolderIds(new Set([rootFolder.id]));
            const children = await loadChildren(rootFolder.id);
            await Promise.all(children.map((child) => loadChildren(child.id)));
            await refreshSubjectsCount();
        }
    };

    const openAddModal = (parentFolderId: number, label: string) => {
        setAddName("");
        setAddModal({ parentFolderId, label });
    };

    const closeAddModal = () => {
        if (addSubmitting) return;
        setAddModal(null);
        setAddName("");
    };

    const openEditModal = (folder: FolderDto, label: string) => {
        setEditName(folder.name);
        setEditModal({ folderId: folder.id, label, name: folder.name });
    };

    const closeEditModal = () => {
        if (editSubmitting) return;
        setEditModal(null);
        setEditName("");
    };

    const addFolderFromModal = async () => {
        if (!addModal || !addName.trim()) return;
        setAddSubmitting(true);
        try {
            const parentFolderId = addModal.parentFolderId;
            const res = await API.PROJECTS.ROOT.CREATE_TERM(parentFolderId, {
                name: addName.trim(),
            });
            if (res && !res.error) {
                const children = await loadChildren(parentFolderId);
                await Promise.all(children.map((child) => loadChildren(child.id)));
                setExpandedFolderIds((prev) => new Set(prev).add(parentFolderId));
                setSelectedFolderId(parentFolderId);
                setAddModal(null);
                setAddName("");
            }
        } finally {
            setAddSubmitting(false);
        }
    };

    const saveEditedFolder = async () => {
        if (!editModal || !editName.trim()) return;
        setEditSubmitting(true);
        try {
            const res = await API.PROJECTS.ROOT.UPDATE_TERM(editModal.folderId, {
                name: editName.trim(),
            });
            if (res && !res.error) {
                const parentId = folderParentById[editModal.folderId];
                if (parentId != null) {
                    await loadChildren(parentId);
                }
                setFolderById((prev) => ({
                    ...prev,
                    [editModal.folderId]: {
                        ...(prev[editModal.folderId] ?? { id: editModal.folderId, name: editName.trim() }),
                        name: editName.trim(),
                    },
                }));
                setEditModal(null);
                setEditName("");
            }
        } finally {
            setEditSubmitting(false);
        }
    };

    const selectFolderInSetup = async (folder: FolderDto) => {
        setSelectedFolderId(folder.id);
        setShowAddSubjectBox(false);
        setExpandedFolderIds((prev) => new Set(prev).add(folder.id));
        const children = await loadChildren(folder.id);
        await Promise.all(children.map((child) => loadChildren(child.id)));
        if (children.length === 0) {
            await loadSubjectsForFolder(folder.id);
        }
    };

    const countSubjectsUnder = (folderId: number): number => {
        let total = subjectCountByFolder[folderId] ?? 0;
        for (const child of foldersByParent[folderId] ?? []) {
            total += countSubjectsUnder(child.id);
        }
        return total;
    };

    const addSubjectToLeaf = async () => {
        if (!selectedFolderId || !newSubjectName.trim()) return;
        setBusy(true);
        try {
            const res = await API.PROJECTS.CREATE({
                name: newSubjectName.trim(),
                description: newSubjectDescription.trim(),
                folderId: selectedFolderId,
            });
            if (res && !res.error) {
                setNewSubjectName("");
                setNewSubjectDescription("");
                setShowAddSubjectBox(false);
                await refreshSubjectsCount();
                await loadSubjectsForFolder(selectedFolderId);
            }
        } finally {
            setBusy(false);
        }
    };

    const persistLevelNames = () => {
        if (rootFolder && typeof window !== "undefined") {
            window.localStorage.setItem(levelStorageKey(rootFolder.id), JSON.stringify(levelNames));
        }
    };

    const persistLevelNamesAsync = async () => {
        persistLevelNames();
        if (rootFolder) {
            await API.PROJECTS.ROOT.UPDATE(rootFolder.id, rootFolder.name, description.trim(), levelNames);
        }
    };

    const refreshProjectRoots = async () => {
        const res = await API.PROJECTS.ROOT.LIST();
        if (res && !res.error && Array.isArray(res.data)) {
            setExistingRoots((res.data as FolderDto[]).filter((f) => !f.parentFolderId));
        }
    };

    const saveProjectDraft = async () => {
        await persistLevelNamesAsync();
        await refreshProjectRoots();
        setIsCreating(false);
        setStep(1);
    };

    const finishProjectSetup = async () => {
        await persistLevelNamesAsync();
        if (rootFolder) {
            router.push(`/projects/${rootFolder.id}`);
            return;
        }
        setIsCreating(false);
        setStep(1);
    };

    const renderEmptyLevelScaffold = (parentFolderId: number, depth: number, canAddAtThisLevel = true) => {
        const levelName = levelNames[depth];
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
                    onClick={(e) => {
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

    const renderFolderNode = (folder: FolderDto, depth: number) => {
        const isSelected = selectedFolderId === folder.id;
        const children = foldersByParent[folder.id] ?? [];
        const subjectCount = subjectCountByFolder[folder.id] ?? 0;
        const badgeCount = children.length > 0 ? children.length : subjectCount;
        const isExpanded = expandedFolderIds.has(folder.id);
        const childLevelName = levelNames[depth] ?? `Folder ${depth + 1}`;
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
                            : "hover:bg-slate-100"
                    }`}
                    style={{ paddingLeft: `${depth * 14 + 10}px` }}
                    onClick={async () => {
                        const willExpand = !expandedFolderIds.has(folder.id);
                        setSelectedFolderId(folder.id);
                        setExpandedFolderIds((prev) => {
                            const next = new Set(prev);
                            if (next.has(folder.id)) next.delete(folder.id);
                            else next.add(folder.id);
                            return next;
                        });
                        if (willExpand || foldersByParent[folder.id] === undefined) {
                            const children = await loadChildren(folder.id);
                            await Promise.all(children.map((child) => loadChildren(child.id)));
                            if (children.length === 0) {
                                await loadSubjectsForFolder(folder.id);
                            }
                        }
                    }}
                >
                    <span className="flex items-center gap-2">
                        <span className="shrink-0">
                            <FolderIcon />
                        </span>
                        <span className="truncate">{folder.name}</span>
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
                    <button
                        type="button"
                        onClick={() => {
                            setNumberOfLevels("");
                            setLevelNames([]);
                            setErrorMessage("");
                            setIsCreating(true);
                        }}
                        className="bg-blue-500 hover:bg-blue-600 text-white px-5 py-2 rounded-lg"
                    >
                        Create Project
                    </button>
                </div>

                <div className="p-6">
                    <div className="bg-white border border-slate-200 rounded-xl p-4">
                        <h2 className="text-xl font-semibold mb-3">Existing Projects</h2>
                        {existingRoots.length === 0 ? (
                            <p className="text-slate-500">No projects found. Click Create Project to start.</p>
                        ) : (
                            <div className="space-y-2">
                                {existingRoots.map((root) => (
                                    <button
                                        key={root.id}
                                        type="button"
                                        className="w-full text-left border border-slate-200 rounded-lg px-3 py-2 hover:bg-slate-50"
                                        onClick={() => router.push(`/projects/${root.id}`)}
                                    >
                                        <div className="font-medium">{root.name}</div>
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        );
    }

    const parsedNumberOfLevels = Number(numberOfLevels);
    const hasValidLevelCount =
        Number.isInteger(parsedNumberOfLevels) && parsedNumberOfLevels >= 1 && parsedNumberOfLevels <= 20;
    const hasValidLevelTitles =
        levelNames.length === parsedNumberOfLevels && levelNames.every((level) => level.trim().length > 0);
    const canAddFolder = selectedDepth < parsedNumberOfLevels;
    const canAddSubject = selectedDepth === parsedNumberOfLevels;
    const nextLevelName = levelNames[selectedDepth] ?? DEFAULT_LEVELS[selectedDepth] ?? `Folder ${selectedDepth + 1}`;
    const nextSingularLevelName = singularLevelName(nextLevelName);
    const currentLevelLabel =
        selectedDepth === 0 ? "Project" : levelNames[selectedDepth - 1] ?? DEFAULT_LEVELS[selectedDepth - 1] ?? `Folder ${selectedDepth}`;
    const currentSingularLabel = singularLevelName(currentLevelLabel);
    const selectedChildCount = selectedFolder ? foldersByParent[selectedFolder.id]?.length ?? 0 : 0;
    const isFolderEmpty = !!selectedFolder && selectedChildCount === 0;
    const selectedSubjects = selectedFolder ? subjectsByFolder[selectedFolder.id] ?? [] : [];
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

                    {(step === 1 || step === 2) && (
                        <div className="bg-white rounded-2xl shadow p-8 w-full max-w-4xl mx-auto">
                            {step === 1 ? (
                                <>
                                    <h2 className="font-bold text-4xl mb-3">Start a New Project</h2>
                                    <p className="text-slate-500 mb-6">
                                        Set the foundation for your educational structure.
                                    </p>
                                    <div className="space-y-4">
                                        <div>
                                            <label className="block text-sm font-semibold mb-1">Project Name</label>
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
                                        <div>
                                            <label className="block text-sm font-semibold mb-1">Number of Levels</label>
                                            <input
                                                type="number"
                                                min={1}
                                                max={20}
                                                className="w-full border rounded-lg px-3 py-2"
                                                value={numberOfLevels}
                                                onChange={(e) => {
                                                    const rawValue = e.target.value;
                                                    setNumberOfLevels(rawValue);
                                                    if (!rawValue) {
                                                        setLevelNames([]);
                                                        return;
                                                    }
                                                    const next = Math.max(1, Math.min(20, Number(rawValue) || 1));
                                                    setLevelNames((prev) => {
                                                        return Array.from({ length: next }, (_, index) => prev[index] ?? "");
                                                    });
                                                }}
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
                                            disabled={
                                                busy ||
                                                !projectName.trim() ||
                                                !hasValidLevelCount
                                            }
                                            onClick={goToStepTwo}
                                            className="bg-blue-500 hover:bg-blue-600 disabled:opacity-60 text-white px-5 py-2 rounded-lg"
                                        >
                                            Save & Continue
                                        </button>
                                    </div>
                                </>
                            ) : (
                                <>
                                    <h2 className="font-bold text-4xl mb-3">Configure Your Levels</h2>
                                    <p className="text-slate-500 mb-6">
                                        Rename each level to match your project&apos;s structure.
                                    </p>
                                    <div className="space-y-3">
                                        {levelNames.map((level, index) => (
                                            <div key={index} className="flex items-center gap-4">
                                                <span className="w-20 text-slate-500 text-sm font-semibold">
                                                    {`Level ${index + 1}`}
                                                </span>
                                                <input
                                                    className="flex-1 border rounded-lg px-3 py-2"
                                                    placeholder={LEVEL_TITLE_PLACEHOLDER}
                                                    value={level}
                                                    onChange={(e) =>
                                                        setLevelNames((prev) =>
                                                            prev.map((v, i) => (i === index ? e.target.value : v))
                                                        )
                                                    }
                                                />
                                            </div>
                                        ))}
                                    </div>
                                    <div className="flex justify-between mt-8">
                                        <button
                                            type="button"
                                            onClick={() => setStep(1)}
                                            className="px-5 py-2 rounded-lg border border-slate-300"
                                        >
                                            Back
                                        </button>
                                        <button
                                            type="button"
                                            disabled={busy || !hasValidLevelTitles}
                                            onClick={goToStepThree}
                                            className="bg-blue-500 hover:bg-blue-600 disabled:opacity-60 text-white px-5 py-2 rounded-lg"
                                        >
                                            Save & Continue
                                        </button>
                                    </div>
                                </>
                            )}
                        </div>
                    )}

                    {step === 3 && (
                        <div className="bg-white rounded-xl overflow-hidden shadow">
                            <div className="grid grid-cols-12 min-h-[560px]">
                                <div className="col-span-4 border-r p-4 bg-slate-50 overflow-auto">
                                    <h3 className="font-semibold mb-3">{projectName}</h3>
                                    <div className="min-w-max pr-4">
                                        {rootFolder ? renderFolderNode(rootFolder, 0) : <div>No root folder</div>}
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
                                            <h2 className="text-4xl font-bold">{selectedFolder?.name ?? "Select a folder"}</h2>
                                        </div>
                                        {selectedFolder && canAddFolder && (
                                            <button
                                                type="button"
                                                className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 mt-10 rounded-lg whitespace-nowrap"
                                                onClick={() => openAddModal(selectedFolder.id, nextSingularLevelName)}
                                            >
                                                Add {nextSingularLevelName} +
                                            </button>
                                        )}
                                        {selectedFolder && canAddSubject && (
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
                                        {selectedFolder && (
                                            <div className="space-y-6">
                                                {isFolderEmpty && canAddFolder && (
                                                    <div className="flex flex-col items-center justify-center text-center py-10">
                                                        <EmptyFolderIllustration />
                                                        <h3 className="text-2xl font-bold text-slate-800 mb-2">{emptyStateTitle}</h3>
                                                        <p className="text-slate-500 max-w-md mb-6">{emptyStateMessage}</p>
                                                        <button
                                                            type="button"
                                                            className="bg-blue-500 hover:bg-blue-600 text-white px-5 py-2 rounded-lg"
                                                            onClick={() => openAddModal(selectedFolder.id, nextSingularLevelName)}
                                                        >
                                                            Add {nextSingularLevelName} +
                                                        </button>
                                                    </div>
                                                )}

                                                {!isFolderEmpty && canAddFolder && (
                                                    <div className="space-y-3">
                                                        {(foldersByParent[selectedFolder.id] ?? []).map((child) => {
                                                            const subjectCount = countSubjectsUnder(child.id);
                                                            return (
                                                                <div
                                                                    key={child.id}
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
                                                                    >
                                                                        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                            <path d="M0 12.7928V15.0858C0 15.2184 0.0526785 15.3456 0.146447 15.4393C0.240215 15.5331 0.367392 15.5858 0.5 15.5858H2.798C2.93035 15.5858 3.05729 15.5333 3.151 15.4398L12.599 5.99179L9.599 2.99179L0.147 12.4398C0.0531646 12.5333 0.000293383 12.6603 0 12.7928ZM10.837 1.75279L13.837 4.75279L15.297 3.29279C15.4845 3.10526 15.5898 2.85095 15.5898 2.58579C15.5898 2.32062 15.4845 2.06631 15.297 1.87879L13.712 0.292786C13.5245 0.105315 13.2702 0 13.005 0C12.7398 0 12.4855 0.105315 12.298 0.292786L10.837 1.75279Z" fill="#3B82F6"/>
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
                                    onClick={() => setStep(2)}
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
                                    Edit {editModal.label}
                                </h2>
                                <p className="mt-1 text-sm text-slate-500">
                                    Update the {editModal.label.toLowerCase()} name.
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
        </div>
    );
};

export default ProjectsIndex;
