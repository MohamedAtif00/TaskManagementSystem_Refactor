import Link from "next/link";
import { UrlObject } from "url";
import EditPen from "../../assets/Icons/EditPen";
import EyeIcon from "../../assets/Icons/Eye";
import CopyIcon from "../../assets/Icons/Copy";
import TrashIcon from "../../assets/Icons/Trash";
import StopIcon from "../../assets/Icons/Stop";
import PauseIcon from "../../assets/Icons/Pause";
import PlayIcon from "../../assets/Icons/Play";
import PieChartIcon from "../../assets/Icons/PieChart";

type TableActionType =
    | "edit"
    | "archive"
    | "eye"
    | "duplicate"
    | "pause"
    | "stop"
    | "chart"
    | "resume";

interface Props {
    url: UrlObject;
    text: string;
    type: TableActionType;
}

const TableAction = ({ type, url, text }: Props) => {
    return (
        <Link href={url}>
            <button
                className={`${
                    type === "edit"
                        ? "hover:border-blue-600"
                        : type === "archive"
                        ? "hover:border-red-600"
                        : type === "duplicate"
                        ? "hover:border-cyan-600"
                        : type === "stop"
                        ? "hover:border-orange-500"
                        : type === "pause"
                        ? "hover:border-teal-500"
                        : type === "resume"
                        ? "hover:border-emerald-500"
                        : "hover:border-black"
                } bg-white shadow-lg transition-all ease-in flex gap-2 h-7 border-solid border rounded items-center group px-2 py-1 opacity-50 hover:opacity-100`}
                title={text}
            >
                {type === "edit" ? (
                    <div>
                        <EditPen className="group-hover:fill-blue-600 w-4 h-4 fill-black transition-all ease-in" />
                    </div>
                ) : type === "archive" ? (
                    <div>
                        <TrashIcon className="group-hover:fill-red-600 w-4 h-4 fill-black transition-all ease-in" />
                    </div>
                ) : type === "eye" ? (
                    <div>
                        <EyeIcon className="w-4 h-4 stroke-black transition-all ease-in" />
                    </div>
                ) : type === "duplicate" ? (
                    <div>
                        <CopyIcon className="group-hover:stroke-cyan-600 w-4 h-4 stroke-black transition-all ease-in" />
                    </div>
                ) : type === "stop" ? (
                    <div>
                        <StopIcon className="group-hover:fill-orange-600 w-4 h-4 fill-black transition-all ease-in" />
                    </div>
                ) : type === "pause" ? (
                    <div>
                        <PauseIcon className="group-hover:fill-teal-600 w-4 h-4 fill-black transition-all ease-in" />
                    </div>
                ) : type === "resume" ? (
                    <div>
                        <PlayIcon className="group-hover:fill-emerald-600 w-4 h-4 fill-black transition-all ease-in" />
                    </div>
                     ) : type === "chart" ? (
                    <div>
                        <div className="w-4 h-4">
                            <PieChartIcon />
                        </div>
                    </div>
                ) : (
                    <></>
                )}
            </button>
        </Link>
    );
};

export default TableAction;
