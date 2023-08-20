import styles from "../styles.module.scss";
import Backdrop from "../backdrop";
import React, { useEffect, useState } from "react";
import { useRouter } from "next/router";
import API from "../../../lib/API";
import Dropdown from "../dropdown";
import { ITask } from "../../taskDetails";

const RollbackForm = (props: { update: (params: ITask) => void }) => {
    const [stepId, setStepId] = useState(0);
    const [rollbackPoints, setRollbackPoints] = useState<
        { id: number; name: string }[]
    >([]);
    const [active, setActive] = useState(false);
    const [submittable, setSubmittable] = useState(false);
    const router = useRouter();

    useEffect(() => {
        const _active =
            router.query.form === "rollback" &&
            router.query.taskId !== undefined;
        setActive(_active);
        if (!_active) {
            setStepId(0);
        }
    }, [router.query, setStepId, setActive]);

    useEffect(() => {
        if (active && router.query.taskId) {
            API.TASKS.PREVIOUS_TASKS(router.query.taskId.toString()).then(
                (res) => {
                    if (res) {
                        setRollbackPoints(res);
                        if (res.length > 0) setStepId(res[0].id);
                    }
                }
            );
        }
    }, [active, router.query.taskId]);

    useEffect(() => {
        setSubmittable(!!stepId);
    }, [stepId, setSubmittable]);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (submittable) {
            const id = router.query.taskId;
            id &&
                API.TASKS.ROLLBACK({ currentTask: id, stepId }).then((res) => {
                    if (res && !res.error) props.update(res.data);
                });
        }
    };

    if (active)
        return (
            <Backdrop
                mainRoute={`/tasks/${router.query.projectId}?taskId=${router.query.taskId}`}
            >
                <div className={[styles.form, styles.center].join(" ")}>
                    <form onSubmit={handleSubmit}>
                        <h2>Rollback Task</h2>
                        <div className={styles.inputs}>
                            <Dropdown
                                label="Rollback Task"
                                id={stepId}
                                options={rollbackPoints}
                                handleChange={setStepId}
                            />
                        </div>
                        <div>
                            <input
                                type="submit"
                                value="Rollback"
                                className={[
                                    styles.submit,
                                    stepId ? "" : styles.inactive,
                                ].join(" ")}
                            />
                        </div>
                    </form>
                </div>
            </Backdrop>
        );

    return <></>;
};

export default RollbackForm;
