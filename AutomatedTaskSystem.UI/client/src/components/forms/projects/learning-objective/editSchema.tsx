import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import useMeasure from "react-use-measure";
import NodeCards from "../../../taskDetails/jumpForm/selectedNode";
import StepCard from "../../../taskDetails/jumpForm/stepCard";
import API from "../../../../lib/API";
import Loader from "../../../loader";

interface State {
    schema: BasicInfo;
    template: string;
    environment: string;
    tag: string;
    name: string;
}

interface Props {
    schemaId: number;
    back: () => void;
    state: State;
	update: (param: LearningObjective) => void
	loId: number;
}

const EditLoSchemaForm: React.FC<Props> = ({ schemaId, back, state, update, loId }) => {
    const [points, setPoints] = useState<NodePoint[]>();
    const [selectedNodes, setSelectedNode] = useState<NodePoint[]>([]);
    const [selectedSteps, setSelectedSteps] = useState<
        { nodeId: number; stepId: number }[]
    >([]);
    const [ref, { height }] = useMeasure();

    useEffect(() => {
        API.SCHEMAS.GET_POINT(schemaId).then((res) => {
            res && !res.error && setPoints(res.data);
        });
    }, [schemaId]);

    if (!points)
        return (
            <motion.div
                initial={{
                    opacity: 0,
                }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="fixed z-50 top-0 bottom-0 left-0 right-0 backdrop-blur-sm bg-black/5 flex justify-center items-center"
            >
                <Loader />
            </motion.div>
        );

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
            API.PROJECTS.UNITS.LESSONS.LEARNING_OBJECTIVES.EDIT(loId, {
                name: state.name,
                steps: selectedSteps.map((s) => s.stepId),
                schemaId: state.schema.id,
                nods:selectedNodes.map(s => s.id),
                tag: state.tag,
                template: state.template,
                environment: state.environment,
            }).then(res => {
				if (res) {
					update(res);
				}
			});
    };

    return (
        <motion.div
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
                    <div className="font-bold text-lg">Select Schema steps</div>
                    <button
                        className="px-3 py-1 bg-black text-white border-2 border-solid border-white/30"
                        onClick={back}
                    >
                        Back
                    </button>
                </div>
                <div className="px-6 py-4">
                    <div className="overflow-x-auto pb-4">
                        <NodeCards
                            updateSelected={handleNodeSelect}
                            nodes={points.map((p) => {
                                return {
                                    ...p,
                                    isComplete: false,
                                    steps: p.steps.map((s) => ({
                                        ...s,
                                        isComplete: false,
                                    })),
                                };
                            })}
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
                                        steps={s.steps.map((p) => {
                                            return {
                                                ...p,
                                                isComplete: false,
                                            };
                                        })}
                                    />
                                );
                            })}
                        </div>
                        {selectedNodes.length > 0 && (
                            <div className="flex justify-end gap-4">
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

export default EditLoSchemaForm;
