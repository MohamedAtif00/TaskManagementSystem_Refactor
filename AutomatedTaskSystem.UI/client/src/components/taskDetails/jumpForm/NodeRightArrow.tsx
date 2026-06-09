import { ChevronRightIcon } from "@heroicons/react/24/outline";

interface Props {
    type?: "current" | "previous" | "parallel";
	isComplete: boolean;
}

const NodeRightArrow: React.FC<Props> = ({type, isComplete}) => (
    <div className="w-4 h-4 relative top-2">
        <ChevronRightIcon
            className={`${
                type === "previous"
                    ? "stroke-teal-600"
                    : isComplete
                    ? "stroke-emerald-600"
                    : type && (type === "parallel" || type === "current")
                    ? "stroke-blue-600"
                    : ""
            } stroke-2`}
        />
    </div>
);

export default NodeRightArrow;
