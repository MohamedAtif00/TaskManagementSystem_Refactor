import { motion } from "framer-motion";
import { useRouter } from "next/router";
import { useState, useEffect, useMemo } from "react";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome";
import CurriculumPathFilters from "../curriculum/CurriculumPathFilters";
import { useCurriculumPathFilters } from "../../hooks/useCurriculumPathFilters";
import { formatCurriculumPathLabel, getCurriculumPathParts, parseCurriculumPath } from "../../lib/curriculumHierarchy";

const formatDateForInput = (date: Date): string => {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
};

interface EditSprintProps {
    sprintId: string;
    onSprintUpdated: () => void;
}

const EditSprint = ({ sprintId, onSprintUpdated }: EditSprintProps) => {
    const { query, pathname, push } = useRouter();
    const { role } = useAppSelector((s) => s.authSlice);

    const [active, setActive] = useState(false);
    const [sprintName, setSprintName] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState<string>('');
    const [endDate, setEndDate] = useState<string>('');
    const [selectedSubjectId, setSelectedSubjectId] = useState<string>('');
    const [scopeLocked, setScopeLocked] = useState(false);

    const [subjects, setSubjects] = useState<IProject[]>([]);
    const [isLoadingSubjects, setIsLoadingSubjects] = useState<boolean>(false);
    const [subjectFetchError, setSubjectFetchError] = useState<string>('');

    const [learningOutcomes, setLearningOutcomes] = useState<IDName[]>([]);
    const [isLoadingLOs, setIsLoadingLOs] = useState<boolean>(false);
    const [loFetchError, setLoFetchError] = useState<string>('');
    const [selectedLos, setSelectedLos] = useState<IDName[]>([]);
    const [formError, setFormError] = useState('');
    const [pendingScopePath, setPendingScopePath] = useState<string | null>(null);
    const [pendingProjectName, setPendingProjectName] = useState<string | null>(null);

    const {
        filters,
        filterLabels,
        filterOptions,
        filteredSubjects,
        updateFilter,
        setFiltersFromPath,
        applyFilters,
        hasProjectSelected,
    } = useCurriculumPathFilters(subjects);

    useEffect(() => {
        if (query.form === "edit-sprint") {
            setActive(true);
        } else {
            setActive(false);
        }
    }, [query]);

    useEffect(() => {
        if (active && sprintId) {
            API.SPRINTS.GET_ONE(sprintId).then(res => {
                if (res && res.data && !res.error) {
                    const sprint = res.data;
                    setSprintName(sprint.name);
                    setDescription(sprint.description || '');
                    setStartDate(formatDateForInput(new Date(sprint.startDate)));
                    setEndDate(formatDateForInput(new Date(sprint.endDate)));
                    setSelectedLos(sprint.learningObjects || []);
                    if (sprint.scopeFolderPath) {
                        setPendingScopePath(sprint.scopeFolderPath);
                        setScopeLocked(true);
                    } else if (sprint.projectNames?.length === 1) {
                        setPendingProjectName(sprint.projectNames[0]);
                        setScopeLocked(true);
                    }
                } else {
                    setFormError(res?.message || "Failed to load sprint data.");
                }
            });
        }
    }, [active, sprintId]);

    useEffect(() => {
        if (!active) return;

        if (pendingScopePath) {
            setFiltersFromPath(pendingScopePath);
            return;
        }

        if (pendingProjectName && subjects.length > 0) {
            const match = subjects.find(
                (subject) => parseCurriculumPath(subject.folderPath).project === pendingProjectName
            );
            if (match?.folderPath) {
                const parts = getCurriculumPathParts(match.folderPath);
                const next: Record<number, string> = {};
                if (parts[0]) next[0] = parts[0];
                if (parts[1]) next[1] = parts[1];
                applyFilters(next);
            }
        }
    }, [active, applyFilters, pendingProjectName, pendingScopePath, setFiltersFromPath, subjects]);

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
            setLearningOutcomes([]);
            return;
        }

        if (
            selectedSubjectId &&
            !filteredSubjects.some((subject) => String(subject.id) === selectedSubjectId)
        ) {
            setSelectedSubjectId('');
            setLearningOutcomes([]);
        }
    }, [filters, filteredSubjects, hasProjectSelected, selectedSubjectId]);

    useEffect(() => {
        if (selectedSubjectId) {
            const fetchLOs = async () => {
                setIsLoadingLOs(true);
                setLoFetchError('');
                setLearningOutcomes([]);
                try {
                    const response = await API.PROJECTS.GET_ALL_LOS(Number(selectedSubjectId));
                    if (response && response.data && !response.error) {
                        setLearningOutcomes(response.data);
                    } else {
                        setLoFetchError(response?.message || "Failed to fetch learning outcomes.");
                    }
                } catch (err) {
                    console.error("Error fetching learning outcomes:", err);
                    setLoFetchError("An error occurred while fetching learning outcomes.");
                } finally {
                    setIsLoadingLOs(false);
                }
            };
            fetchLOs();
        }
    }, [selectedSubjectId]);

    const selectedScopeLabel = useMemo(() => {
        const parts = [filters[0], filters[1], filters[2], filters[3]].filter(Boolean);
        return parts.join(" › ");
    }, [filters]);

    const handleFilterChange = (index: number, value: string) => {
        if (scopeLocked && index <= 1 && selectedLos.length > 0) {
            return;
        }
        updateFilter(index, value);
    };

    const handleSelectLo = (lo: IDName) => {
        if (!selectedLos.some(selected => selected.id === lo.id)) {
            setSelectedLos([...selectedLos, lo]);
        }
    };

    const handleDeselectLo = (lo: IDName) => {
        setSelectedLos(selectedLos.filter(selected => selected.id !== lo.id));
    };

    const closeModal = () => {
        const newQuery = { ...query };
        delete newQuery.form;
        push({
            pathname: pathname,
            query: newQuery,
        });
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
            const losToSend = selectedLos.map(lo => lo.id);

            const response = await API.SPRINTS.UPDATE_SPRINT(sprintId, {
                name: sprintName,
                description,
                startDate: dStartDate.toISOString(),
                endDate: dEndDate.toISOString(),
                los: losToSend,
            });

            if (response && !response.error) {
                onSprintUpdated();
                closeModal();
            } else if (response.error) {
                setFormError(`Error: ${response.message}`);
            } else {
                setFormError("Failed to update sprint. Please try again.");
            }
        } catch (err) {
            console.error(err);
            setFormError("An error occurred. Please try again.");
        }
    };

    if (!active || (role !== 2 && role !== 0)) return null;

    return (
        <motion.div
            initial={{ backgroundColor: "#00000000" }}
            animate={{ backgroundColor: "#00000055", height: "auto" }}
            className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 w-screen min-h-screen"
        >
            <motion.div
                initial={{ opacity: 0.1 }}
                animate={{ opacity: 1 }}
                className="bg-white p-6 w-full max-w-5xl max-h-[90vh] overflow-y-auto rounded-lg shadow space-y-6"
            >
                <h2 className="text-2xl font-semibold text-center">Edit Sprint</h2>
                {formError && <div className="text-red-500 text-center mb-4 p-2 bg-red-100 border border-red-400 rounded">{formError}</div>}

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
                            Learning outcomes can only be added from subjects within the selected project.
                            {scopeLocked && " The project scope is locked based on existing sprint learning outcomes."}
                        </p>
                        {isLoadingSubjects && <p className="text-gray-500">Loading curriculum...</p>}
                        {subjectFetchError && <p className="text-red-500">{subjectFetchError}</p>}
                        {!isLoadingSubjects && !subjectFetchError && (
                            <CurriculumPathFilters
                                filterLabels={filterLabels}
                                filterOptions={filterOptions}
                                filters={filters}
                                onFilterChange={handleFilterChange}
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
                            >
                                <option value="">Select a subject to add more LOs</option>
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

                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Learning Outcomes</label>
                        {isLoadingLOs && selectedSubjectId && <p className="text-gray-500">Loading learning outcomes...</p>}
                        {loFetchError && <p className="text-red-500">{loFetchError}</p>}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-lg">
                            <div>
                                <h4 className="font-semibold mb-2 text-gray-800">Available from Subject</h4>
                                <div className="h-48 overflow-y-auto border rounded p-2 space-y-1 bg-gray-50">
                                    {learningOutcomes.filter(lo => !selectedLos.some(selected => selected.id === lo.id)).map(lo => (
                                        <div key={lo.id} onClick={() => handleSelectLo(lo)} className="p-2 border rounded cursor-pointer hover:bg-blue-100 transition-colors">
                                            {lo.name}
                                        </div>
                                    ))}
                                    {learningOutcomes.filter(lo => !selectedLos.some(selected => selected.id === lo.id)).length === 0 && selectedSubjectId && (
                                        <p className="text-gray-500 text-sm p-2">All available LOs from this subject are selected.</p>
                                    )}
                                    {!selectedSubjectId && !isLoadingLOs && (
                                        <p className="text-gray-500 text-sm p-2">Select a subject to see available LOs.</p>
                                    )}
                                </div>
                            </div>
                            <div>
                                <h4 className="font-semibold mb-2 text-gray-800">Selected for Sprint ({selectedLos.length})</h4>
                                <div className="h-48 overflow-y-auto border rounded p-2 space-y-1 bg-blue-50">
                                    {selectedLos.map(lo => (
                                        <div key={lo.id} onClick={() => handleDeselectLo(lo)} className="p-2 border rounded cursor-pointer hover:bg-red-100 transition-colors flex justify-between items-center bg-white">
                                            <span>{lo.name}</span>
                                            <span className="text-red-500 font-bold text-lg leading-none">&times;</span>
                                        </div>
                                    ))}
                                    {selectedLos.length === 0 && (
                                        <p className="text-gray-500 text-sm p-2">Click on an available LO to select it.</p>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="startDate" className="block text-gray-700 font-medium mb-1">Start Date</label>
                            <input type="date" id="startDate" value={startDate} onChange={(e) => setStartDate(e.target.value)} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500" />
                        </div>
                        <div>
                            <label htmlFor="endDate" className="block text-gray-700 font-medium mb-1">End Date</label>
                            <input type="date" id="endDate" value={endDate} onChange={(e) => setEndDate(e.target.value)} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500" />
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <div className="flex gap-4">
                            <button type="button" onClick={closeModal} className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600">
                                Cancel
                            </button>
                            <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
                                Update Sprint
                            </button>
                        </div>
                    </div>
                </form>
            </motion.div>
        </motion.div>
    );
};

export default EditSprint;
