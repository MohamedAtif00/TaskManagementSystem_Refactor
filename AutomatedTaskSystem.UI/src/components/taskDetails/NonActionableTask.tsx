import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { ITask } from ".";
import TaskIcon from "../../assets/Icons/Task";
import API from "../../lib/API";
import ExpansionPanel from "../expansionPanel";
import Backdrop from "../forms/backdrop";

interface Props {
    projectId: number;
}

const NonActionableTaskDetails = ({ projectId }: Props) => {
    const [task, setTask] = useState<ITask>();
    const router = useRouter();

    useEffect(() => {
        const id = router.query.taskId;
        if (id)
            API.TASKS.GET_ONE(id).then((res) => {
                if (res && !res.error) {
                    setTask(res.data);
                }
            });
        else setTask(undefined);
    }, [setTask, router.query.taskId]);

    if (task === undefined) return <></>;

    return (
        <Backdrop mainRoute={`/summaries/${projectId}`}>
            <div className="bg-white px-8 py-8 rounded-lg cursor-default w-[30rem] max-h-[35rem]">
                <div className="text-slate-500 flex justify-between">
                    <div>{task.learningObjective.name}</div>
                    <div>{task.schema.name}</div>
                </div>
                <div className="select-none mt-2 flex justify-start items-center gap-4">
                    <h1 className="text-xl flex items-center gap-2">
                        <TaskIcon color="black" />
                        <div>{task.name}</div>
                    </h1>
                    <div
                        className={`px-3 py-1 rounded-3xl ${
                            task.status === 0
                                ? "bg-emerald-500 text-white"
                                : task.status === 1
                                ? "bg-orange-500 text-white"
                                : task.status === 4
                                ? "bg-black text-white"
                                : task.status === 2
                                ? "bg-blue-500 text-white"
                                : "border-2 border-black border-solid"
                        }`}
                    >
                        {task.status}
                    </div>
                </div>
                <ExpansionPanel header="Template" content={task.template} />
                <ExpansionPanel header="Tag" content={task.tag} />
                <ExpansionPanel
                    header="Environment"
                    content={task.environment}
                />
            </div>
        </Backdrop>
    );
};

export default NonActionableTaskDetails;
