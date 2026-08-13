type CurriculumPathFiltersProps = {
    filterLabels: string[];
    filterOptions: string[][];
    filters: Record<number, string>;
    onFilterChange: (index: number, value: string) => void;
    className?: string;
};

const CurriculumPathFilters = ({
    filterLabels,
    filterOptions,
    filters,
    onFilterChange,
    className = "",
}: CurriculumPathFiltersProps) => (
    <div className={`grid w-full grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4 xl:grid-cols-5 ${className}`}>
        {filterOptions.map((options, index) => (
            <label key={index} className="block text-sm font-medium text-slate-700">
                {filterLabels[index] ?? `Level ${index + 1}`}
                <select
                    className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                    value={filters[index] ?? ""}
                    onChange={(e) => onFilterChange(index, e.target.value)}
                >
                    <option value="">
                        All {filterLabels[index] ?? `Level ${index + 1}`}
                    </option>
                    {options.map((option) => (
                        <option key={option} value={option}>
                            {option}
                        </option>
                    ))}
                </select>
            </label>
        ))}
    </div>
);

export default CurriculumPathFilters;
