import {
    CalendarIcon,
    ClockIcon,
    DocumentPlusIcon,
} from "@heroicons/react/24/outline";
import dateHandler from "../../../lib/DateHandler";

interface Props {
    date: string;
    name: string;
}

const CreatedActivity: React.FC<Props> = (props) => {
    const date = dateHandler(props.date);

    return (
        <div className="w-full">
            <div className="flex gap-4">
                <DocumentPlusIcon className="p-1 h-6 w-6 rounded-full border border-solid border-slate-400 box-content" />
                <div className="flex flex-col justify-center">
                    <div>
                        <span className="font-bold">{props.name}</span> was
                        created
                    </div>
                </div>
            </div>
                <div className="flex gap-4 justify-center text-slate-600 text-sm">
                    <div className="flex gap-1 items-center w-32 justify-end">
                        <CalendarIcon className="w-5 h-5" />
                        <div>{date.date}</div>
                    </div>
                    <div className="pl-1 rounded-t-full rounded-md bg-slate-300"></div>
                    <div className="flex gap-1 items-center w-32 justify-start">
                        <ClockIcon className="w-5 h-5" />
                        <div>{`${date.hours}:${date.minutes} ${date.con}`}</div>
                    </div>
                </div>
        </div>
    );
};

export default CreatedActivity;
