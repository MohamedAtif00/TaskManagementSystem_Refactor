import Loader from "../loader";

interface Props {
    pagination: {
        page: number;
        totalPages: number;
        totalCount: number;
        pageSize: number;
    };
    loading?: boolean;
    onPageChange: (page: number) => void;
    onPageSizeChange: (pageSize: number) => void;
}

const TaskLoggerPagination = ({ pagination, loading = false, onPageChange, onPageSizeChange }: Props) => {
    const { page, totalPages, totalCount, pageSize } = pagination;
    if (totalCount === 0) return null;

    const start = (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalCount);

    return (
        <div className="flex flex-wrap items-center justify-between gap-3 mb-8">
            <div className="text-sm text-slate-600">
                Showing {start}–{end} of {totalCount}
            </div>
            <div className="flex items-center gap-2">
                <select
                    value={pageSize}
                    disabled={loading}
                    onChange={(e) => onPageSizeChange(Number(e.target.value))}
                    className="px-2 py-1.5 rounded-full bg-white border border-slate-200 text-sm disabled:opacity-60"
                >
                    {[5, 10, 25, 50, 100].map((n) => (
                        <option key={n} value={n}>
                            {n} / page
                        </option>
                    ))}
                </select>
                <button
                    type="button"
                    disabled={loading || page <= 1}
                    onClick={() => onPageChange(page - 1)}
                    className="px-3 py-1.5 rounded-full bg-white border border-slate-200 text-sm font-semibold disabled:opacity-40"
                >
                    Previous
                </button>
                <span className="text-sm font-semibold">
                    {page} / {Math.max(1, totalPages)}
                </span>
                <button
                    type="button"
                    disabled={loading || page >= totalPages}
                    onClick={() => onPageChange(page + 1)}
                    className="px-3 py-1.5 rounded-full bg-white border border-slate-200 text-sm font-semibold disabled:opacity-40"
                >
                    Next
                </button>
                {loading && <Loader />}
            </div>
        </div>
    );
};

export default TaskLoggerPagination;
