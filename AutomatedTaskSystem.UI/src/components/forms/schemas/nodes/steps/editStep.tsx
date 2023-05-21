import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../../../../app/hooks";
import styles from "../../../styles.module.scss";
import Backdrop from "../../../backdrop";
import FormField from "../../../field";
import API from "../../../../../lib/API";
import { clear, load } from "../../../../../slices/groupSlice";
import { editStep } from "../../../../../slices/nodesSlice";
import SelectList from "../../../selectList";
import Checkbox from "../../../checkbox";
import { ITaskBank } from "../../../../../pages/schemas/task-bank";
import Dropdown from "../../../dropdown";
import HoursMinutes from "../../../HoursMinutes";

interface Props {
    schemaId: number;
    step: IStep;
}

const EditStep = (props: Props) => {
    const [submittable, setSubmittable] = useState(false);
    const router = useRouter();
    const dispatch = useAppDispatch();
    const [taskBank, setTaskBank] = useState<ITaskBank[]>([]);
    const [taskBankItem, setTaskBankItem] = useState<number>(0);
    const [duration, setDuration] = useState<number>(0);

    useEffect(() => {
        API.SCHEMAS.TASK_BANK.GET_ALL().then(res => {
            if (res)
                setTaskBank(res);
        })
    }, []);

    useEffect(() => {
        if (taskBankItem === 0) {
            setSubmittable(false)
        } else {
            setSubmittable(true);
        }
    }, [taskBankItem]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!submittable) {
            return;
        }
        API.SCHEMAS.NODES.STEPS.EDIT({
            stepId: router.query.stepId!.toString(),
            taskBankItem,
            duration
        }).then((res) => {
            if (res) {
                dispatch(editStep({ step: res }));
                router.back();
            }
        });
    };

    return (
        <Backdrop mainRoute={`/schemas/${props.schemaId}`}>
            <div className={[styles.form, styles.center].join(" ")}>
                <form onSubmit={handleSubmit}>
                    <div className={styles.inputs}>
                        <Dropdown id={taskBankItem} label="Task Bank Item" options={taskBank.map(tb => ({
                            id: tb.id, name: [
                                tb.group.name, tb.tl ? "Team Leader" : "", tb.name
                            ].join(" ")
                        }))} handleChange={setTaskBankItem} />
                        <HoursMinutes value={duration} setValue={setDuration} />
                    </div>
                    <div>
                        <input
                            type="submit"
                            value="Save"
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
};

export default EditStep;
