import React, { useState, useCallback, useMemo, ReactNode, useEffect, useRef } from "react";
import {
    FiChevronLeft,
    FiChevronRight,
    FiChevronsLeft,
    FiChevronsRight,
    FiSearch,
} from "react-icons/fi";
import { format } from "date-fns";

interface FilterOption {
    value: string | number;
    label: string;
}

type FilterType = "text" | "select" | "date" | "number";

interface FilterConfig {
    key: string;
    label: string;
    type: FilterType;
    options?: FilterOption[]; // Only used for select
}

interface DataTableProps {
    data: Record<string, any>[];
    totalCount: number; // This will now come from the API response
    // onPageChange will now also send filter state
    onPageChange: (page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => void;
    filterConfig?: FilterConfig[];
    // customFilterHandler is redundant if onPageChange handles all updates
    // customFilterHandler?: (filters: Record<string, any>) => void;
    itemsPerPage?: number;
    loading?: boolean;
    columnRenderers?: Record<string, (value: any, row?: Record<string, any>) => ReactNode>;
    // Optional prop to indicate if pagination and filtering are handled externally (server-side)
    serverSide?: boolean;
}

interface PaginationButtonProps {
    onClick: () => void;
    disabled: boolean;
    children: ReactNode;
    ariaLabel: string;
}

const DataTable: React.FC<DataTableProps> = ({
    data = [],
    totalCount = 0,
    onPageChange, // This will be the main callback for server-side
    filterConfig = [],
    // customFilterHandler, // No longer needed for server-side
    itemsPerPage = 10,
    loading = false,
    columnRenderers = {},
    serverSide = false, // New prop to toggle server-side behavior
}) => {
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [filters, setFilters] = useState<Record<string, any>>({});
    const [searchText, setSearchText] = useState<string>("");

    // Effect to trigger API call when page, filters, or search text changes
    useEffect(() => {
        if (serverSide) {
            // Debounce searchText to prevent excessive API calls
            const handler = setTimeout(() => {
                onPageChange(currentPage, itemsPerPage, filters, searchText);
            }, 500); // Debounce for 300ms

            return () => {
                clearTimeout(handler);
            };
        }
    }, [currentPage, itemsPerPage, filters, searchText, serverSide, onPageChange]);


    const handlePageChange = useCallback(
        (page: number) => {
            setCurrentPage(page);
            if (!serverSide) {
                // Client-side pagination logic
                onPageChange?.(page, itemsPerPage, filters, searchText);
            }
            // For server-side, useEffect will trigger the API call
        },
        [itemsPerPage, onPageChange, serverSide, filters, searchText]
    );

    const handleFilterChange = useCallback(
        (key: string, value: any) => {
            setCurrentPage(1); // Reset to first page on filter change
            const updatedFilters = { ...filters, [key]: value };
            setFilters(updatedFilters);
            if (!serverSide) {
                // Client-side filtering logic
                // If not server-side, you might want to re-filter immediately or apply a custom handler
                // The current client-side logic re-filters via useMemo (filteredData)
            }
            // For server-side, useEffect will trigger the API call
        },
        [filters, serverSide]
    );

    // Client-side filtering and pagination (only if not serverSide)
    const filteredData = useMemo(() => {
        if (serverSide) return data; // If server-side, data is already filtered/paginated
        
        let currentFilteredData = data;

        // Apply text search
        if (searchText.trim()) {
            currentFilteredData = currentFilteredData.filter(row =>
                Object.values(row).some(value =>
                    String(value).toLowerCase().includes(searchText.toLowerCase())
                )
            );
        }

        // Apply additional filters (for client-side)
        Object.keys(filters).forEach(filterKey => {
            const filterValue = filters[filterKey];
            if (filterValue !== "" && filterValue !== undefined && filterValue !== null) {
                currentFilteredData = currentFilteredData.filter(row =>
                    String(row[filterKey]).toLowerCase().includes(String(filterValue).toLowerCase())
                );
            }
        });

        return currentFilteredData;
    }, [data, searchText, filters, serverSide]);

    const paginatedData = useMemo(() => {
        if (serverSide) return data; // If server-side, data is already paginated
        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        return filteredData.slice(startIndex, endIndex);
    }, [data, filteredData, currentPage, itemsPerPage, serverSide]);


    // totalPages calculation depends on whether it's server-side or client-side
    const totalPages = useMemo(() => {
        if (serverSide) {
            return Math.ceil(totalCount / itemsPerPage);
        } else {
            return Math.ceil(filteredData.length / itemsPerPage);
        }
    }, [totalCount, itemsPerPage, filteredData.length, serverSide]);


    const renderFilterInput = useCallback(
        (filter: FilterConfig): ReactNode => {
            switch (filter.type) {
                case "text":
                case "number":
                    return (
                        <input
                            type={filter.type}
                            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder={filter.label}
                            onChange={(e) => handleFilterChange(filter.key, e.target.value)}
                            value={filters[filter.key] || ""}
                        />
                    );
                case "select":
                    return (
                        <select
                            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            onChange={(e) => handleFilterChange(filter.key, e.target.value)}
                            value={filters[filter.key] || ""}
                        >
                            <option value="">Select {filter.label}</option>
                            {filter.options?.map((option) => (
                                <option key={String(option.value)} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    );
                case "date":
                    return (
                        <input
                            type="date"
                            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            onChange={(e) => handleFilterChange(filter.key, e.target.value)}
                            value={filters[filter.key] || ""}
                        />
                    );
                default:
                    return null;
            }
        },
        [filters, handleFilterChange]
    );

    const PaginationButton: React.FC<PaginationButtonProps> = ({
        onClick,
        disabled,
        children,
        ariaLabel,
    }) => (
        <button
            onClick={onClick}
            disabled={disabled}
            className={`px-4 py-2 mx-1 rounded-lg ${
                disabled
                    ? "bg-gray-200 cursor-not-allowed"
                    : "bg-blue-500 hover:bg-blue-600 text-white"
            }`}
            aria-label={ariaLabel}
        >
            {children}
        </button>
    );

    const renderPagination = useMemo(() => (
        <div className="flex items-center justify-center mt-4 space-x-2">
            <PaginationButton
                onClick={() => handlePageChange(1)}
                disabled={currentPage === 1}
                ariaLabel="First page"
            >
                <FiChevronsLeft />
            </PaginationButton>
            <PaginationButton
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                ariaLabel="Previous page"
            >
                <FiChevronLeft />
            </PaginationButton>
            <span className="px-4 py-2 text-gray-700">
                Page {currentPage} of {totalPages}
            </span>
            <PaginationButton
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage >= totalPages} // Use >= for totalPages
                ariaLabel="Next page"
            >
                <FiChevronRight />
            </PaginationButton>
            <PaginationButton
                onClick={() => handlePageChange(totalPages)}
                disabled={currentPage >= totalPages} // Use >= for totalPages
                ariaLabel="Last page"
            >
                <FiChevronsRight />
            </PaginationButton>
        </div>
    ), [currentPage, totalPages, handlePageChange]);

    const renderCellContent = useCallback((header: string, value: any, row: Record<string, any>) => {
        if (columnRenderers[header]) {
            return columnRenderers[header](value, row);
        }
        
        // Handle Date objects explicitly if they are passed as such
        if (value instanceof Date) {
            return format(value, "PP");
        }
        
        return String(value);
    }, [columnRenderers]);

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-[400px]">
                <div className="w-16 h-16 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    // Determine which data set to use for display (paginatedData or just data if serverSide)
    const displayData = serverSide ? data : paginatedData;
    const currentTotalCount = serverSide ? totalCount : filteredData.length;

    return (
        <div className="w-full bg-white rounded-lg shadow-lg p-6">
            <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="relative flex items-end">
                    <input
                        type="text"
                        className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Search..."
                        value={searchText}
                        onChange={(e) => setSearchText(e.target.value)}
                    />
                    <FiSearch className="absolute left-3 top-9 text-gray-400" />
                </div>
                {filterConfig.map((filter, index) => (
                    <div key={filter.key} className="flex flex-col"> {/* Use filter.key for key */}
                        <label className="mb-1 text-sm font-medium text-gray-700">
                            {filter.label}
                        </label>
                        {renderFilterInput(filter)}
                    </div>
                ))}
            </div>

            {displayData.length === 0 ? (
                <div className="text-center py-8 text-gray-500">
                    {searchText || Object.values(filters).some(f => f) ? "No results found for your criteria" : "No data available"}
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full table-auto">
                        <thead>
                            <tr className="bg-gray-50">
                                {Object.keys(displayData[0]).map((header) => (
                                    <th
                                        key={header}
                                        className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                    >
                                        {header}
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {displayData.map((row, rowIndex) => (
                                <tr key={rowIndex} className="hover:bg-gray-50 transition-colors">
                                    {Object.entries(row).map(([header, value], cellIndex) => (
                                        <td
                                            key={cellIndex}
                                            className="px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                        >
                                            {renderCellContent(header, value, row)}
                                        </td>
                                    ))}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {currentTotalCount > itemsPerPage && renderPagination}
            
            <div className="mt-4 text-sm text-gray-500 text-center">
                Showing {Math.min(displayData.length, itemsPerPage)} of {currentTotalCount} results
                {(searchText || Object.values(filters).some(f => f)) && serverSide && ` (filtered by server)`}
                {!serverSide && searchText && ` (filtered from ${data.length} total)`}
            </div>
        </div>
    );
};

export default DataTable;