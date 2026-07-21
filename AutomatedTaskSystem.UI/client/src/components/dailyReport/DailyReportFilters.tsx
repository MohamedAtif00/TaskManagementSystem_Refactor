import { useEffect, useRef, useState } from "react";
import { PRIORITY_OPTIONS, STATUS_OPTIONS } from "./constants";

interface Props {
    filters: DailyReportFilters;
    lookups: DailyReportLookups | null;
    rowCount: number;
    onFilterChange: (key: keyof DailyReportFilters, value: string) => void;
    onProblemTypesChange: (values: string[]) => void;
    onReset: () => void;
}

const selectClass =
    "px-3 py-2 rounded-full border border-slate-300 bg-white text-sm min-w-[120px]";

const DailyReportFilters = ({
    filters,
    lookups,
    rowCount,
    onFilterChange,
    onProblemTypesChange,
    onReset,
}: Props) => {
    const selectedProblemTypes = filters.problemTypes ?? [];
    const [problemTypesOpen, setProblemTypesOpen] = useState(false);
    const problemTypesRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const closeOnOutsideClick = (event: MouseEvent) => {
            if (!problemTypesRef.current?.contains(event.target as Node)) {
                setProblemTypesOpen(false);
            }
        };

        document.addEventListener("mousedown", closeOnOutsideClick);
        return () => document.removeEventListener("mousedown", closeOnOutsideClick);
    }, []);

    const toggleProblemType = (value: string) => {
        if (selectedProblemTypes.includes(value)) {
            onProblemTypesChange(selectedProblemTypes.filter((v) => v !== value));
        } else {
            onProblemTypesChange([...selectedProblemTypes, value]);
        }
    };
    return (
        <div className="bg-white rounded-3xl p-4 mb-6 flex flex-wrap gap-3 items-end shadow-sm">
            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Team</label>
                <select
                    className={selectClass}
                    value={filters.team}
                    onChange={(e) => onFilterChange("team", e.target.value)}
                >
                    <option value="">All</option>
                    {(lookups?.teams ?? []).map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Semester</label>
                <select
                    className={selectClass}
                    value={filters.semester}
                    onChange={(e) => onFilterChange("semester", e.target.value)}
                >
                    <option value="">All</option>
                    {(lookups?.semesters ?? ["Term 1", "Term 2"]).map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Subjects</label>
                <select
                    className={selectClass}
                    value={filters.subject}
                    onChange={(e) => onFilterChange("subject", e.target.value)}
                >
                    <option value="">All</option>
                    {(lookups?.subjects ?? []).map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Grade</label>
                <select
                    className={selectClass}
                    value={filters.grade}
                    onChange={(e) => onFilterChange("grade", e.target.value)}
                >
                    <option value="">All</option>
                    {(lookups?.grades ?? []).map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex w-[180px] flex-col gap-1">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Task Name</label>
                <select
                    className={`${selectClass} w-full min-w-0`}
                    value={filters.taskName}
                    onChange={(e) => onFilterChange("taskName", e.target.value)}
                >
                    <option value="">All</option>
                    {(lookups?.taskNames ?? []).map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Status</label>
                <select
                    className={selectClass}
                    value={filters.status}
                    onChange={(e) => onFilterChange("status", e.target.value)}
                >
                    <option value="">All</option>
                    {STATUS_OPTIONS.map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div ref={problemTypesRef} className="relative flex flex-col gap-1 min-w-[160px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">
                    Problem Type
                </label>
                <button
                    type="button"
                    className={`${selectClass} flex items-center justify-between gap-2 text-left`}
                    aria-expanded={problemTypesOpen}
                    onClick={() => setProblemTypesOpen((open) => !open)}
                >
                    <span className="truncate">
                        {selectedProblemTypes.length === 0
                            ? "All"
                            : `${selectedProblemTypes.length} selected`}
                    </span>
                    <span className="text-[10px] text-slate-500">▼</span>
                </button>
                {problemTypesOpen && (
                    <div className="absolute left-0 top-full z-30 mt-1 min-w-full max-h-56 overflow-y-auto rounded-xl border border-slate-200 bg-white p-2 shadow-lg">
                        <button
                            type="button"
                            className="w-full rounded-lg px-2 py-1.5 text-left text-xs hover:bg-slate-100"
                            onClick={() => onProblemTypesChange([])}
                        >
                            All
                        </button>
                        {(lookups?.problemTypes ?? []).length === 0 ? (
                            <div className="px-2 py-1.5 text-xs text-slate-400">
                                No problem types
                            </div>
                        ) : (
                            (lookups?.problemTypes ?? []).map((v) => (
                                <label
                                    key={v}
                                    className="flex cursor-pointer items-center gap-2 rounded-lg px-2 py-1.5 hover:bg-slate-100"
                                >
                                    <input
                                        type="checkbox"
                                        checked={selectedProblemTypes.includes(v)}
                                        onChange={() => toggleProblemType(v)}
                                    />
                                    <span className="whitespace-nowrap text-xs">{v}</span>
                                </label>
                            ))
                        )}
                    </div>
                )}
                </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Priority</label>
                <select
                    className={selectClass}
                    value={filters.priority}
                    onChange={(e) => onFilterChange("priority", e.target.value)}
                >
                    <option value="">All</option>
                    {PRIORITY_OPTIONS.map((v) => (
                        <option key={v} value={v}>{v}</option>
                    ))}
                </select>
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Date From</label>
                <input
                    type="date"
                    className={selectClass}
                    value={filters.from}
                    onChange={(e) => onFilterChange("from", e.target.value)}
                />
            </div>

            <div className="flex flex-col gap-1 min-w-[120px]">
                <label className="text-[0.7rem] font-semibold text-slate-600 uppercase">Date To</label>
                <input
                    type="date"
                    className={selectClass}
                    value={filters.to}
                    onChange={(e) => onFilterChange("to", e.target.value)}
                />
            </div>

            <div className="inline-flex items-center gap-2 bg-indigo-50 rounded-full px-4 py-2 text-sm font-bold text-slate-800">
                Filtered: {rowCount} row{rowCount !== 1 ? "s" : ""}
            </div>

            <button
                type="button"
                onClick={onReset}
                className="ml-auto px-4 py-2 rounded-full bg-slate-400 text-white text-sm font-semibold hover:bg-slate-500"
            >
                Clear Filters
            </button>
        </div>
    );
};

export default DailyReportFilters;
