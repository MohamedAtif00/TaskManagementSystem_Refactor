import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { useAppDispatch } from "../../../../../app/hooks";
import API, { BasicInfo } from "../../../../../lib/API";
import { editStep } from "../../../../../slices/nodesSlice";
import { ITaskBank } from "../../../../../pages/schemas/task-bank";
import HoursMinutes from "../../../HoursMinutes";
import CustomizedCombobox from "../../../../formComponents/Combobox";
import CrossIcon from "../../../../../assets/Icons/Cross";
import Link from "next/link";

interface Props {
    schemaId: number;
    step: IStep;
}

const EditStep = (props: Props) => {
    const [submittable, setSubmittable] = useState(false);
    const router = useRouter();
    const dispatch = useAppDispatch();
    const [taskBank, setTaskBank] = useState<ITaskBank[]>([]);
    const [taskBankItem, setTaskBankItem] = useState<ITaskBank>();
    const [duration, setDuration] = useState<number>(0);
    const [rollbacks, setRollbacks] = useState<BasicInfo[]>();
    const [rollbackOptions, setRollbackOptions] = useState<BasicInfo[]>();
    const [selectedRollbackOption, setSelectedRollbackOption] =
        useState<BasicInfo>();

    useEffect(() => {
        API.SCHEMAS.TASK_BANK.GET_ALL().then((res) => {
            if (res) {
                setTaskBank(res);
                const foundItem = res.find(
                    (tb) => tb.id === props.step.taskBankItemId
                );
                if (foundItem) setTaskBankItem(foundItem);
                else setTaskBankItem(undefined);
            }
        });
    }, [props.step.taskBankItemId]);

    useEffect(() => {
        if (taskBankItem === undefined) setSubmittable(false);
        else setSubmittable(true);
    }, [taskBankItem]);

    useEffect(() => {
        if (taskBankItem?.type.id === 3)
            API.SCHEMAS.NODES.STEPS.GET_ROLLBACK_POINTS(props.step.id).then(
                (res) => {
                    if (res && !res.error) setRollbacks(res.data);
                }
            );
        API.SCHEMAS.NODES.STEPS.AVAILABLE_ROLLBACK_OPTIONS(props.step.id).then(
            (res) => {
                if (res && !res.error) setRollbackOptions(res.data);
            }
        );
    }, [taskBankItem, props.step.id]);

    const updateRollbackPoints = () => {
        API.SCHEMAS.NODES.STEPS.GET_ROLLBACK_POINTS(props.step.id).then(
            (res) => {
                if (res && !res.error) setRollbacks(res.data);
            }
        );
        API.SCHEMAS.NODES.STEPS.AVAILABLE_ROLLBACK_OPTIONS(props.step.id).then(
            (res) => {
                if (res && !res.error) setRollbackOptions(res.data);
            }
        );
    };

    const addRollbackPoint = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        selectedRollbackOption &&
            API.SCHEMAS.NODES.STEPS.ADD_ROLLBACK_POINT(
                props.step.id,
                selectedRollbackOption.id
            ).then((res) => {
                if (res && !res.error) setSelectedRollbackOption(undefined);
                updateRollbackPoints();
            });
    };

    const removeRollbackPoint = (id: number) => {
        API.SCHEMAS.NODES.STEPS.REMOVE_ROLLBACK_POINT(props.step.id, id).then(
            (res) => {
                if (res && !res.error) setSelectedRollbackOption(undefined);
                updateRollbackPoints();
            }
        );
    };

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!submittable || taskBankItem === undefined) return;
        API.SCHEMAS.NODES.STEPS.EDIT({
            stepId: router.query.stepId!.toString(),
            taskBankItem: taskBankItem.id,
            duration,
        }).then((res) => {
            if (res) {
                dispatch(editStep({ step: res }));
                router.back();
            }
        });
    };

    return (
        <div className="fixed top-0 left-0 bottom-0 right-0 flex items-center justify-center bg-black/30 z-40">
            <div className="bg-white shadow-lg p-6 rounded-md">
                <div className="flex justify-end">
                    <Link href={`/schemas/${props.schemaId}`}>
                        <button>
                            <CrossIcon className="stroke-black" />
                        </button>
                    </Link>
                </div>
                <div
                    className={`grid ${
                        taskBankItem?.type.id === 3
                            ? "grid-cols-2"
                            : "grid-cols-1"
                    } grid-rows-1 gap-4`}
                >
                    <form
                        onSubmit={handleSubmit}
                        className="flex flex-col w-64 box-content gap-4"
                    >
                        <div>
                            <div className="text-sm">Schema:</div>
                            <CustomizedCombobox
                                value={taskBankItem}
                                onChange={(e) => setTaskBankItem(e)}
                                options={taskBank}
                            />
                        </div>
                        <HoursMinutes value={duration} setValue={setDuration} />
                        <div className="flex mt-4">
                            <input
                                type="submit"
                                value="Add"
                                className={`${
                                    submittable
                                        ? "bg-blue-500 cursor-pointer"
                                        : "bg-neutral-500"
                                } text-white grow rounded-md py-1`}
                            />
                        </div>
                    </form>
                    {taskBankItem?.type.id === 3 && (
                        <div className="flex gap-4 flex-col">
                            <div>
                                <div className="text-sm">Rollback Points:</div>
                                <form
                                    className="flex gap-1 items-center"
                                    onSubmit={addRollbackPoint}
                                >
                                    <div className="grow">
                                        <CustomizedCombobox
                                            value={selectedRollbackOption}
                                            onChange={(e) =>
                                                setSelectedRollbackOption(e)
                                            }
                                            options={
                                                rollbackOptions
                                                    ? rollbackOptions
                                                    : []
                                            }
                                        />
                                    </div>
                                    <button
                                        type="submit"
                                        className="px-2 py-1 bg-blue-500 text-white rounded-md"
                                    >
                                        Add
                                    </button>
                                </form>
                            </div>
                            <div className="flex flex-col gap-1 grow overflow-y-auto">
                                {rollbacks &&
                                    rollbacks.map((i) => {
                                        return (
                                            <div
                                                key={i.id}
                                                className="border border-solid border-slate-300 hover:bg-slate-50 flex justify-between px-2 py-1 rounded-md items-center"
                                            >
                                                <div>{i.name}</div>
                                                <button
                                                    onClick={() =>
                                                        removeRollbackPoint(
                                                            i.id
                                                        )
                                                    }
                                                >
                                                    <CrossIcon className="w-4 h-4 stroke-red-500" />
                                                </button>
                                            </div>
                                        );
                                    })}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EditStep;
