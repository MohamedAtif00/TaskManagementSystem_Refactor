import { useEffect, useState } from "react";
import API, { BasicInfo } from "../../../../lib/API";
import { ITaskBank } from "../../../../pages/schemas/task-bank";
import Backdrop from "../../backdrop";
import Checkbox from "../../checkbox";
import Dropdown from "../../dropdown";
import FormField from "../../field";
import HoursMinutes from "../../HoursMinutes";
import styles from "../../styles.module.scss";

const AddTaskBank = (props: { complete: (response: ITaskBank) => void }) => {
    const [name, setName] = useState("");
    const [teamLeader, setTeamLeader] = useState(false);
    const [group, setGroup] = useState(0);
    const [type, setType] = useState(0);
    const [duration, setDurations] = useState(0);
    const [groups, setGroups] = useState<BasicInfo[]>([]);

    useEffect(() => {
        API.RESOURCES.GROUPS.GET_ALL_MINI().then((res) => {
            if (res && !res.error) setGroups(res.data);
        });
    }, [setGroups]);

    const handleSumbit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        API.SCHEMAS.TASK_BANK.ADD({
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
                        <HoursMinutes
                            value={duration}
                            setValue={setDurations}
                        />
                        <Dropdown
                            handleChange={setGroup}
                            id={group}
                            options={groups}
                            label="Group"
                        />
                        <Dropdown
                            handleChange={setType}
                            id={type}
                            options={[
                                { id: 1, name: "Creation" },
                                { id: 3, name: "Review" },
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

export default AddTaskBank;
