import { useRouter } from "next/router";
import { Fragment, useEffect, useRef, useState } from "react";
import { useAppSelector } from "../../../../app/hooks";
import TaskDetails from "../../../../components/taskDetails";
import API from "../../../../lib/API";
import Head from "next/head";
import Loader from "../../../../components/loader";
import CreateStandAloneTaskForm from "../../../../components/pageComponent/tasks/CreateStandAloneForm";
import TaskCol from "../../../../components/pageComponent/tasks/TasksCol";
import useTaskPathHandler from "../../../../components/taskDetails/useTaskPathHandler.ts";
import TaskIcon from "../../../../assets/Icons/Task";
import Link from "next/link";
import { Combobox, Transition } from "@headlessui/react";
import { CheckIcon, ChevronUpDownIcon } from "@heroicons/react/24/solid";

const TaskBoard = () => {
    const [tasks, setTasks] = useState<TaskInfo[]>();
    const [sprint, setSprint] = useState<ISprint>();
    const auth = useAppSelector((e) => e.authSlice);
    const router = useRouter();
    const pathHandler = useTaskPathHandler({type:"sprints"});
    const [selectedLo, setSelected] = useState<BasicInfo>({ id: 0, name: "None" });
    const [query, setQuery] = useState('');
	    const [searchType, setSearchType] = useState<'lo' | 'task'>('lo');
	    const latestRequestId = useRef(0);
     const { loid, loName } = router.query;

    useEffect(() => {
        const id = router.query.sprintId;
        if (id !== undefined)
            API.SPRINTS.GET_ONE(id).then(
                (res) => res && !res.error && setSprint(res.data)
            );
    }, [router.query.sprintId]);

     // After tasks load, apply the loid filter if present
    useEffect(() => {
        if (!tasks) return;

        if (loid) {
            const loId = Number(loid);
            const matchingLo = tasks.map(t => t.learningObjective).find(lo => lo.id === loId);
            if (matchingLo) {
                setSelected(matchingLo);
                setSearchType('lo');
            }
            return;
        }

        if (loName) {
            const loNameStr = Array.isArray(loName) ? loName[0] : loName;
            const target = String(loNameStr ?? '').trim().toLowerCase();
            if (!target) return;

            const matchingLo = tasks
                .map(t => t.learningObjective)
                .find(lo => lo?.name?.trim().toLowerCase() === target);

            if (matchingLo) {
                setSelected(matchingLo);
                setSearchType('lo');
            }
        }
    }, [loid, loName, tasks]);


    const loadTasks = async (id: string | string[]) => {
        const currentRequestId = ++latestRequestId.current;
        try {
            const allTasks = await API.SPRINTS.GET_ALL_CARDS_STREAM(
                id,
                (streamedTasks, totalCount) => {
                    // Ignore stale responses from earlier requests
                    if (currentRequestId !== latestRequestId.current) return;
                    // Progressive UI updates as tasks are accumulated
                    setTasks([...streamedTasks]);
                }
            );

            if (currentRequestId !== latestRequestId.current) return;
            setTasks(allTasks);
        } catch (error) {
            console.error('Error loading tasks:', error);
            if (currentRequestId !== latestRequestId.current) return;

            // Fallback to non-streaming endpoint if streaming fails
            const res = await API.SPRINTS.GET_ALL_CARDS(id);
            if (res && !res.error) {
                setTasks(res.data);
            }
        }
    };

    useEffect(() => {
        if (sprint) {
            loadTasks(sprint.id.toString());
        }
    }, [sprint]);

    useEffect(() => {
        if (sprint) {
            const refreshInterval = setInterval(() => {
                loadTasks(sprint.id.toString());
            }, 30000);
            return () => clearInterval(refreshInterval);
        }
    }, [sprint]);

    if (tasks === undefined)
        return (
            <div className="flex items-center justify-center mx-auto h-full">
                <Head>
                    <title>ATS - Loading</title>
                </Head>
                <Loader />
            </div>
        );

	    const refreshTasks = () => {
	        const sprintId = router.query.sprintId;
	        if (sprintId) {
	            loadTasks(sprintId);
	        }
	    };

    if (!sprint) return <div>Loading</div>;

    const los = tasks.map(t => t.learningObjective).filter((lo, idx, self) => {
        const element = self.find(_ => _.id == lo.id);
        if (element === undefined)
            return false;
        const elementId = self.indexOf(element);
        return elementId === idx;
    });

    const filteredTasks = query === ''
        ? tasks
        : searchType === 'task'
            ? tasks.filter((task) =>
                task.name
                    .toLowerCase()
                    .replaceAll(/\s+/g, '')
                    .includes(
                        query.toLowerCase()
                            .replaceAll(/\s+/g, '')
                    ))
            : tasks;

    const filteredLos = query === ''
        ? [{ id: 0, name: "None" }, ...los]
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
                        .replaceAll(/\s+/g, ''))
        );

    const selection = searchType === 'lo' 
        ? (selectedLo.id !== 0 ? tasks.filter(t => t.learningObjective.id === selectedLo.id) : tasks)
        : filteredTasks;

    const view = {
        backlog: selection.filter((t) => t.status === 0),
        todo: selection.filter((t) => t.status === 1),
        doing: selection.filter((t) => t.status === 2),
        done: selection.filter(
            (t) => t.status === 3 || t.status === 4
        ),
    };

    return (
        <>
            <Head>
                <title>ATS - {sprint.name} Tasks</title>
            </Head>
            <div className="w-full h-screen overflow-hidden flex flex-col">
                <div className="px-8 relative z-20">
                    <div className="sticky left-0 right-0 top-0 px-4 bg-white py-4 rounded-b-md border-solid border-2 border-sky-950 border-t-0 flex justify-between">
                        <div className="flex items-center gap-2">
                            <TaskIcon color={"#29313d"} />
                            <span className="text-2xl font-bold">{sprint.name}</span>
                        </div>
                        <div className="flex gap-4 items-center">
                            
                            {/* <div>
                                <Link href={{
                                    pathname: `/tasks/sprint/${sprint.id}/sheet`,
                                }} onClick={() => {
                                    localStorage.setItem("tasks:view", "sheet");
                                }}>
                                    <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-pink-700 hover:text-pink-700 transition ease-in">Sheet View</button>
                                </Link>
                            </div>
                            <div>
                                {auth.role !== 3 &&
                                    <Link href={{
                                        pathname: pathHandler(),
                                        query: {
                                            form: "new-task",
                                        },
                                    }}>
                                        <button className="px-4 py-1 bg-slate-50 rounded-md text-black border border-solid border-black text-sm hover:border-green-600 hover:text-green-600 transition ease-in">New Task</button>
                                    </Link>}
                            </div> */}
                            <div>
                                <form className="flex gap-4 items-end" onSubmit={e => { e.preventDefault(); }}>
                                    <label className="relative block">
                                        <div className="text-xs mb-1">Search type:</div>
                                        <select 
                                            className="w-32 border-none py-2 pl-3 pr-10 text-sm leading-5 text-gray-900 focus:ring-0 rounded-lg bg-white shadow-md"
                                            value={searchType}
                                            onChange={(e) => setSearchType(e.target.value as 'lo' | 'task')}
                                        >
                                            <option value="lo">Learning Objective</option>
                                            <option value="task">Task Name</option>
                                        </select>
                                    </label>
                                    {searchType === 'lo' ? (
                                        <label className="relative block">
                                            <div className="text-xs mb-1">Search LO:</div>
                                            <Combobox value={selectedLo} onChange={setSelected}>
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
                                                        <Combobox.Options className="absolute mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black/5 focus:outline-none sm:text-sm">
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
                                    ) : (
                                        <label className="relative block">
                                            <div className="text-xs mb-1">Search task name:</div>
                                            <input
                                                type="text"
                                                className="w-full border-none py-2 pl-3 pr-10 text-sm leading-5 text-gray-900 focus:ring-0 rounded-lg bg-white shadow-md"
                                                value={query}
                                                onChange={(e) => setQuery(e.target.value)}
                                                placeholder="Enter task name..."
                                            />
                                        </label>
                                    )}
                                </form>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="px-8 overflow-x-auto flex grow">
                    <div className="flex gap-1 bg-slate-50">
                        <TaskCol label="Backlog" items={view.backlog}  type={"task-sprint"}/>
                        <TaskCol label="To Do" items={view.todo}  type={"task-sprint"}/>
                        <TaskCol label="Doing" items={view.doing}  type={"task-sprint"}/>
                        <TaskCol label="Done" items={view.done}  type={"task-sprint"}/>
                    </div>
                </div>
                <TaskDetails
                    refreshTasks={refreshTasks}
                    type={"task-sprint"}
                />
                <CreateStandAloneTaskForm
                    refreshTasks={refreshTasks}
                    projectId={sprint.id}
                    type={"task-sprint"}
                />
            </div>
        </>
    );
};

export default TaskBoard;

