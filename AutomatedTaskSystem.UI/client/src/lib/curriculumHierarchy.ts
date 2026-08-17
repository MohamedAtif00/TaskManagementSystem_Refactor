/** Year → Project → Term → Subject Group → Subject records */
export const CURRICULUM_ROOT_LEVEL = "Year" as const;

export const CURRICULUM_CHILD_LEVELS = ["Project", "Term", "Subject Group"] as const;

export const CURRICULUM_STRUCTURE_PATH =
    "Year → Project → Term → Subject Group → Subject";

export const curriculumChildLevels = (): string[] => [...CURRICULUM_CHILD_LEVELS];

export const curriculumLevelLabel = (depth: number): string => {
    if (depth <= 0) return CURRICULUM_ROOT_LEVEL;
    return CURRICULUM_CHILD_LEVELS[depth - 1] ?? `Level ${depth}`;
};

export type CurriculumNodeType = "year" | "project" | "term" | "subjectGroup";

export type ArchivedSubjectSummary = {
    id: number;
    name: string;
};

export type ArchivedCurriculumTreeNode = {
    id: number;
    name: string;
    nodeType: CurriculumNodeType;
    path: string;
    depth: number;
    archived: boolean;
    subjects: ArchivedSubjectSummary[];
    children: ArchivedCurriculumTreeNode[];
};

export type CurriculumNode = {
    id: number;
    name: string;
    levelName: string;
    depth: number;
    path: string;
    parentId?: number | null;
    nodeType: CurriculumNodeType;
};

export const parseCurriculumPath = (folderPath?: string) => {
    if (!folderPath) {
        return { year: "-", project: "-", term: "-", subjectGroup: "-" };
    }
    const parts = folderPath
        .split(">")
        .map((part) => part.trim())
        .filter((part) => part.length > 0);
    return {
        year: parts[0] ?? "-",
        project: parts[1] ?? "-",
        term: parts[2] ?? "-",
        subjectGroup: parts[3] ?? "-",
    };
};

export const parseYearTerm = (folderPath?: string) => {
    const { year, term } = parseCurriculumPath(folderPath);
    return { year, term };
};

/** Human-readable hierarchy label, e.g. `2026/2027 › Selah Eltelmeez › Term 1 › Arabic`. */
export const formatCurriculumPathLabel = (path?: string, separator = " › ") => {
    if (!path?.trim()) return "";
    return path
        .split(">")
        .map((part) => part.trim())
        .filter(Boolean)
        .join(separator);
};

export type SubjectGroupPickerOption = { id: number; name: string };

/** Maps curriculum subject-group nodes to unique dropdown labels using the full path. */
export const mapSubjectGroupNodesToOptions = (
    nodes: Array<{ id?: number; name?: string; path?: string }>
): SubjectGroupPickerOption[] =>
    nodes
        .filter((node) => node.id != null)
        .map((node) => ({
            id: Number(node.id),
            name:
                formatCurriculumPathLabel(node.path) ||
                String(node.name ?? "").trim() ||
                `Subject group #${node.id}`,
        }))
        .sort((a, b) => a.name.localeCompare(b.name));

export const subjectLocationLabel = (folderPath?: string) =>
    formatCurriculumPathLabel(folderPath) || folderPath?.trim() || "";

/** Split a subject folder path into hierarchy segments (Year, Project, Term, Subject Group). */
export const getCurriculumPathParts = (folderPath?: string) =>
    String(folderPath ?? "")
        .split(">")
        .map((part) => part.trim())
        .filter(Boolean);

export const namesMatch = (left?: string, right?: string) =>
    String(left ?? "").trim().toLowerCase() === String(right ?? "").trim().toLowerCase();

/** True when every populated filter segment matches the subject folder path. */
export const subjectMatchesHierarchyFilters = (
    folderPath: string | undefined,
    filters: Record<number, string>
) => {
    const parts = getCurriculumPathParts(folderPath);
    return Object.entries(filters).every(([key, value]) => {
        if (!value) return true;
        return namesMatch(parts[Number(key)], value);
    });
};

/** True when a sprint belongs to the selected curriculum project. */
export const sprintMatchesProjectFilter = (
    projectNames: string[] | undefined,
    projectName?: string
) => {
    if (!projectName?.trim()) return true;
    return (projectNames ?? []).some((name) => namesMatch(name, projectName));
};
