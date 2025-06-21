import { useEffect, useState } from "react";
import { useRouter } from "next/router";
import { motion } from "framer-motion";
import { addDays } from "date-fns";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";

// Define a Project interface (adjust based on your actual data structure)
interface Project {
    _id: string; // Or 'id: number' or similar, depending on your backend
    name: string;
}

// Helper function to format date for input type="date"
const formatDateForInput = (date: Date): string => {
    return date.toISOString().split('T')[0];
};

const CreateSprint = () => {
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
    const [projects, setProjects] = useState<Project[]>([]);
    const [isLoadingProjects, setIsLoadingProjects] = useState<boolean>(false);
    const [projectFetchError, setProjectFetchError] = useState<string>('');

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
                    // IMPORTANT: Assume API.PROJECTS.GET_ALL_PROJECTS() exists and
                    // returns an object like { data: Project[], error?: boolean, message?: string }
                    // Adjust this call and response handling to match your actual API.
                    const response = await API.PROJECTS.GET_ALL_PROJECTS(); 
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

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setFormError("");

        if (sprintName.trim() === "") return setFormError("Please enter a sprint name.");
        if (selectedProjectId === "") return setFormError("Please select a project.");
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
                projectId: selectedProjectId 
            });

            if (response && !response.error) {
                push(pathname); // Close modal
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

                <h2 className="text-2xl font-semibold text-center">Create New Sprint</h2>
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
                    <div>
                        <label htmlFor="project" className="block text-gray-700 font-medium mb-1">Project</label>
                        {isLoadingProjects && <p className="text-gray-500">Loading projects...</p>}
                        {projectFetchError && <p className="text-red-500">{projectFetchError}</p>}
                        {!isLoadingProjects && !projectFetchError && (
                            <select
                                id="project"
                                value={selectedProjectId}
                                onChange={(e) => setSelectedProjectId(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring focus:border-blue-500"
                                required
                            >
                                <option value="" disabled>Select a project</option>
                                {projects.length === 0 && !isLoadingProjects && <option value="" disabled>No projects available</option>}
                                {projects.map((project) => (
                                    <option key={project._id} value={project._id}>
                                        {project.name}
                                    </option>
                                ))}
                            </select>
                        )}
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
            </motion.div>
        </motion.div>
    );
};

export default CreateSprint;
