import { STATUS_OPTIONS } from "./constants";

interface Props {
    filters: TaskLoggerFilters;
    lookups: TaskLoggerLookups;
    rowCount: number;
    onFilterChange: (key: keyof TaskLoggerFilters, value: string) => void;
    onReset: () => void;
}

const selectClass = "w-full border rounded-lg p-2 mt-1 text-sm bg-white";

const TaskLoggerFilters = ({ filters, lookups, rowCount, onFilterChange, onReset }: Props) => {
    return (
        <div className="bg-white rounded-xl shadow-md p-4 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-3">
                <div>
                    <label className="block text-sm font-semibold">Search (LO / Task)</label>
                    <input
                        type="text"
                        className={selectClass}
                        defaultValue={filters.search ?? ""}
                        key={`search-${filters.from}-${filters.to}-${filters.member}-${filters.subject}-${filters.status}`}
                        onKeyDown={(e) => {
                            if (e.key === "Enter") {
                                onFilterChange("search", (e.target as HTMLInputElement).value);
                            }
                        }}
                        onBlur={(e) => onFilterChange("search", e.target.value)}
                        placeholder="LO code or task name (Enter)"
                    />
                </div>
                <div>
                    <label className="block text-sm font-semibold">Member</label>
                    <select
                        className={selectClass}
                        value={filters.member ?? ""}
                        onChange={(e) => onFilterChange("member", e.target.value)}
                    >
                        <option value="">All Members</option>
                        {(lookups.members ?? []).map((v) => (
                            <option key={v} value={v}>{v}</option>
                        ))}
                    </select>
                </div>
                <div>
                    <label className="block text-sm font-semibold">Status</label>
                    <select
                        className={selectClass}
                        value={filters.status ?? ""}
                        onChange={(e) => onFilterChange("status", e.target.value)}
                    >
                        <option value="">All</option>
                        {(lookups.statuses ?? STATUS_OPTIONS).map((v) => (
                            <option key={v} value={v}>{v}</option>
                        ))}
                    </select>
                </div>
                <div>
                    <label className="block text-sm font-semibold">Subject</label>
                    <select
                        className={selectClass}
                        value={filters.subject ?? ""}
                        onChange={(e) => onFilterChange("subject", e.target.value)}
                    >
                        <option value="">All Subjects</option>
                        {(lookups.subjects ?? []).map((v) => (
                            <option key={v} value={v}>{v}</option>
                        ))}
                    </select>
                </div>
                <div>
                    <label className="block text-sm font-semibold">Date From</label>
                    <input
                        type="date"
                        className={selectClass}
                        value={filters.from ?? ""}
                        onChange={(e) => onFilterChange("from", e.target.value)}
                    />
                </div>
                <div>
                    <label className="block text-sm font-semibold">Date To</label>
                    <input
                        type="date"
                        className={selectClass}
                        value={filters.to ?? ""}
                        onChange={(e) => onFilterChange("to", e.target.value)}
                    />
                </div>
            </div>
            <div className="mt-3 flex flex-wrap items-center gap-3">
                <span className="text-sm text-gray-600">{rowCount} result(s) found</span>
                <button
                    type="button"
                    onClick={onReset}
                    className="ml-auto bg-gray-200 hover:bg-gray-300 px-4 py-2 rounded-lg text-sm"
                >
                    Reset Filters
                </button>
            </div>
        </div>
    );
};

export default TaskLoggerFilters;
