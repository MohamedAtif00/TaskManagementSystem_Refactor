import { useEffect, useRef, useState } from "react";
import ArrowIcon from "../../assets/Icons/Arrow";

const minutesList = [
    { value: 0, name: "00" },
    { value: 10, name: "10" },
    { value: 20, name: "20" },
    { value: 30, name: "30" },
    { value: 40, name: "40" },
    { value: 50, name: "50" },
];

const hoursList = [
    { value: 0, name: "0" },
    { value: 1, name: "1" },
    { value: 2, name: "2" },
    { value: 3, name: "3" },
    { value: 4, name: "4" },
    { value: 5, name: "5" },
    { value: 6, name: "6" },
];

const Selector = ({ label, setValue, placeholder, options }: {
    options: { value: number; name: string }[];
    value: number; placeholder: string;
    setValue: (params: { value: number; name: string }) => void;
    label: string;
}) => {
    const [toggle, setToggle] = useState(false);
    const [xy, setXY] = useState<{ x: number; y: number }>();
    const buttonRef = useRef<HTMLButtonElement>(null);

    const handleToggle = () => {
        if (toggle) {
            setXY(undefined);
            setToggle(false);
        } else {
            let x = 0, y = 0;
            if (buttonRef.current !== null) {
                let ele: any = buttonRef.current;
                while (true) {
                    x += ele.offsetLeft;
                    y += buttonRef.current.offsetTop;
                    if (ele.offsetParent !== null) {
                        ele = ele.offsetParent;
                    } else break;
                }
            }
            setXY({ x, y });
            setToggle(true);
        }
    }

    console.log(xy)

    const handleSet = (params: { value: number; name: string }) => {
        setValue(params);
        setToggle(false);
    };

    return (
        <div>
            <div className="flex justify-between items-center">
                <div>{label}:</div>
                <button
                    ref={buttonRef}
                    onClick={handleToggle}
                    type="button"
                    className={`w-16 hover:border-slate-600 hover:text-slate-600 border border-solid rounded text-slate-500 border-slate-400 flex items-center justify-between gap-2 px-2 py-1`}
                >
                    <div>{placeholder}</div>
                    <ArrowIcon color="#DBDFE5" />
                </button>
            </div>
            {toggle ? (
                <div className="flex justify-end relative top-1">
                    <div className={`z-50 bg-white py-1 rounded w-16 fixed border border-solid border-slate-600 text-slate-600`}>
                        {options.map(_ => {
                            return <div
                                key={_.value}
                                className="flex items-center justify-center py-1 hover:bg-slate-100"
                                onClick={() => handleSet(_)}
                            >{_.name}</div>
                        })}
                    </div>
                </div>
            ) : <></>}
        </div>
    );
};

const HoursMinutes = ({ value, setValue }: { value: number, setValue: (value: number) => void }) => {
    const foundHour = hoursList.find(_ => _.value === Math.floor(value / 60));
    const foundMinute = minutesList.find(_ => _.value === (value % 60));

    const [minutes, setMinutes] = useState(foundMinute ? foundMinute : minutesList[0]);
    const [hours, setHours] = useState(foundHour ? foundHour : hoursList[0]);

    useEffect(() => {
        setValue(minutes.value + hours.value * 60);
    }, [minutes.value, hours.value, setValue]);

    return (
        <div>
            <div>Duration</div>
            <div className="flex gap-4 justify-between">
                <Selector label="HH" value={hours.value} setValue={setHours} options={hoursList} placeholder={hours.name} />
                <Selector label="MM" value={minutes.value} setValue={setMinutes} options={minutesList} placeholder={minutes.name} />
            </div>
        </div>
    );
}

export default HoursMinutes;
