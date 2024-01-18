import { useEffect, useState } from "react";
import API from "../../../lib/API";
import { ITaskBank } from "../../../pages/schemas/task-bank";
import Backdrop from "../backdrop";
import Dropdown from "../dropdown";
import styles from "../styles.module.scss";

const CreateTask = (props: { refresh: () => void; projectId: number }) => {
    const [user, setUser] = useState(0);
    const [users, setUsers] = useState<IUser[]>([]);
    const [lo, setLo] = useState(0);
    const [los, setLos] = useState<{ id: number; name: string }[]>([]);
    const [taskBankItem, setTaskBankItem] = useState(0);
    const [taskBank, setTaskBank] = useState<ITaskBank[]>([]);

    useEffect(() => {
        API.PROJECTS.USERS_ASSIGNED(`${props.projectId}`).then((res) => {
            if (res && !res.error) setUsers(res.data);
        });
    }, [setUsers, props.projectId]);

    useEffect(() => {
        API.SCHEMAS.TASK_BANK.GET_ALL().then((res) => {
            if (res) setTaskBank(res);
        });
    }, [setTaskBank]);

    useEffect(() => {
        API.PROJECTS.GET_ALL_LOS(props.projectId).then((res) => {
            if (res && !res.error) setLos(res.data);
        });
    }, [props.projectId, setLos]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (lo !== 0 && taskBankItem !== 0)
            API.TASKS.ADD_TASK({
                userId: user,
                learningObjectiveId: lo,
                TaskBankItemId: taskBankItem,
            }).then(() => {
                props.refresh();
            });
    };

    const selectTaskBankItem = taskBank.find((t) => t.id === taskBankItem);

    return (
        <Backdrop mainRoute={`/tasks/${props.projectId}`}>
            <div className={styles.form}>
                <form onSubmit={handleSubmit}>
                    <div className={styles.inputs}>
                        <Dropdown
                            label="Task Bank Item"
                            id={taskBankItem}
                            options={taskBank}
                            handleChange={setTaskBankItem}
                        />
                        <Dropdown
                            label="Learning Objective"
                            id={lo}
                            options={los}
                            handleChange={setLo}
                        />
                        {selectTaskBankItem != null ? (
                            <Dropdown
                                label="User"
                                id={user}
                                options={[
                                    { id: 0, name: "None" },
                                    ...users.filter(
                                        (u) =>
                                            u.group.id ===
                                            selectTaskBankItem.group.id
                                    ),
                                ]}
                                handleChange={setUser}
                            />
                        ) : (
                            ""
                        )}
                    </div>
                    <div>
                        <input
                            type="submit"
                            value="Add"
                            className={[
                                styles.submit,
                                lo !== 0 && taskBankItem !== 0
                                    ? ""
                                    : styles.inactive,
                            ].join(" ")}
                        />
                    </div>
                </form>
            </div>
        </Backdrop>
    );
};

export default CreateTask;
