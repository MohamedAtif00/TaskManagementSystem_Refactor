import { CheckIcon } from "@heroicons/react/24/outline";

type Props = {
	nodeId: number;
    isSelected: boolean;
	onClick: () => void;
} & StepAhead;

const StepListItem: React.FC<Props> = (props) => {
    return (
        <div
            className={`px-2 py-2 border border-solid rounded flex justify-between items-center cursor-pointer select-none ${
                props.isComplete ? "text-emerald-600" : props.isSelected ? "text-blue-600" : "text-slate-800 "
            }`}
			onClick={props.onClick}
        >
            <div className="flex items-center justify-center gap-2">
                <div>{props.name}</div>
                <div className="p-[3px] bg-slate-400 rounded-full"></div>
                <div>{props.group.name}</div>
            </div>
            <div>
                {props.isComplete ? (
                    <div className="border-2 border-solid border-green-200 bg-emerald-400 rounded-full">
                        <CheckIcon className="h-6 w-6 p-1 stroke-white stroke-2" />
                    </div>
                ) : props.isSelected ? (
                    <div className="border-2 border-solid border-blue-200 bg-blue-500 rounded-full">
                        <CheckIcon className="h-6 w-6 p-1 stroke-white stroke-2" />
                    </div>
                ) : (
                    <div className="p-3 border-slate-400 border-2 border-solid rounded-full"></div>
                )}
            </div>
        </div>
    );
};

export default StepListItem;
