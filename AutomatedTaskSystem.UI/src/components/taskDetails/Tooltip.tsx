interface Props {
    label: string;
}

const BadgeTooltip: React.FC<Props> = ({ label }) => {
    return (
        <span className="pointer-events-none group-hover:opacity-100 opacity-0 absolute -bottom-8 border-slate-200 bg-slate-50 border border-solid text-black px-2 rounded-md">
            {label}
        </span>
    );
};

export default BadgeTooltip;
