interface Props {
    duration: number;
}

const DurationBadge: React.FC<Props> = (props) => {
    const time = new Date(props.duration).getTime();

    const seconds = Math.floor(time / 1000),
        minutes = Math.floor(seconds / 60),
        hours = Math.floor(minutes / 60),
        days = Math.floor(hours / 24);

    return (
        <>
            <div className="pl-[1px] bg-slate-200"></div>
            <div className="text-sm flex flex-col justify-center items-start text-slate-600 gap-1">
                <div>Duration:</div>
                <div className="flex gap-2">
                    {days > 0 ? `${days} Days ` : ""}
                    {hours % 24 > 0 || days > 0 ? `${hours % 24} Hours ` : ""}
                    {minutes % 60 >= 10 ? "" : "0"}
                    {minutes % 60} Minutes
                </div>
            </div>
        </>
    );
};

export default DurationBadge;
