interface Props {
    label: string;
    value: string | number;
}

const DashboardCard: React.FC<Props> = ({ label, value }) => (
    <div className="col-span-3 p-4 bg-white rounded">
        <div className="text-sm font-bold">{label}</div>
        <div className="text-2xl">{value}</div>
    </div>
);

export default DashboardCard;
