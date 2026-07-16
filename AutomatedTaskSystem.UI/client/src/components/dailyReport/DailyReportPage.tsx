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
        updateFilter,
        setPage,
        setPageSize,
        resetFilters,
        updateNotes,
        fetchAllRowsForExport,
    } = useDailyReport();

    if (loading && !summary) {
        return (
            <div className="flex items-center justify-center h-full">
                <Head>
                    <title>TMS - Daily Report</title>
                </Head>
                <Loader />
            </div>
        );
    }

    return (
        <div className="grow overflow-y-auto p-6 md:p-8 max-w-[1800px] mx-auto w-full">
            <Head>
                <title>TMS - Daily Report</title>
            </Head>

            <DailyReportHeader rows={rows} onExport={fetchAllRowsForExport} />

            {error && (
                <div className="mb-4 p-3 rounded-lg bg-red-100 text-red-700 text-sm">{error}</div>
            )}

            <div className="bg-white rounded-3xl p-5 mb-6 shadow-sm">
                <DailyReportStats summary={summary} />
                <DailyReportCharts charts={charts} />
            </div>

            <DailyReportFilters
                filters={filters}
                lookups={lookups}
                rowCount={pagination.totalCount}
                onFilterChange={updateFilter}
                onReset={resetFilters}
            />

            {loading ? (
                <div className="flex justify-center py-12">
                    <Loader />
                </div>
            ) : (
                <>
                    <DailyReportTable rows={rows} onUpdateNotes={updateNotes} />
                    <DailyReportPagination
                        pagination={pagination}
                        onPageChange={setPage}
                        onPageSizeChange={setPageSize}
                    />
                </>
            )}
        </div>
    );
};

export default DailyReportPage;
