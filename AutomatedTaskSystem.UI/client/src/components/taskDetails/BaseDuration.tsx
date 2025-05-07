interface Props {
    duration: number; // in minutes
}

const BaseDuration: React.FC<Props> = ({ duration }) => {
    const days = Math.floor(duration / 1440); // 1440 minutes in a day
    const hours = Math.floor((duration % 1440) / 60);
    const minutes = duration % 60;

    return (
        <>
            <div className="pl-[1px] bg-slate-200"></div>
            <div className="text-sm flex flex-col justify-center items-start text-slate-600 gap-1">
                <div>Base Duration:</div>
                <div className="flex gap-2">
                    {days > 0 && <span>{days} Day{days > 1 ? "s" : ""}</span>}
                    {hours > 0 && <span>{hours} Hour{hours > 1 ? "s" : ""}</span>}
                    {minutes > 0 && <span>{minutes} Minute{minutes > 1 ? "s" : ""}</span>}
                    {duration === 0 && <span>0 Minutes</span>}
                </div>
            </div>
        </>
    );
};

export default BaseDuration;
