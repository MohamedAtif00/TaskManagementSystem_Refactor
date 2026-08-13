import { useMemo, useState } from "react";
import {
    CURRICULUM_ROOT_LEVEL,
    curriculumLevelLabel,
    getCurriculumPathParts,
    subjectMatchesHierarchyFilters,
} from "../lib/curriculumHierarchy";

export const useCurriculumPathFilters = (subjects: IProject[]) => {
    const [filters, setFilters] = useState<Record<number, string>>({});

    const filterLabels = useMemo(() => {
        const selectedYear = filters[0];
        if (!selectedYear) {
            return [CURRICULUM_ROOT_LEVEL];
        }

        const scopedSubjects = subjects.filter(
            (subject) => getCurriculumPathParts(subject.folderPath)[0] === selectedYear
        );
        const pathDepth = scopedSubjects.reduce(
            (max, subject) => Math.max(max, getCurriculumPathParts(subject.folderPath).length),
            1
        );

        return Array.from({ length: pathDepth }, (_, index) => curriculumLevelLabel(index));
    }, [filters, subjects]);

    const filterOptions = useMemo(() => {
        return Array.from({ length: filterLabels.length }, (_, index) => {
            const scopedSubjects = subjects.filter((subject) => {
                const parts = getCurriculumPathParts(subject.folderPath);
                return Object.entries(filters).every(([key, value]) => {
                    const filterIndex = Number(key);
                    return filterIndex >= index || !value || parts[filterIndex] === value;
                });
            });

            return Array.from(
                new Set(
                    scopedSubjects
                        .map((subject) => getCurriculumPathParts(subject.folderPath)[index])
                        .filter(Boolean)
                )
            ).sort((a, b) => a.localeCompare(b));
        });
    }, [filterLabels.length, filters, subjects]);

    const filteredSubjects = useMemo(
        () =>
            subjects.filter((subject) =>
                subjectMatchesHierarchyFilters(subject.folderPath, filters)
            ),
        [filters, subjects]
    );

    const updateFilter = (index: number, value: string) => {
        setFilters((prev) => {
            const next: Record<number, string> = {};
            for (const [key, existingValue] of Object.entries(prev)) {
                const existingIndex = Number(key);
                if (existingIndex < index && existingValue) {
                    next[existingIndex] = existingValue;
                }
            }
            if (value) next[index] = value;
            return next;
        });
    };

    const setFiltersFromPath = (folderPath?: string) => {
        const parts = getCurriculumPathParts(folderPath);
        if (parts.length === 0) return;
        const next: Record<number, string> = {};
        parts.forEach((part, index) => {
            next[index] = part;
        });
        setFilters(next);
    };

    const applyFilters = (next: Record<number, string>) => {
        setFilters(next);
    };

    const hasProjectSelected = Boolean(filters[1]);

    return {
        filters,
        filterLabels,
        filterOptions,
        filteredSubjects,
        updateFilter,
        setFiltersFromPath,
        applyFilters,
        hasProjectSelected,
    };
};
