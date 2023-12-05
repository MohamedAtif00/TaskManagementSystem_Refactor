import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppDispatch } from "../../../../../app/hooks";
import styles from "../../../styles.module.scss";
import Backdrop from "../../../backdrop";
import API from "../../../../../lib/API";
import { addStep } from "../../../../../slices/nodesSlice";
import Dropdown from "../../../dropdown";
import { ITaskBank } from "../../../../../pages/schemas/task-bank";
import HoursMinutes from "../../../HoursMinutes";

interface Props {
    schemaId: number;
}

const AddStep = (props: Props) => {
    const [submittable, setSubmittable] = useState(false);
    const router = useRouter();
    const [active, setActive] = useState(false);
    const dispatch = useAppDispatch();
    const [taskBank, setTaskBank] = useState<ITaskBank[]>([]);
    const [taskBankItem, setTaskBankItem] = useState<number>(0);
    const [duration, setDuration] = useState<number>(0);

    useEffect(() => {
        API.SCHEMAS.TASK_BANK.GET_ALL().then((res) => {
            if (res) setTaskBank(res);
        });
    }, []);

    useEffect(() => {
        if (taskBankItem === 0) {
            setSubmittable(false);
        } else setSubmittable(true);
    }, [taskBankItem]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (submittable)
            API.SCHEMAS.NODES.STEPS.ADD({
                nodeId: router.query.nodeId!.toString(),
                taskBankItem,
                duration,
            }).then((res) => {
                if (res) {
                    dispatch(
                        addStep({
                            id: parseInt(router.query.nodeId!.toString()),
                            step: res,
                        })
                    );
                    router.push(`/schemas/${props.schemaId}`);
                }
            });
    };

    useEffect(() => {
        const _active =
            router.query.form === "step" && router.query.nodeId !== undefined;
        setActive(_active);
    }, [router, dispatch]);

    if (active)
        return (
            <Backdrop mainRoute={`/schemas/${props.schemaId}`}>
                <div className={[styles.form, styles.center].join(" ")}>
                    <form onSubmit={handleSubmit}>
                        <div className={styles.inputs}>
                            <Dropdown
                                id={taskBankItem}
                                label="Task Bank Item"
                                options={taskBank.map((tb) => ({
                                    id: tb.id,
                                    name: [
                                        tb.group.name,
                                        tb.tl ? "Team Leader" : "",
                                        tb.name,
                                    ].join(" "),
                                }))}
                                handleChange={setTaskBankItem}
                            />
                            <HoursMinutes
                                value={duration}
                                setValue={setDuration}
                            />
                        </div>
                        <div>
                            <input
                                type="submit"
                                value="Add"
                                className={[
                                    styles.submit,
                                    submittable ? "" : styles.inactive,
                                ].join(" ")}
                            />
                        </div>
                    </form>
                </div>
            </Backdrop>
        );

    return <></>;
};

export default AddStep;
