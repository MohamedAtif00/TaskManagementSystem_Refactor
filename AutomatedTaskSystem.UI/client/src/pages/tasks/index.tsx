import { useEffect, useMemo, useState } from "react";
import Head from "next/head";
import API from "../../lib/API";
import Loader from "../../components/loader";
import TaskIcon from "../../assets/Icons/Task";
import TaskSubjectsDataGrid from "../../components/tasks/TaskSubjectsDataGrid";

const fallbackLevelLabels = [
    "Project Name",
    "Year",
    "Term",
    "Subject",
    "Grade",
];

const getPathParts = (subject: IProject) =>
    String((subject as any).folderPath ?? "")
        .split(">")
        .map((x) => x.trim())
        .filter(Boolean);

const getSubjectLevelNames = (subject: IProject) =>
    Array.isArray((subject as any).levelNames)
        ? (subject as any).levelNames
            .map((x: unknown) => String(x).trim())
            .filter(Boolean)
        : [];

const TasksIndex = () => {
    const [subjects, setSubjects] = useState<IProject[]>([]);
    const [filters, setFilters] = useState<Record<number, string>>({});
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        API.TASKS.PROJECTS().then((res) => {
            if (res && !res.error && Array.isArray(res.data)) {
                setSubjects(res.data);
            } else {
                setSubjects([]);
            }
            setLoading(false);
        });
    }, []);

    const maxDepth = useMemo(
        () => subjects.reduce((max, subject) => Math.max(max, getPathParts(subject).length), 0),
        [subjects]
    );

    const filterLabels = useMemo(() => {
        const selectedProject = filters[0];
        if (!selectedProject) {
            return [fallbackLevelLabels[0]];
        }

        const scopedSubjects = subjects.filter((subject) => getPathParts(subject)[0] === selectedProject);

        const configuredLevelNames = scopedSubjects.reduce<string[]>((longest, subject) => {
            const levelNames = getSubjectLevelNames(subject);
            return levelNames.length > longest.length ? levelNames : longest;
        }, []);

        const fallbackDepth = Math.max(
            0,
            scopedSubjects.reduce(
                (max, subject) => Math.max(max, getPathParts(subject).length - 1),
                maxDepth - 1
            )
        );
        const folderLevelLabels = configuredLevelNames.length > 0
            ? configuredLevelNames
            : Array.from(
                { length: fallbackDepth },
                (_, index) => fallbackLevelLabels[index + 1] ?? `Level ${index + 1}`
            );

        return [fallbackLevelLabels[0], ...folderLevelLabels];
    }, [filters, maxDepth, subjects]);

    const filterOptions = useMemo(() => {
        return Array.from({ length: filterLabels.length }, (_, index) => {
            const scopedSubjects = subjects.filter((subject) => {
                const parts = getPathParts(subject);
                return Object.entries(filters).every(([key, value]) => {
                    const filterIndex = Number(key);
                    return filterIndex >= index || !value || parts[filterIndex] === value;
                });
            });

            return Array.from(
                new Set(
                    scopedSubjects
                        .map((subject) => getPathParts(subject)[index])
                        .filter(Boolean)
                )
            ).sort((a, b) => a.localeCompare(b));
        });
    }, [filterLabels.length, filters, subjects]);

    const filteredSubjects = useMemo(() => {
        return subjects.filter((subject) => {
            const parts = getPathParts(subject);
            return Object.entries(filters).every(([key, value]) => {
                if (!value) return true;
                return parts[Number(key)] === value;
            });
        });
    }, [filters, subjects]);

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

    if (loading) {
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Loader />
            </div>
        );
    }

    return (
        <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
            <Head>
                <title>ATS - Tasks</title>
            </Head>
            <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                <div className="flex gap-3 items-center min-w-0">
                    <div className="w-6 h-6 shrink-0">
                        <TaskIcon className="stroke-black" />
                    </div>
                    <h1 className="font-bold text-2xl truncate">Tasks</h1>
                </div>
            </div>

            <div className="mt-4 rounded-lg border border-slate-200 bg-white p-4">
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
                    {filterOptions.map((options, index) => (
                        <label key={index} className="block text-sm font-medium text-slate-700">
                            {filterLabels[index] ?? `Level ${index + 1}`}
                            <select
                                className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                                value={filters[index] ?? ""}
                                onChange={(e) => updateFilter(index, e.target.value)}
                            >
                                <option value="">Select {filterLabels[index] ?? `Level ${index + 1}`}</option>
                                {options.map((option) => (
                                    <option key={option} value={option}>
                                        {option}
                                    </option>
                                ))}
                            </select>
                        </label>
                    ))}
                </div>
            </div>

            <TaskSubjectsDataGrid subjects={filteredSubjects} />
        </div>
    );
};

export default TasksIndex;
