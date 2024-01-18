import StepListItem from "./stepListItem";

interface Props {
    nodeId: number;
    name: string;
    steps: StepAhead[];
	select: (value: {nodeId: number; stepId: number}) => void
	selectedStep?: number;
}

const StepCard: React.FC<Props> = (props) => {
	const {nodeId} = props;
    return (
        <div className="border border-solid border-slate-500 px-4 py-4 rounded grow shrink-0">
            <h3 className="text-slate-700 font-bold text-lg">{props.name}</h3>
            <div className="mt-2 flex flex-col gap-2">
                {props.steps.map((s) => {
                    return (
                        <StepListItem
                            onClick={() => !s.isComplete && props.select({nodeId, stepId: s.id})}
                            nodeId={props.nodeId}
                            key={s.id}
                            isSelected={props.selectedStep === s.id}
                            id={s.id}
                            name={s.name}
                            isComplete={s.isComplete}
                            group={s.group}
                        />
                    );
                })}
            </div>
        </div>
    );
};

export default StepCard;
