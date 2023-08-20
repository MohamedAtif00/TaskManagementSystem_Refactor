import BadgeTooltip from "./Tooltip";

interface Props {
    status: "Backlog" | "To Do" | "Doing" | "Done" | "Rollback";
}

const StatusBadge: React.FC<Props> = ({ status }) => {
    let colors = "";
    switch (status) {
        case "Backlog":
            colors = "text-black border-black bg-white";
            break;
        case "To Do":
            colors = "text-white border-blue-300 bg-blue-500";
            break;
        case "Doing":
            colors = "text-white border-amber-300 bg-amber-500";
            break;
        case "Done":
            colors = "text-white border-emerald-300 bg-emerald-400";
            break;
        case "Rollback":
            colors = "text-white border-red-300 bg-red-500";
            break;
    }

    return (
        <div className="group relative flex flex-col justify-start items-center">
            <div
                className={[
                    "px-4 py-1 border-2 border-solid rounded-full shadow-md select-none",
                    colors,
                ].join(" ")}
            >
                {status}
            </div>
            <BadgeTooltip label="Status" />
        </div>
    );
};

export default StatusBadge;
