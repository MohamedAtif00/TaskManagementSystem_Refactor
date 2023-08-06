import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import FormConclusion from "../../formComponents/FormConclusion";
import { useRouter } from "next/router";
import API, { BasicInfo } from "../../../lib/API";
import CustomizedCombobox from "../../formComponents/Combobox";

interface Props {
    projectId: number;
    refreshTasks: () => void;
}

const CreateStandAloneTaskForm: React.FC<Props> = (props) => {
    const { query, push: routerPush } = useRouter();
    const [active, setActive] = useState(false);
    const [lists, setLists] = useState<{
        learningObjectives: BasicInfo[];
        assignees: {
            id: number;
            name: string;
            group: BasicInfo;
        }[];
        options: {
            id: number;
            name: string;
            group: BasicInfo;
            teamLead: boolean;
        }[];
    }>();
    const [taskBank, setTaskBank] = useState<{
        id: number;
        name: string;
        group: BasicInfo;
        teamLead: boolean;
    }>();
    const [lo, setLo] = useState<BasicInfo>();
    const [user, setUser] = useState<{
        id: number;
        name: string;
        group: BasicInfo;
    }>();

    useEffect(() => {
        if (query.form === "new-task") {
            API.TASKS.GET_CREATABLE_TASKS(props.projectId).then((res) => {
                if (res && !res.error) setLists(res.data);
                return setActive(true);
            });
            return;
        }

        setLists(undefined);
        return setActive(false);
    }, [query, props.projectId]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (lo !== undefined && taskBank !== undefined)
            API.TASKS.ADD_TASK({
                userId: user ? user.id : 0,
                learningObjectiveId: lo.id,
                TaskBankItemId: taskBank.id,
            }).then(() => {
                props.refreshTasks();
                routerPush(`/tasks/${props.projectId}`);
            });
    };

    if (active && lists !== undefined)
        return (
            <motion.div
                initial={{ backgroundColor: "#00000000" }}
                animate={{ backgroundColor: "#00000055", height: "auto" }}
                className="z-50 flex items-center justify-center fixed top-0 left-0 right-0 min-h-screen"
            >
                <motion.div
                    initial={{ opacity: 0.1 }}
                    animate={{ opacity: 1 }}
                    className="bg-white px-5 py-4 basis-80 rounded-lg"
                >
                    <h2 className="text-lg mb-5">Add new Task</h2>
                    <form
                        onSubmit={handleSubmit}
                        className="flex flex-col gap-8"
                    >
                        <div className="flex flex-col gap-2">
                            <div>
                                <div className="text-sm">Task Option:</div>
                                <CustomizedCombobox
                                    value={taskBank}
                                    onChange={(e) => {
                                        setTaskBank(e);
                                        if (
                                            user &&
                                            user.group.id !== e.group.id
                                        ) {
                                            setUser(undefined);
                                        }
                                    }}
                                    options={lists.options}
                                />
                            </div>
                            <div>
                                <div className="text-sm">
                                    Learning Objective:
                                </div>
                                <CustomizedCombobox
                                    value={lo}
                                    onChange={(e) => setLo(e)}
                                    options={lists.learningObjectives}
                                />
                            </div>
                            <div>
                                <div className="text-sm">User:</div>
                                <CustomizedCombobox
                                    value={user}
                                    onChange={(e) => setUser(e)}
                                    options={
                                        taskBank
                                            ? lists.assignees.filter(
                                                  (u) =>
                                                      u.group.id ===
                                                      taskBank.group.id
                                              )
                                            : []
                                    }
                                />
                            </div>
                        </div>
                        <FormConclusion
                            pathname={`/tasks/${props.projectId}`}
                            submittable={true}
                        />
                    </form>
                </motion.div>
            </motion.div>
        );

    return <></>;
};

export default CreateStandAloneTaskForm;
