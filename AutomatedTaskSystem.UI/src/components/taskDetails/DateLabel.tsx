import { CalendarIcon, ClockIcon } from "@heroicons/react/24/outline";
import dateHandler from "../../lib/DateHandler";

interface Props {
    date: string;
    label: string;
}

const DateLabel: React.FC<Props> = ({ date, label }) => {
    const data = dateHandler(date);

    return (
        <div className="text-sm flex flex-col justify-center items-start text-slate-600 gap-1">
            <div>{label}:</div>
            <div className="flex gap-2">
                <div className="flex gap-1">
                    <CalendarIcon className="w-5 h-5" />
                    <div>{data.date}</div>
                </div>
                <div className="pl-[1px] bg-slate-200"></div>
                <div className="flex gap-1">
                    <ClockIcon className="w-5 h-5" />
                    <div>{`${data.hours}:${data.minutes} ${data.con}`}</div>
                </div>
            </div>
        </div>
    );
};

export default DateLabel;
