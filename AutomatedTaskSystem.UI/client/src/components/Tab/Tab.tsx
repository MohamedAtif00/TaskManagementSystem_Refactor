import { CSSProperties } from "react";

const Tab = ({
    active,
    label,
    onClick,
    style
}: {
    active?: boolean;
    label: string;
    onClick?: () => void;
    style?:CSSProperties
}) => (
    <div
        onClick={onClick}
        className={ `border border-solid border-b-0 border-slate-400 rounded-t px-4 transition-all flex items-center justify-center ease-in ${
            active ? "py-1 bg-white font-bold" : "cursor-pointer py-0"
        }`}
        style={{...style, marginBottom:-1}}
    >
        {label}
    </div>
);

export default Tab;