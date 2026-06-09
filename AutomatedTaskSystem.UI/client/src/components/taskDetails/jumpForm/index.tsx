import { motion } from "framer-motion";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import API from "../../../lib/API";
import CrossIcon from "../../../assets/Icons/Cross";
import Link from "next/link";
import useMeasure from "react-use-measure";
import StepCard from "./stepCard";
import NodeCards from "./selectedNode";
import { ITask } from "..";
import useTaskPathHandler from "../useTaskPathHandler.ts";

interface Props {
    taskId: number;
    updateTask: (value: ITask) => void;
    type: TaskType;
}

const JumpForm: React.FC<Props> = ({ taskId, updateTask,type }) => {
    const [points, setPoints] = useState<NodeAhead[]>();
    const router = useRouter();
    const [selectedNodes, setSelectedNode] = useState<NodeAhead[]>([]);
    const [selectedSteps, setSelectedSteps] = useState<
        { nodeId: number; stepId: number }[]
    >([]);  
    const [ref, { height }] = useMeasure();
    const pathHandler = useTaskPathHandler({type:type});

    useEffect(() => {
        if (router.query.form === "jump")
            API.TASKS.GET_JUMP_POINTS(taskId).then((res) => {
                if (res && !res.error) setPoints(res.data);
            });
        else {
            setPoints(undefined);
            setSelectedNode([]);
            setSelectedSteps([]);
        }
    }, [taskId, router.query.form]);

    if (router.query.form !== "jump" || !points) return <></>;

    const handleNodeRemove = (value: number) =>
        setSelectedSteps((ps) => {
            const newState: { nodeId: number; stepId: number }[] = [];
            for (const element of ps)
                if (element.nodeId !== value) newState.push(element);
            return newState;
        });

    const handleStepSelect = (value: { nodeId: number; stepId: number }) => {
        handleNodeRemove(value.nodeId);
        setSelectedSteps((ps) => [...ps, value]);
    };

    const handleNodeSelect = (value: NodeAhead[]) => {
        setSelectedNode(value);
        setSelectedSteps((ps) => {
            const newState: { nodeId: number; stepId: number }[] = [];
            for (const step of ps) {
                if (value.some((n) => n.id === step.nodeId))
                    newState.push(step);
            }
            return newState;
        });
    };

    const handleSubmit = () => {
        selectedSteps.length > 0 &&
            selectedSteps.length === selectedNodes.length &&
            API.TASKS.JUMP_TASK(taskId, selectedSteps).then((res) => {
                if (res && !res.error) {
                    updateTask(res.data);
                }
            });
    };

    return (
        <motion.div
            initial={{
                opacity: 0,
            }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed z-50 top-0 bottom-0 left-0 right-0 backdrop-blur-sm bg-black/5 flex justify-center items-center"
        >
            <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="bg-white shadow border-2 border-neutral-700 border-solid rounded flex flex-col gap-1 max-w-[90%]"
            >
                <div className="pt-4 flex items-center justify-between px-6 shrink-0">
                    <div className="font-bold text-lg">Jump Forward</div>
                    <Link
                        href={{
                            pathname: pathHandler(),
                            query: {
                                taskId,
                            },
                        }}
                    >
                        <button className="p-1 box-content">
                            <CrossIcon className="stroke-black" />
                        </button>
                    </Link>
                </div>
                <div className="px-6 py-4">
                    <div className="overflow-x-auto pb-4">
                        <NodeCards
                            updateSelected={handleNodeSelect}
                            nodes={points}
                        />
                    </div>
                    <motion.div
                        animate={{ height: height === 0 ? 0 : height + 44 }}
                        className="overflow-hidden flex flex-col gap-2"
                    >
                        <div ref={ref} className="flex gap-2 flex-wrap">
                            {selectedNodes.map((s) => {
                                return (
                                    <StepCard
                                        select={handleStepSelect}
                                        selectedStep={
                                            selectedSteps.find(
                                                (_) => _.nodeId === s.id
                                            )?.stepId
                                        }
                                        nodeId={s.id}
                                        name={s.name}
                                        key={s.id}
                                        steps={s.steps}
                                    />
                                );
                            })}
                        </div>
                        {selectedNodes.length > 0 && (
                            <div className="flex justify-end">
                                <button
                                    onClick={handleSubmit}
                                    className="px-3 py-1 bg-blue-500 text-white rounded-md border-2 border-solid border-blue-300"
                                >
                                    Submit
                                </button>
                            </div>
                        )}
                    </motion.div>
                </div>
            </motion.div>
        </motion.div>
    );
};

export default JumpForm;
