import Head from "next/head";
import Loader from "../loader";
import DailyReportHeader from "./DailyReportHeader";
import DailyReportStats from "./DailyReportStats";
import DailyReportCharts from "./DailyReportCharts";
import DailyReportFilters from "./DailyReportFilters";
import DailyReportTable from "./DailyReportTable";
import DailyReportPagination from "./DailyReportPagination";
import { useDailyReport } from "./hooks/useDailyReport";

const DailyReportPage = () => {
    const {
        filters,
        rows,
        pagination,
        summary,
        charts,
        lookups,
        loading,
        error,
        hasLoadedOnce,
        updateFilter,
        setPage,
        setPageSize,
        setProblemTypes,
        resetFilters,
        fetchAllRowsForExport,
        refetch,
    } = useDailyReport();

    return (
        <div className="grow overflow-y-auto p-6 md:p-8 max-w-[1800px] mx-auto w-full">
            <Head>
                <title>TMS - Daily Report</title>
            </Head>

            <DailyReportHeader rows={rows} onExport={fetchAllRowsForExport} />

            {error && (
                <div className="mb-4 p-3 rounded-lg bg-red-100 text-red-700 text-sm flex items-center justify-between gap-3">
                    <span>{error}</span>
                    <button
                        type="button"
                        onClick={refetch}
                        className="shrink-0 px-3 py-1 rounded-full bg-red-200 text-red-900 text-xs font-semibold"
                    >
                        Retry
                    </button>
                </div>
            )}

            {!hasLoadedOnce && loading ? (
                <div className="flex items-center justify-center py-24">
                    <Loader />
                </div>
            ) : (
                <>
                    <div className="bg-white rounded-3xl p-5 mb-6 shadow-sm relative">
                        {loading && (
                            <div className="absolute inset-0 bg-white/60 rounded-3xl flex items-center justify-center z-10">
                                <Loader />
                            </div>
                        )}
                        <DailyReportStats summary={summary} />
                        <DailyReportCharts charts={charts} />
                    </div>

                    <DailyReportFilters
                        filters={filters}
                        lookups={lookups}
                        rowCount={pagination.totalCount}
                        onFilterChange={updateFilter}
                        onProblemTypesChange={setProblemTypes}
                        onReset={resetFilters}
                    />

                    <div className="relative">
                        {loading && hasLoadedOnce && (
                            <div className="absolute inset-0 bg-white/70 z-10 flex items-center justify-center rounded-3xl min-h-[200px]">
                                <Loader />
                            </div>
                        )}
                        <DailyReportTable rows={rows} />
                        <DailyReportPagination
                            pagination={pagination}
                            loading={loading}
                            onPageChange={setPage}
                            onPageSizeChange={setPageSize}
                        />
                    </div>
                </>
            )}
        </div>
    );
};

export default DailyReportPage;
