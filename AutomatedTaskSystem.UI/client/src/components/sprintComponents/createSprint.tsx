import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { motion } from "framer-motion";
import { addDays } from "date-fns";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome";
import SprintLoPicker from "./sprintLoPicker";

const formatDateForInput = (date: Date): string => {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
};

interface CreateSprintProps {
    onSprintCreated?: () => void;
}

const CreateSprint = ({ onSprintCreated }: CreateSprintProps) => {
    const { query, pathname, push } = useRouter();
    const { role } = useAppSelector((s) => s.authSlice);

    const [active, setActive] = useState(false);
    // Form fields state
    const [sprintName, setSprintName] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState<string>(formatDateForInput(new Date()));
    const [endDate, setEndDate] = useState<string>(formatDateForInput(addDays(new Date(), 7)));
    const [selectedProjectId, setSelectedProjectId] = useState<string>('');

    // State for projects list
    const [projects, setProjects] = useState<IProject[]>([]);
    const [isLoadingProjects, setIsLoadingProjects] = useState<boolean>(false);
    const [projectFetchError, setProjectFetchError] = useState<string>('');

    const [selectedLos, setSelectedLos] = useState<IDName[]>([]);

    // Error state for the form
    const [formError, setFormError] = useState('');

    useEffect(() => {
        if (query.form === "create-sprint") return setActive(true);
        setActive(false);
    }, [query]);

    useEffect(() => {
        if (active) {
            const fetchProjects = async () => {
                setIsLoadingProjects(true);
                setProjectFetchError('');
                try {
                    const response = await API.PROJECTS.GET_ALL_FOR_SPRINT(); 
                    if (response && response.data && !response.error) {
                        setProjects(response.data);
                    } else {
                        setProjectFetchError( response?.message || "Failed to fetch projects.");
                    }
                } catch (err) {
                    console.error("Error fetching projects:", err);
                    setProjectFetchError("An error occurred while fetching projects.");
                } finally {
                    setIsLoadingProjects(false);
                }
            };
            fetchProjects();
        }
    }, [active]);

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

    const resetForm = () => {
        setSprintName('');
        setDescription('');
        setStartDate(formatDateForInput(new Date()));
        setEndDate(formatDateForInput(addDays(new Date(), 7)));
        setSelectedProjectId('');
        setSelectedLos([]);
        setFormError('');
        setProjectFetchError('');
    };

    const closeModal = () => {
        resetForm();
        push(pathname);
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setFormError("");

        if (sprintName.trim() === "") return setFormError("Please enter a sprint name.");
        if (selectedLos.length === 0) return setFormError("Please select at least one learning outcome.");
        if (!startDate) return setFormError("Please select a start date.");
        if (!endDate) return setFormError("Please select an end date.");

        const dStartDate = new Date(startDate);
        const dEndDate = new Date(endDate);

        if (dEndDate <= dStartDate) {
            return setFormError("End date must be after start date.");
        }

        try {
            // IMPORTANT: Assume API.SPRINTS.CREATE_SPRINT now accepts projectId.
            // Adjust the payload if your API expects a different structure.
            const response = await API.SPRINTS.CREATE_SPRINT({
                name: sprintName,
                description,
                startDate: dStartDate.toISOString(),
                endDate: dEndDate.toISOString(),
                los: selectedLos
            });

            if (response && !response.error) {
                onSprintCreated?.();
                closeModal();
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
                        <label htmlFor="project" className="block text-gray-700 font-medium mb-1">Project</label>
                        {isLoadingProjects && <p className="text-gray-500">Loading projects...</p>}
                        {projectFetchError && <p className="text-red-500">{projectFetchError}</p>}
                        {!isLoadingProjects && !projectFetchError && (
                            <>
                                <select
                                    id="project"
                                    value={selectedProjectId}
                                    onChange={(e) => setSelectedProjectId(e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                                >
                                    <option value="" disabled>Select a project</option>
                                    {projects.length === 0 && !isLoadingProjects && <option value="" disabled>No projects available</option>}
                                    {projects.map((project) => (
                                        <option key={project.id} value={project.id}>
                                            {project.name}
                                        </option>
                                    ))}
                                </select>
                                <p className="text-xs text-gray-500 mt-1">You can switch projects to add learning outcomes from multiple sources.</p>
                            </>
                        )}
                    </div>

                    <SprintLoPicker
                        selectedLos={selectedLos}
                        selectedProjectId={selectedProjectId}
                        onSelectLos={handleSelectLos}
                        onDeselectLo={handleDeselectLo}
                        onImported={handleExcelImported}
                    />

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
                                onClick={closeModal}
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
