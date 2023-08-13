import { CalendarIcon, ClockIcon } from "@heroicons/react/24/outline";

const dateHandler = (params: string) => {
    const date = new Date(params);

    const MonthNames = new Map<number, string>();

    MonthNames.set(0, "Jan");
    MonthNames.set(1, "Feb");
    MonthNames.set(2, "Mar");
    MonthNames.set(3, "Apr");
    MonthNames.set(4, "May");
    MonthNames.set(5, "Jun");
    MonthNames.set(6, "Jul");
    MonthNames.set(7, "Aug");
    MonthNames.set(8, "Sep");
    MonthNames.set(9, "Oct");
    MonthNames.set(10, "Nov");
    MonthNames.set(11, "Dec");

    const dateHours = date.getHours() % 12 === 0 ? 12 : date.getHours() % 12;

    const dateMinutes =
        date.getMinutes() < 10
            ? `0${date.getMinutes()}`
            : `${date.getMinutes()}`;

    const time = {
        date: `${date.getDate()} ${MonthNames.get(
            date.getMonth()
        )} ${date.getFullYear()}`,
        hours: dateHours < 10 ? `0${dateHours}` : `${dateHours}`,
        minutes: dateMinutes,
        con: date.getHours() > 12 ? "PM" : "AM",
    };

    return time;
};

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
