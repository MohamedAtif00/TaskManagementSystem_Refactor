import BadgeTooltip from "./Tooltip";

interface Props {
    status: TaskStatus;
}

const StatusBadge: React.FC<Props> = ({ status }) => {
    let colors = "";
    switch (status) {
        case 0:
            colors = "text-black border-black bg-white";
            break;
        case 1:
            colors = "text-white border-blue-300 bg-blue-500";
            break;
        case 2:
            colors = "text-white border-amber-300 bg-amber-500";
            break;
        case 3:
            colors = "text-white border-emerald-300 bg-emerald-400";
            break;
        case 4:
            colors = "text-white border-red-300 bg-red-500";
            break;
    }

	const statuses = new Map<number, string>();
	statuses.set(0, "Backlog");
	statuses.set(1, "To Do");
	statuses.set(2, "Doing");
	statuses.set(3, "Done");
	statuses.set(4, "Roll Back");

    return (
        <div className="group relative flex flex-col justify-start items-center">
            <div
                className={[
                    "px-4 py-1 border-2 border-solid rounded-full shadow-md select-none",
                    colors,
                ].join(" ")}
            >
                {statuses.get(status)}
            </div>
            <BadgeTooltip label="Status" />
        </div>
    );
};

export default StatusBadge;
