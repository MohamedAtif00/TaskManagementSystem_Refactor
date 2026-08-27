import { motion } from "framer-motion";
import { useRouter } from "next/router";
import { useState, useEffect } from "react";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { IDName } from "../../lib/API/workFromHome"; // Assuming IDName is defined here

// Define IProject interface if it's not already globally available
interface IProject {
    id: number;
    name: string;
    // Add other project properties as needed
}

// Helper function to format date for input type="date"
const formatDateForInput = (date: Date): string => {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0'); // getMonth() is zero-based
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
    // Form fields state
    const [sprintName, setSprintName] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState<string>('');
    const [endDate, setEndDate] = useState<string>('');
    const [selectedProjectId, setSelectedProjectId] = useState<string>('');

    // State for projects list
    const [projects, setProjects] = useState<IProject[]>([]);
    const [isLoadingProjects, setIsLoadingProjects] = useState<boolean>(false);
    const [projectFetchError, setProjectFetchError] = useState<string>('');

    // State for Learning Outcomes list
    const [learningOutcomes, setLearningOutcomes] = useState<IDName[]>([]);
    const [isLoadingLOs, setIsLoadingLOs] = useState<boolean>(false);
    const [loFetchError, setLoFetchError] = useState<string>('');

    // ⭐ FIX: Change selectedLos type back to IDName[]
    // This state will hold the full LO objects (id and name) for display and selection logic
    const [selectedLos, setSelectedLos] = useState<IDName[]>([]);

    // Error state for the form
    const [formError, setFormError] = useState('');

    useEffect(() => {
        if (query.form === "edit-sprint") {
            setActive(true);
        } else {
            setActive(false);
        }
    }, [query]);

    // Fetch existing sprint data when modal becomes active
    useEffect(() => {
        if (active && sprintId) {
            API.SPRINTS.GET_ONE(sprintId).then(res => {
                if (res && res.data && !res.error) {
                    const sprint = res.data;
                    setSprintName(sprint.name);
                    setDescription(sprint.description || '');
                    setStartDate(formatDateForInput(new Date(sprint.startDate)));
                    setEndDate(formatDateForInput(new Date(sprint.endDate)));
                    // This assignment is now correct as selectedLos is IDName[]
                    setSelectedLos(sprint.learningObjects || []);
                } else {
                    setFormError(res?.message || "Failed to load sprint data.");
                }
            });
        }
    }, [active, sprintId]);

    // Fetch projects when modal is active
    useEffect(() => {
        if (active) {
            const fetchProjects = async () => {
                setIsLoadingProjects(true);
                setProjectFetchError('');
                try {
                    // Assuming API.PROJECTS.GET_ALL_FOR_SPRINT() exists and returns IProject[]
                    const response = await API.PROJECTS.GET_ALL_FOR_SPRINT();
                    if (response && response.data && !response.error) {
                        setProjects(response.data);
                    } else {
                        setProjectFetchError(response?.message || "Failed to fetch projects.");
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

    // Fetch LOs when a project is selected
    useEffect(() => {
        if (selectedProjectId) {
            const fetchLOs = async () => {
                setIsLoadingLOs(true);
                setLoFetchError('');
                setLearningOutcomes([]); // Reset previous LOs
                try {
                    // Assuming API.PROJECTS.GET_ALL_LOS() exists and returns IDName[]
                    const response = await API.PROJECTS.GET_ALL_LOS(Number(selectedProjectId));
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
    }, [selectedProjectId]);

    // handleSelectLo and handleDeselectLo now correctly operate on IDName objects
    const handleSelectLo = (lo: IDName) => {
        // Check if the LO is not already in selectedLos based on its ID
        if (!selectedLos.some(selected => selected.id === lo.id)) {
            setSelectedLos([...selectedLos, lo]);
        }
    };

    const handleDeselectLo = (lo: IDName) => {
        // Filter out the LO with the matching ID
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
        if (selectedLos.length === 0) return setFormError("Please select at least one learning outcome.");
        if (!startDate) return setFormError("Please select a start date.");
        if (!endDate) return setFormError("Please select an end date.");

        const dStartDate = new Date(startDate);
        const dEndDate = new Date(endDate);

        if (dEndDate <= dStartDate) {
            return setFormError("End date must be after start date.");
        }

        try {
            // ⭐ CRITICAL FIX: Map the IDName[] to number[] ONLY when sending to the API
            const losToSend = selectedLos.map(lo => lo.id);

            const response = await API.SPRINTS.UPDATE_SPRINT(sprintId, {
                name: sprintName,
                description,
                startDate: dStartDate.toISOString(), // Send as ISO string
                endDate: dEndDate.toISOString(),     // Send as ISO string
                los: losToSend // Pass the array of numbers
            });

            if (response && !response.error) {
                onSprintUpdated(); // Callback to refresh the parent page
                closeModal(); // Close modal by removing query param
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
                        <label htmlFor="project" className="block text-gray-700 font-medium mb-1">Project (for adding more Learning Outcomes)</label>
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
                                    <option value="" disabled>Select a project to see its LOs</option>
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

                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Learning Outcomes</label>
                        {isLoadingLOs && selectedProjectId && <p className="text-gray-500">Loading learning outcomes...</p>}
                        {loFetchError && <p className="text-red-500">{loFetchError}</p>}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-lg">
                            <div>
                                <h4 className="font-semibold mb-2 text-gray-800">Available from Project</h4>
                                <div className="h-48 overflow-y-auto border rounded p-2 space-y-1 bg-gray-50">
                                    {/* Filter available LOs: show only those NOT in selectedLos */}
                                    {learningOutcomes.filter(lo => !selectedLos.some(selected => selected.id === lo.id)).map(lo => (
                                        <div key={lo.id} onClick={() => handleSelectLo(lo)} className="p-2 border rounded cursor-pointer hover:bg-blue-100 transition-colors">
                                            {lo.name}
                                        </div>
                                    ))}
                                    {learningOutcomes.filter(lo => !selectedLos.some(selected => selected.id === lo.id)).length === 0 && selectedProjectId && (
                                        <p className="text-gray-500 text-sm p-2">All available LOs from this project are selected.</p>
                                    )}
                                    {!selectedProjectId && !isLoadingLOs && (
                                        <p className="text-gray-500 text-sm p-2">Select a project to see available LOs.</p>
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
