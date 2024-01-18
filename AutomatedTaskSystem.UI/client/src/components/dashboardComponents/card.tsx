interface Props {
    label: string;
    value: string | number;
    size?: 3 | 4;
}

const DashboardCard: React.FC<Props> = ({ label, value, size = 3 }) => (
    <div
        className={`${
            size === 3 ? "col-span-3" : "col-span-4"
        } p-4 bg-white rounded`}
    >
        <div className="text-sm font-bold">{label}</div>
        <div className="text-2xl">{value}</div>
    </div>
);

export default DashboardCard;
