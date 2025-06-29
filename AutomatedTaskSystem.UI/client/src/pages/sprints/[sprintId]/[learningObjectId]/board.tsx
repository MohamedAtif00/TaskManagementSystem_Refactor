// pages/sprints/[sprintId]/board.tsx (or [learningObjectId]/board.tsx if that's the true parent)

import { Fragment, useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../../lib/API";
import Head from "next/head";
import Loader from "../../../../components/loader";
import CreateStandAloneTaskForm from "../../../../components/pageComponent/tasks/CreateStandAloneForm";
import TaskCol from "../../../../components/pageComponent/tasks/TasksCol";
import TaskIcon from "../../../../assets/Icons/Task";
import Link from "next/link";
import { Combobox, Transition } from "@headlessui/react";
import { CheckIcon, ChevronUpDownIcon } from "@heroicons/react/24/solid";
import TaskDetails from "../../../../components/taskDetails";


const TaskBoard = () => {
    const [tasks, setTasks] = useState<TaskInfo[] | undefined>(undefined);
    const [project, setProject] = useState<ISprint | undefined>(undefined); // This 'project' will still be the sprint
    const [error, setError] = useState<string | null>(null);
    const router = useRouter();

    // The current URL structure is /sprints/[sprintId]/[learningObjectId]/board
    // So both sprintId and learningObjectId come from the route.
    const { sprintId, learningObjectId } = router.query;

    const [selectedLo, setSelectedLo] = useState<BasicInfo>({ id: 0, name: "All" });
    const [query, setQuery] = useState('');

    // Effect to fetch sprint details (still needs sprintId)
    useEffect(() => {
        if (!router.isReady || typeof sprintId !== 'string') {
            return;
        }

        // Fetch sprint details using sprintId
        API.SPRINTS.GET_ONE(sprintId).then(
            res => {
                if (res && !res.error && res.data) {
                    setProject(res.data);
                    // Set initial LO filter from URL if present and valid
                    if (typeof learningObjectId === 'string' && Number(learningObjectId) !== 0 && res.data.learningObjects) {
                        const initialLo = res.data.learningObjects.find(
                            (lo: BasicInfo) => lo.id === Number(learningObjectId)
                        );
                        if (initialLo) {
                            setSelectedLo(initialLo);
                        }
                    } else {
                        setSelectedLo({ id: 0, name: "All" }); // Default to "All" if no LO in URL or it's 0
                    }
                } else {
                    const errorMessage = res?.message || "Failed to load sprint details.";
                    console.error(errorMessage);
                    setError(errorMessage);
                }
            }
        );
    }, [router.isReady, sprintId, learningObjectId]); // Depend on router.isReady, sprintId, and learningObjectId

    // Function to fetch tasks, **now only taking the learningObjectiveId based on your API's definition**
    const fetchTasks = async (loIdInPath: string) => { // This parameter maps to `learningObjectId` in your API definition
        if (!loIdInPath ||!sprintId) { // Ensure the ID for the path is not empty
            console.warn("No learning objective ID provided for fetching tasks.");
            setTasks([]); // Set to empty to clear previous tasks if no LO is selected
            return;
        }

        const res = await API.SPRINTS.GET_ALL_CARDS_FOR_LO(loIdInPath,sprintId); // Call API with the LO ID

        if (res && !res.error) {
            setTasks(res.data);
        } else {
            console.error("Failed to load tasks:", res?.message || "Unknown error");
            setError(res?.message || "Failed to load tasks."); // Set error for tasks loading
            setTasks([]); // Clear tasks on error
        }
    };

    // Effect to fetch tasks initially and set up refresh interval
    useEffect(() => {
        // Only fetch tasks if router is ready and `learningObjectId` is a string
        if (!router.isReady || typeof learningObjectId !== 'string') {
            setTasks(undefined); // Reset tasks while waiting or if invalid LO ID
            return;
        }

        // Use the `learningObjectId` from the URL as the ID for the API call
        // This implicitly filters tasks by this LO if the backend supports it at that route.
        fetchTasks(learningObjectId);

        const refreshInterval = setInterval(() => fetchTasks(learningObjectId), 30000);
        return () => clearInterval(refreshInterval);
    }, [router.isReady, learningObjectId]); // Depend only on learningObjectId (and router.isReady) for task fetching

    // Loading State
    if (tasks === undefined || project === undefined) { // Check both project and tasks for loading
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );
    }

    // Error State
    if (error) {
        return (
            <div className="flex items-center justify-center mx-auto h-full text-red-500">
                <Head>
                    <title>ATS - Error</title>
                </Head>
                <p>{error}</p>
            </div>
        );
    }

    // Derive unique Learning Objectives from tasks (still relevant for the Combobox options)
    const los = tasks
        .map(t => t.learningObjective)
        .filter((lo): lo is BasicInfo => lo !== null && lo !== undefined)
        .filter((lo, idx, self) => self.findIndex(_ => _.id === lo.id) === idx);

    // Filtered LOs for the Combobox, including "All"
    const filteredLos =
        query === ''
            ? [{ id: 0, name: "All" }, ...los]
            : los.filter((lo) =>
                lo.name
                    .toLowerCase()
                    .replaceAll("0", "")
                    .replaceAll("_", "")
                    .replaceAll(/\s+/g, '')
                    .includes(
                        query.toLowerCase()
                            .replaceAll("0", "")
                            .replaceAll("_", "")
                            .replaceAll(/\s+/g, '')
                    )
            );

    // This handler will now cause a route change when an LO is selected in the Combobox.
    const handleLoSelectionChange = (newLo: BasicInfo) => {
        setSelectedLo(newLo); // Update local state for Combobox display
        
        const currentSprintId = typeof sprintId === 'string' ? sprintId : '';
        const newLoIdForUrl = newLo.id === 0 ? '0' : newLo.id.toString(); // Use '0' or empty string for "All" in URL

        // Construct new path: /sprints/[sprintId]/[newLoIdForUrl]/board
        // This will trigger a re-render and re-fetch of tasks based on the new learningObjectId in the URL.
        const newPath = `/sprints/${currentSprintId}/${newLoIdForUrl}/board`;
        router.push(newPath, undefined, { shallow: true }); // Shallow routing prevents full page reload if only query changes
    };

    // Tasks are already filtered by the API call based on `learningObjectId` from the URL.
    // The `view` object just organizes these already filtered tasks by status.
    const view = {
        backlog: tasks.filter((t) => t.status === 0),
        todo: tasks.filter((t) => t.status === 1),
        doing: tasks.filter((t) => t.status === 2),
        done: tasks.filter(
            (t) => t.status === 3 || t.status === 4
        ),
    };

    // refreshTasks will re-fetch based on the current `learningObjectId` in the URL
    const refreshTasks = () => {
        const currentLoIdForRefresh = typeof learningObjectId === 'string' ? learningObjectId : '0'; // Use '0' if no LO ID in URL
        fetchTasks(currentLoIdForRefresh);
    };

    return (
        <>
            <Head>
                <title>ATS - {project.name} Tasks</title>
            </Head>
            <div className="w-full h-screen overflow-hidden flex flex-col">
                <div className="px-8 relative z-20">
                    <div className="sticky left-0 right-0 top-0 px-4 bg-white py-4 rounded-b-md border-solid border-2 border-sky-950 border-t-0 flex justify-between">
                        <div className="flex items-center gap-2">
                            <TaskIcon color={"#29313d"} />
                            <span className="text-2xl font-bold">{project.name}</span>
                        </div>
                        <div className="flex gap-4 items-center">
                            {/* Link to Sheet View - Ensure both sprintId and learningObjectId are passed */}
                            <Link href={
                                typeof sprintId === 'string' && typeof learningObjectId === 'string'
                                    ? `/sprints/${sprintId}/${learningObjectId}/sheet`
                                    : `/sprints/${sprintId}/sheet` // Fallback if learningObjectId is not valid (e.g., initially undefined)
                            }>
                                <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-pink-700 hover:text-pink-700 transition ease-in">
                                    Sheet View
                                </button>
                            </Link>
                            {/* LO Filter Combobox */}
                            <div>
                                <form className="flex gap-4 items-end" onSubmit={e => { e.preventDefault(); }}>
                                    <label className="relative block">
                                        <div className="text-xs mb-1">Filter by LO:</div>
                                        <Combobox value={selectedLo} onChange={handleLoSelectionChange}>
                                            <div className="relative mt-1">
                                                <div className="relative w-full cursor-default overflow-hidden rounded-lg bg-white text-left shadow-md focus:outline-none focus-visible:ring-2 focus-visible:ring-white/75 focus-visible:ring-offset-2 focus-visible:ring-offset-blue-300 sm:text-sm">
                                                    <Combobox.Input
                                                        className="w-full border-none py-2 pl-3 pr-10 text-sm leading-5 text-gray-900 focus:ring-0"
                                                        displayValue={(item: BasicInfo) => item.name}
                                                        onChange={(event) => setQuery(event.target.value)}
                                                    />
                                                    <Combobox.Button className="absolute inset-y-0 right-0 flex items-center pr-2">
                                                        <ChevronUpDownIcon
                                                            className="h-5 w-5 text-gray-400"
                                                            aria-hidden="true"
                                                        />
                                                    </Combobox.Button>
                                                </div>
                                                <Transition
                                                    as={Fragment}
                                                    leave="transition ease-in duration-100"
                                                    leaveFrom="opacity-100"
                                                    leaveTo="opacity-0"
                                                    afterLeave={() => setQuery('')}
                                                >
                                                    <Combobox.Options className="absolute mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black/5 focus:outline-none sm:text-sm z-50">
                                                        {filteredLos.length === 0 && query !== '' ? (
                                                            <div className="relative cursor-default select-none px-4 py-2 text-gray-700">
                                                                Nothing found.
                                                            </div>
                                                        ) : (
                                                            filteredLos.map((item) => (
                                                                <Combobox.Option
                                                                    key={item.id}
                                                                    className={({ active }) =>
                                                                        `relative cursor-default select-none py-2 pl-10 pr-4 ${active ? 'bg-blue-600 text-white' : 'text-gray-900'
                                                                        }`
                                                                    }
                                                                    value={item}
                                                                >
                                                                    {({ selected, active }) => (
                                                                        <>
                                                                            <span
                                                                                className={`block truncate ${selected ? 'font-medium' : 'font-normal'
                                                                                    }`}
                                                                            >
                                                                                {item.name}
                                                                            </span>
                                                                            {selected ? (
                                                                                <span
                                                                                    className={`absolute inset-y-0 left-0 flex items-center pl-3 ${active ? 'text-white' : 'text-blue-600'
                                                                                        }`}
                                                                                >
                                                                                    <CheckIcon className="h-5 w-5" aria-hidden="true" />
                                                                                </span>
                                                                            ) : null}
                                                                        </>
                                                                    )}
                                                                </Combobox.Option>
                                                            ))
                                                        )}
                                                    </Combobox.Options>
                                                </Transition>
                                            </div>
                                        </Combobox>
                                    </label>
                                </form>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="px-8 overflow-x-auto flex grow">
                    <div className="flex gap-1 bg-slate-50">
                        <TaskCol label="Backlog" items={view.backlog} type={"sprints"} />
                        <TaskCol label="To Do" items={view.todo} type={"sprints"} />
                        <TaskCol label="Doing" items={view.doing} type={"sprints"} />
                        <TaskCol label="Done" items={view.done} type={"sprints"} />
                    </div>
                </div>
                <TaskDetails
                    refreshTasks={refreshTasks}
                    type={"sprints"}
                />
                <CreateStandAloneTaskForm
                    refreshTasks={refreshTasks}
                    projectId={project.id}
                    type={"sprints"}
                />
            </div>
        </>
    );
};

export default TaskBoard;