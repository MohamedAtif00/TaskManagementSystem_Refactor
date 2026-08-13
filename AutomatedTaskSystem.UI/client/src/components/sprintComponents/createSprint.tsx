import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/router";
import { motion } from "framer-motion";
import { addDays } from "date-fns";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome";
import CurriculumPathFilters from "../curriculum/CurriculumPathFilters";
import { useCurriculumPathFilters } from "../../hooks/useCurriculumPathFilters";
import {
    formatCurriculumPathLabel,
} from "../../lib/curriculumHierarchy";
import SprintLoPicker from "./sprintLoPicker";

const formatDateForInput = (date: Date): string => {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, "0");
    const day = date.getDate().toString().padStart(2, "0");
    return `${year}-${month}-${day}`;
};

interface CreateSprintProps {
    onSprintCreated?: () => void;
}

const CreateSprint = ({ onSprintCreated }: CreateSprintProps) => {
    const { query, pathname, push } = useRouter();
    const { role } = useAppSelector((s) => s.authSlice);

    const [active, setActive] = useState(false);
    const [sprintName, setSprintName] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState<string>(formatDateForInput(new Date()));
    const [endDate, setEndDate] = useState<string>(formatDateForInput(addDays(new Date(), 7)));
    const [selectedSubjectId, setSelectedSubjectId] = useState<string>('');

    const [subjects, setSubjects] = useState<IProject[]>([]);
    const [isLoadingSubjects, setIsLoadingSubjects] = useState<boolean>(false);
    const [subjectFetchError, setSubjectFetchError] = useState<string>('');

    const [selectedLos, setSelectedLos] = useState<IDName[]>([]);
    const [formError, setFormError] = useState('');

    const {
        filters,
        filterLabels,
        filterOptions,
        filteredSubjects,
        updateFilter,
        applyFilters,
        hasProjectSelected,
    } = useCurriculumPathFilters(subjects);

    useEffect(() => {
        if (query.form === "create-sprint") return setActive(true);
        setActive(false);
    }, [query]);

    useEffect(() => {
        if (!active) return;

        const next: Record<number, string> = {};
        if (typeof query.yearName === "string") next[0] = query.yearName;
        if (typeof query.projectName === "string") next[1] = query.projectName;
        if (typeof query.termName === "string") next[2] = query.termName;
        if (typeof query.subjectGroupName === "string") next[3] = query.subjectGroupName;

        if (Object.keys(next).length > 0) {
            applyFilters(next);
        }
    }, [active, applyFilters, query.yearName, query.projectName, query.termName, query.subjectGroupName]);

    useEffect(() => {
        if (active) {
            const fetchSubjects = async () => {
                setIsLoadingSubjects(true);
                setSubjectFetchError('');
                try {
                    const response = await API.PROJECTS.GET_ALL_FOR_SPRINT();
                    if (response && response.data && !response.error) {
                        setSubjects(response.data);
                    } else {
                        setSubjectFetchError(response?.message || "Failed to fetch subjects.");
                    }
                } catch (err) {
                    console.error("Error fetching subjects:", err);
                    setSubjectFetchError("An error occurred while fetching subjects.");
                } finally {
                    setIsLoadingSubjects(false);
                }
            };
            fetchSubjects();
        }
    }, [active]);

    useEffect(() => {
        if (!hasProjectSelected) {
            setSelectedSubjectId('');
            setSelectedLos([]);
            return;
        }

        if (
            selectedSubjectId &&
            !filteredSubjects.some((subject) => String(subject.id) === selectedSubjectId)
        ) {
            setSelectedSubjectId('');
        }
    }, [filters, filteredSubjects, hasProjectSelected, selectedSubjectId]);

    const scopeKey = useMemo(
        () => [filters[0], filters[1], filters[2], filters[3]].join("|"),
        [filters]
    );

    useEffect(() => {
        setSelectedLos([]);
        setSelectedSubjectId('');
    }, [scopeKey]);

    const selectedScopeLabel = useMemo(() => {
        const parts = [filters[0], filters[1], filters[2], filters[3]].filter(Boolean);
        return parts.join(" › ");
    }, [filters]);

    const handleSelectLos = (los: IDName[]) => {
        setSelectedLos((prev) => {
            const existingIds = new Set(prev.map((lo) => lo.id));
            return [...prev, ...los.filter((lo) => !existingIds.has(lo.id))];
        });
    };

    const handleExcelImported = (matched: IDName[]) => {
        handleSelectLos(matched);
    };

    const handleDeselectLo = (lo: IDName) => {
        setSelectedLos((prev) => prev.filter((selected) => selected.id !== lo.id));
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setFormError("");

        if (sprintName.trim() === "") return setFormError("Please enter a sprint name.");
        if (!hasProjectSelected) return setFormError("Please select a project from the hierarchy filters.");
        if (selectedLos.length === 0) return setFormError("Please select at least one learning outcome.");
        if (!startDate) return setFormError("Please select a start date.");
        if (!endDate) return setFormError("Please select an end date.");

        const dStartDate = new Date(startDate);
        const dEndDate = new Date(endDate);

        if (dEndDate <= dStartDate) {
            return setFormError("End date must be after start date.");
        }

        try {
            const response = await API.SPRINTS.CREATE_SPRINT({
                name: sprintName,
                description,
                startDate: dStartDate.toISOString(),
                endDate: dEndDate.toISOString(),
                los: selectedLos,
            });

            if (response && !response.error) {
                onSprintCreated?.();
                push(pathname);
            } else if (response.error) {
                setFormError(`Error: ${response.message}`);
            } else {
                setFormError("Failed to create sprint. Please try again.");
            }
        } catch (err) {
            console.error(err);
            setFormError("An error occurred. Please try again.");
        }
    };

    if (!active || (role !== 0 && role !== 4)) return null;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 w-screen min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white w-full max-w-5xl max-h-[90vh] overflow-y-auto rounded-lg shadow relative flex flex-col"
            >
                <div className="sticky top-0 z-10 bg-white px-6 pt-6 pb-2 rounded-t-lg">
                    <h2 className="text-2xl font-semibold text-center">Create New Sprint</h2>
                    {formError && (
                        <div className="mt-3 text-red-600 text-center text-sm p-2 bg-red-50 border border-red-300 rounded-lg flex items-center gap-2 justify-center">
                            <span className="text-red-500">⚠</span>
                            {formError}
                        </div>
                    )}
                </div>
                <div className="px-6 pb-6 overflow-y-auto flex-1">
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Sprint Name</label>
                        <input
                            type="text"
                            value={sprintName}
                            onChange={(e) => setSprintName(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            placeholder="Enter sprint name"
                        />
                    </div>

                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Description</label>
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            placeholder="Enter description"
                            rows={3}
                        />
                    </div>

                    <div>
                        <label className="block text-gray-700 font-medium mb-2">Curriculum Scope</label>
                        <p className="text-xs text-gray-500 mb-3">
                            Select the project scope first. Learning outcomes can only be added from subjects within the selected project.
                        </p>
                        {isLoadingSubjects && <p className="text-gray-500">Loading curriculum...</p>}
                        {subjectFetchError && <p className="text-red-500">{subjectFetchError}</p>}
                        {!isLoadingSubjects && !subjectFetchError && (
                            <CurriculumPathFilters
                                filterLabels={filterLabels}
                                filterOptions={filterOptions}
                                filters={filters}
                                onFilterChange={updateFilter}
                            />
                        )}
                        {hasProjectSelected && (
                            <p className="text-xs text-blue-700 mt-2">
                                Selected scope: {selectedScopeLabel}
                            </p>
                        )}
                    </div>

                    {hasProjectSelected && (
                        <div>
                            <label htmlFor="subject" className="block text-gray-700 font-medium mb-1">Subject</label>
                            <select
                                id="subject"
                                value={selectedSubjectId}
                                onChange={(e) => setSelectedSubjectId(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                                required
                            >
                                <option value="" disabled>Select a subject</option>
                                {filteredSubjects.length === 0 && (
                                    <option value="" disabled>No subjects available in this scope</option>
                                )}
                                {filteredSubjects.map((subject) => (
                                    <option key={subject.id} value={subject.id}>
                                        {subject.name}
                                        {subject.folderPath
                                            ? ` (${formatCurriculumPathLabel(subject.folderPath)})`
                                            : ""}
                                    </option>
                                ))}
                            </select>
                        </div>
                    )}

                    {selectedSubjectId && (
                        <SprintLoPicker
                            selectedLos={selectedLos}
                            selectedProjectId={selectedSubjectId}
                            onSelectLos={handleSelectLos}
                            onDeselectLo={handleDeselectLo}
                            onImported={handleExcelImported}
                        />
                    )}

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="startDate" className="block text-gray-700 font-medium mb-1">Start Date</label>
                            <input
                                type="date"
                                id="startDate"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="endDate" className="block text-gray-700 font-medium mb-1">End Date</label>
                            <input
                                type="date"
                                id="endDate"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                            />
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <div className="flex gap-4">
                            <button
                                type="button"
                                onClick={() => push(pathname)}
                                className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
                            >
                                Create Sprint
                            </button>
                        </div>
                    </div>
                </form>
                </div>
            </motion.div>
        </motion.div>
    );
};

export default CreateSprint;
