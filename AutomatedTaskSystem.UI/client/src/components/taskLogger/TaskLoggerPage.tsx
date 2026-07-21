import Head from "next/head";
import Loader from "../loader";
import TaskLoggerHeader from "./TaskLoggerHeader";
import TaskLoggerStats from "./TaskLoggerStats";
import TaskLoggerRankings from "./TaskLoggerRankings";
import TaskLoggerFilters from "./TaskLoggerFilters";
import TaskLoggerTable from "./TaskLoggerTable";
import TaskLoggerPagination from "./TaskLoggerPagination";
import { useTaskLogger } from "./hooks/useTaskLogger";

const TaskLoggerPage = () => {
    const {
        filters,
        rows,
        pagination,
        summary,
        rankings,
        lookups,
        loading,
        error,
        hasLoadedOnce,
        dupMode,
        dupOnly,
        dupLoading,
        duplicateColorMap,
        updateFilter,
        setPage,
        setPageSize,
        setRankingRange,
        resetFilters,
        fetchAllRowsForExport,
        toggleDetectDuplicates,
        toggleShowDuplicatesOnly,
        refetch,
    } = useTaskLogger();

    return (
        <div className="grow overflow-y-auto p-6 md:p-8 max-w-[1800px] mx-auto w-full">
            <Head>
                <title>TMS - Task Logger</title>
            </Head>

            <TaskLoggerHeader rows={rows} onExport={fetchAllRowsForExport} />

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
                    <div className="relative">
                        {loading && (
                            <div className="absolute inset-0 bg-white/50 z-10 flex items-center justify-center rounded-xl">
                                <Loader />
                            </div>
                        )}
                        <TaskLoggerStats summary={summary} />
                        <TaskLoggerRankings
                            rankings={rankings}
                            rankingRange={filters.rankingRange ?? "all"}
                            onRangeChange={setRankingRange}
                        />
                    </div>

                    <TaskLoggerFilters
                        filters={filters}
                        lookups={lookups}
                        rowCount={dupMode ? rows.length : pagination.totalCount}
                        onFilterChange={updateFilter}
                        onReset={resetFilters}
                    />

                    <div className="relative">
                        {loading && hasLoadedOnce && (
                            <div className="absolute inset-0 bg-white/70 z-10 flex items-center justify-center rounded-xl min-h-[200px]">
                                <Loader />
                            </div>
                        )}
                        <TaskLoggerTable
                            rows={rows}
                            dupMode={dupMode}
                            dupOnly={dupOnly}
                            dupLoading={dupLoading}
                            duplicateColorMap={duplicateColorMap}
                            onDetectDuplicates={toggleDetectDuplicates}
                            onShowDuplicatesOnly={toggleShowDuplicatesOnly}
                            totalLabelCount={dupMode ? rows.length : pagination.totalCount}
                        />

                        {!dupMode && (
                            <TaskLoggerPagination
                                pagination={pagination}
                                loading={loading}
                                onPageChange={setPage}
                                onPageSizeChange={setPageSize}
                            />
                        )}
                    </div>
                </>
            )}
        </div>
    );
};

export default TaskLoggerPage;
