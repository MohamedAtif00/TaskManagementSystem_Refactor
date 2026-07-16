interface Props {
    pagination: {
        totalCount: number;
        page: number;
        pageSize: number;
        totalPages: number;
    };
    onPageChange: (page: number) => void;
    onPageSizeChange: (pageSize: number) => void;
}

const DailyReportPagination = ({ pagination, onPageChange, onPageSizeChange }: Props) => {
    const { page, totalPages, totalCount, pageSize } = pagination;
    if (totalCount === 0) return null;

    const start = (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalCount);

    return (
        <div className="flex flex-wrap items-center justify-between gap-3 mt-4 px-2">
            <span className="text-sm text-slate-600">
                Showing {start}–{end} of {totalCount}
            </span>
            <div className="flex items-center gap-2">
                <label className="text-sm text-slate-600">Per page</label>
                <select
                    className="px-2 py-1 rounded-full border border-slate-200 text-sm"
                    value={pageSize}
                    onChange={(e) => onPageSizeChange(Number(e.target.value))}
                >
                    {[5, 10, 25, 50, 100].map((n) => (
                        <option key={n} value={n}>{n}</option>
                    ))}
                </select>
                <button
                    type="button"
                    disabled={page <= 1}
                    onClick={() => onPageChange(page - 1)}
                    className="px-3 py-1.5 rounded-full bg-white border border-slate-200 text-sm font-semibold disabled:opacity-40"
                >
                    Previous
                </button>
                <span className="text-sm font-semibold text-slate-700">
                    Page {page} of {Math.max(totalPages, 1)}
                </span>
                <button
                    type="button"
                    disabled={page >= totalPages}
                    onClick={() => onPageChange(page + 1)}
                    className="px-3 py-1.5 rounded-full bg-white border border-slate-200 text-sm font-semibold disabled:opacity-40"
                >
                    Next
                </button>
            </div>
        </div>
    );
};

export default DailyReportPagination;
