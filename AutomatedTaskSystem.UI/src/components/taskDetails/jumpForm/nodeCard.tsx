import { motion } from "framer-motion";

interface Props {
    id: number;
    isComplete: boolean;
    name: string;
    onHover: (value: number) => void;
    type?: "current" | "previous" | "parallel";
    onLeave: () => void;
    onClick: () => void;
}

const NodeCard: React.FC<Props> = ({
    id,
    isComplete,
    name,
    onHover,
    type,
    onLeave,
    onClick,
}) => {
    return (
        <div className="pt-4 relative">
            {type === "parallel" && (
                <motion.div
                    initial={{
                        scale: 0,
                    }}
                    animate={{
                        scale: 1,
                    }}
                    className="absolute top-0 left-0 right-0 text-xs flex items-center justify-center italic text-slate-700 origin-bottom h-4 overflow-hidden"
                >
                    Parallel
                </motion.div>
            )}
            <div
                onClick={onClick}
                className={`${
                    isComplete
                        ? "text-emerald-600 border-emerald-600 cursor-default"
                        : type && type === "previous"
                        ? "text-teal-600 border-teal-600 cursor-pointer"
                        : type === "current" || type === "parallel"
                        ? "text-blue-600 border-blue-600 cursor-pointer"
                        : "cursor-pointer"
                } border border-solid rounded text-center min-w-[5rem] select-none`}
                onMouseEnter={() => onHover(id)}
                onMouseLeave={onLeave}
            >
                <div className="px-2 h-8 flex items-center justify-center whitespace-nowrap">
                    {name}
                </div>
            </div>
        </div>
    );
};

export default NodeCard;
