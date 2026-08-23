import { CURRICULUM_CHILD_LEVELS } from "./curriculumHierarchy";

export const SUBJECT_GROUP_TREE_LEVEL = CURRICULUM_CHILD_LEVELS.length;

export const projectTreeNodeKey = (level: number, id: number) => level * 10_000_000 + id;

export type ProjectTreeNodeRef = {
    level: number;
    id: number;
};

export type ProjectTreeSessionState = {
    selected: ProjectTreeNodeRef | null;
    expanded: ProjectTreeNodeRef[];
    scrollTop?: number;
};

const storageKey = (yearId: number) => `projects:tree-state:${yearId}`;

const canUseSessionStorage = () => typeof window !== "undefined" && !!window.sessionStorage;

export const loadProjectTreeState = (yearId: number): ProjectTreeSessionState | null => {
    if (!canUseSessionStorage() || Number.isNaN(yearId)) return null;
    try {
        const raw = sessionStorage.getItem(storageKey(yearId));
        if (!raw) return null;
        const parsed = JSON.parse(raw) as ProjectTreeSessionState;
        if (!parsed || !Array.isArray(parsed.expanded)) return null;
        return parsed;
    } catch {
        return null;
    }
};

export const saveProjectTreeState = (yearId: number, state: ProjectTreeSessionState) => {
    if (!canUseSessionStorage() || Number.isNaN(yearId)) return;
    try {
        sessionStorage.setItem(storageKey(yearId), JSON.stringify(state));
    } catch {
        // Ignore quota / privacy errors.
    }
};

export const markProjectTreeSubjectGroup = (yearId: number, subjectGroupId: number) => {
    const existing = loadProjectTreeState(yearId);
    saveProjectTreeState(yearId, {
        selected: { level: SUBJECT_GROUP_TREE_LEVEL, id: subjectGroupId },
        expanded: existing?.expanded?.length
            ? existing.expanded
            : [{ level: 0, id: yearId }],
        scrollTop: existing?.scrollTop,
    });
};

type TreeNodeLike = {
    key: number;
    level: number;
    id: number;
    parentKey: number | null;
};

export const restoreProjectTreeUiState = (
    yearId: number,
    nodeByKey: Record<number, TreeNodeLike>
): { selectedKey: number | null; expandedKeys: Set<number> } => {
    const root = Object.values(nodeByKey).find((n) => n.level === 0) ?? null;
    const defaultState = {
        selectedKey: root?.key ?? null,
        expandedKeys: new Set(root ? [root.key] : []),
    };

    const saved = loadProjectTreeState(yearId);
    if (!saved) return defaultState;

    const expandedKeys = new Set<number>();
    for (const nodeRef of saved.expanded) {
        const key = projectTreeNodeKey(nodeRef.level, nodeRef.id);
        if (nodeByKey[key]) expandedKeys.add(key);
    }

    let selectedKey: number | null = null;
    if (saved.selected) {
        const key = projectTreeNodeKey(saved.selected.level, saved.selected.id);
        if (nodeByKey[key]) selectedKey = key;
    }

    if (selectedKey == null) {
        return {
            selectedKey: defaultState.selectedKey,
            expandedKeys: expandedKeys.size > 0 ? expandedKeys : defaultState.expandedKeys,
        };
    }

    let current: TreeNodeLike | null = nodeByKey[selectedKey] ?? null;
    while (current) {
        expandedKeys.add(current.key);
        current = current.parentKey != null ? nodeByKey[current.parentKey] ?? null : null;
    }

    return {
        selectedKey,
        expandedKeys: expandedKeys.size > 0 ? expandedKeys : defaultState.expandedKeys,
    };
};

export const persistProjectTreeUiState = (
    yearId: number,
    selectedKey: number | null,
    expandedKeys: Set<number>,
    nodeByKey: Record<number, TreeNodeLike>,
    scrollTop?: number
) => {
    const selectedNode = selectedKey != null ? nodeByKey[selectedKey] : null;
    const expanded = Array.from(expandedKeys)
        .map((key) => nodeByKey[key])
        .filter((node): node is TreeNodeLike => !!node)
        .map((node) => ({ level: node.level, id: node.id }));

    saveProjectTreeState(yearId, {
        selected: selectedNode ? { level: selectedNode.level, id: selectedNode.id } : null,
        expanded,
        scrollTop,
    });
};
