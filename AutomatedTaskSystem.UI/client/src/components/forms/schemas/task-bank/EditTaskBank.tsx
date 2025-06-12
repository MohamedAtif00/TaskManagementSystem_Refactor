import { useEffect, useState } from "react";
import API, { BasicInfo } from "../../../../lib/API";
import { ITaskBank } from "../../../../pages/schemas/task-bank";
import Backdrop from "../../backdrop";
import Checkbox from "../../checkbox";
import Dropdown from "../../dropdown";
import FormField from "../../field";
import HoursMinutes from "../../HoursMinutes";
import styles from "../../styles.module.scss";

const EditTaskBank = (props: {
    taskBank: ITaskBank;
    complete: (response: ITaskBank) => void;
}) => {
    const [name, setName] = useState(props.taskBank.name);
    const [teamLeader, setTeamLeader] = useState(props.taskBank.tl);
    const [group, setGroup] = useState(props.taskBank.group.id);
    const [type, setType] = useState(props.taskBank.type);
    const [duration, setDuration] = useState(props.taskBank.duration);
    const [groups, setGroups] = useState<BasicInfo[]>([]);

	// console.log(props.taskBank)

    useEffect(() => {
        API.RESOURCES.GROUPS.GET_ALL_MINI().then((res) => {
            if (res && !res.error) setGroups(res.data);
        });
    }, [setGroups]);

    const handleSumbit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        API.SCHEMAS.TASK_BANK.EDIT(props.taskBank.id.toString(), {
            group,
            type,
            name,
            tl: teamLeader,
            duration,
        }).then((res) => {
            if (res) props.complete(res);
        });
    };

    return (
        <Backdrop mainRoute="/schemas/task-bank">
            <div className={[styles.form, styles.center].join(" ")}>
                <form onSubmit={handleSumbit}>
                    <div className={styles.inputs}>
                        <FormField
                            label="Name"
                            value={name}
                            onChange={setName}
                        />
                        <HoursMinutes value={duration} setValue={setDuration} />
                        <Dropdown
                            handleChange={setGroup}
                            id={group}
                            options={groups}
                            label="Group"
                        />
                        <Dropdown
                            handleChange={(v) => {
                                setType(v as TaskBankType);
                            }}
                            id={type}
                            options={[
                                { id: 0, name: "Creation" },
                                { id: 1, name: "Review" },
                            ]}
                            label="Type"
                        />
                        <Checkbox
                            label="Team Leader"
                            active={teamLeader}
                            toggle={() => setTeamLeader((ps) => !ps)}
                        />
                    </div>
                    <div>
                        <input
                            type="submit"
                            value="Add"
                            className={[styles.submit].join(" ")}
                        />
                    </div>
                </form>
            </div>
        </Backdrop>
    );
};

export default EditTaskBank;
