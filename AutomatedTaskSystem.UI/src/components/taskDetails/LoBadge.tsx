import BadgeTooltip from "./Tooltip";

interface Props {
    text: string;
    label: string;
}

const LoBadge: React.FC<Props> = ({ text, label }) => (
    <div className="relative group flex justify-start items-center flex-col">
        <div className="rounded-full px-3 border-2 border-solid border-slate-600">
            {text}
        </div>
        <BadgeTooltip label={label} />
    </div>
);

export default LoBadge;
