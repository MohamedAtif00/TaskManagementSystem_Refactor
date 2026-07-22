/** Root folder = Season. Nested folders: Project → Term → Subject. */
export const CURRICULUM_ROOT_LEVEL = "Season" as const;

export const CURRICULUM_FOLDER_LEVELS = ["Project", "Term", "Subject"] as const;

export const CURRICULUM_STRUCTURE_PATH =
    "Season → Project → Term → Subject";

export const curriculumFolderLevels = (): string[] => [...CURRICULUM_FOLDER_LEVELS];

export const curriculumLevelLabel = (depth: number): string => {
    if (depth === 0) return CURRICULUM_ROOT_LEVEL;
    return CURRICULUM_FOLDER_LEVELS[depth - 1] ?? `Level ${depth}`;
};

export const parseCurriculumPath = (folderPath?: string) => {
    if (!folderPath) {
        return { season: "-", project: "-", term: "-", subject: "-" };
    }
    const parts = folderPath
        .split(">")
        .map((part) => part.trim())
        .filter((part) => part.length > 0);
    return {
        season: parts[0] ?? "-",
        project: parts[1] ?? "-",
        term: parts[2] ?? "-",
        subject: parts[3] ?? "-",
    };
};

export const parseSeasonTerm = (folderPath?: string) => {
    const { season, term } = parseCurriculumPath(folderPath);
    return { season, term };
};
