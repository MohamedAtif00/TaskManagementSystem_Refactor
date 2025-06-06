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
    itemsPerPage?: number; // Default value is handled internally
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
    isActive?: boolean; // New prop for active page styling
}

const DataTable: React.FC<DataTableProps> = ({
    data = [],
    totalCount = 0,
    onPageChange,
    filterConfig = [],
    itemsPerPage: propItemsPerPage = 10, // Renamed to avoid confusion with internal state
    loading = false,
    columnRenderers = {},
    serverSide = false,
}) => {
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [currentItemsPerPage, setCurrentItemsPerPage] = useState<number>(propItemsPerPage);
    const [filters, setFilters] = useState<Record<string, any>>({});
    const [searchText, setSearchText] = useState<string>("");
    const initialRenderRef = useRef(true); // Ref to track initial render for useEffect

    // Effect to trigger API call when page, filters, or search text changes
    useEffect(() => {
        // Skip API call on initial render if serverSide is true,
        // assuming parent component triggers initial data fetch
        if (serverSide && initialRenderRef.current) {
            initialRenderRef.current = false;
            return;
        }

        if (serverSide) {
            // Debounce searchText and filters to prevent excessive API calls
            const handler = setTimeout(() => {
                onPageChange(currentPage, currentItemsPerPage, filters, searchText);
            }, 300); // Standard debounce for 300ms

            return () => {
                clearTimeout(handler);
            };
        } else {
            // For client-side, the onPageChange callback can be called directly
            // when data or filters change, to inform the parent about current state
            onPageChange(currentPage, currentItemsPerPage, filters, searchText);
        }
    }, [currentPage, currentItemsPerPage, filters, searchText, serverSide, onPageChange]);

    // Update internal itemsPerPage if propItemsPerPage changes from parent
    useEffect(() => {
        if (propItemsPerPage !== currentItemsPerPage) {
            setCurrentItemsPerPage(propItemsPerPage);
            setCurrentPage(1); // Reset page if items per page changes
        }
    }, [propItemsPerPage]);


     // Client-side filtering and pagination (only if not serverSide)
    const filteredData = useMemo(() => {
        if (serverSide) return data; // If server-side, data is already filtered/paginated by the parent

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
            // Only apply filter if value is not empty/all for select and not undefined/null
            if (filterValue !== "" && filterValue !== undefined && filterValue !== null && filterValue !== "all") {
                currentFilteredData = currentFilteredData.filter(row => {
                    const rowValue = row[filterKey];
                    if (rowValue === undefined || rowValue === null) return false;

                    // Special handling for date filtering for client-side
                    if (filterConfig.find(f => f.key === filterKey)?.type === "date") {
                        try {
                            // Compare dates based on YYYY-MM-DD
                            return format(new Date(rowValue), 'yyyy-MM-dd') === format(new Date(filterValue), 'yyyy-MM-dd');
                        } catch (e) {
                            console.warn(`Invalid date value for filter key ${filterKey}:`, rowValue, filterValue);
                            return false;
                        }
                    }

                    // Default string comparison for other types
                    return String(rowValue).toLowerCase().includes(String(filterValue).toLowerCase());
                });
            }
        });

        return currentFilteredData;
    }, [data, searchText, filters, serverSide, filterConfig]); // Added filterConfig to dependencies
        // totalPages calculation depends on whether it's server-side or client-side
    const totalPages = useMemo(() => {
        if (serverSide) {
            return Math.ceil(totalCount / currentItemsPerPage);
        } else {
            return Math.ceil(filteredData.length / currentItemsPerPage);
        }
    }, [totalCount, currentItemsPerPage, filteredData.length, serverSide]);

    const handlePageChange = useCallback(
        (page: number) => {
            if (page < 1 || page > totalPages) return; // Prevent invalid page numbers
            setCurrentPage(page);
            // onPageChange for server-side is handled by the useEffect debounce
            // For client-side, it's implicitly handled by the useMemo dependencies
        },
        [totalPages] // Dependency on totalPages is important here
    );

    const handleFilterChange = useCallback(
        (key: string, value: any) => {
            setCurrentPage(1); // Always reset to first page on filter change
            const updatedFilters = { ...filters, [key]: value };
            setFilters(updatedFilters);
            // onPageChange for server-side is handled by the useEffect debounce
        },
        [filters]
    );

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setCurrentPage(1); // Always reset to first page on search change
        setSearchText(e.target.value);
        // onPageChange for server-side is handled by the useEffect debounce
    }, []);

   

    const paginatedData = useMemo(() => {
        if (serverSide) return data; // If server-side, data is already paginated by the parent
        const startIndex = (currentPage - 1) * currentItemsPerPage;
        const endIndex = startIndex + currentItemsPerPage;
        return filteredData.slice(startIndex, endIndex);
    }, [data, filteredData, currentPage, currentItemsPerPage, serverSide]);




    // Generate page numbers for pagination control
    const getPageNumbers = useMemo(() => {
        const pageNumbers = [];
        const maxPagesToShow = 5; // e.g., 1 2 [3] 4 5
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            pageNumbers.push(i);
        }
        return pageNumbers;
    }, [currentPage, totalPages]);


    const renderFilterInput = useCallback(
        (filter: FilterConfig): ReactNode => {
            const commonClasses = "w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500";
            switch (filter.type) {
                case "text":
                case "number":
                    return (
                        <input
                            type={filter.type}
                            className={commonClasses}
                            placeholder={filter.label}
                            onChange={(e) => handleFilterChange(filter.key, e.target.value)}
                            value={filters[filter.key] || ""}
                        />
                    );
                case "select":
                    return (
                        <select
                            className={commonClasses}
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
                            className={commonClasses}
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
        isActive = false, // Default to false
    }) => (
        <button
            onClick={onClick}
            disabled={disabled}
            className={`px-4 py-2 mx-1 rounded-lg transition-colors duration-200
                ${disabled
                    ? "bg-gray-200 text-gray-500 cursor-not-allowed"
                    : isActive
                        ? "bg-blue-600 text-white shadow-md" // Active page style
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
                disabled={currentPage === 1 || totalPages === 0}
                ariaLabel="First page"
            >
                <FiChevronsLeft />
            </PaginationButton>
            <PaginationButton
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1 || totalPages === 0}
                ariaLabel="Previous page"
            >
                <FiChevronLeft />
            </PaginationButton>

            {/* Render dynamic page numbers */}
            {getPageNumbers.map((pageNumber) => (
                <PaginationButton
                    key={pageNumber}
                    onClick={() => handlePageChange(pageNumber)}
                    disabled={false} // Page numbers themselves are not disabled unless totalPages is 0
                    ariaLabel={`Page ${pageNumber}`}
                    isActive={pageNumber === currentPage} // Highlight active page
                >
                    {pageNumber}
                </PaginationButton>
            ))}

            <PaginationButton
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage >= totalPages || totalPages === 0}
                ariaLabel="Next page"
            >
                <FiChevronRight />
            </PaginationButton>
            <PaginationButton
                onClick={() => handlePageChange(totalPages)}
                disabled={currentPage >= totalPages || totalPages === 0}
                ariaLabel="Last page"
            >
                <FiChevronsRight />
            </PaginationButton>
        </div>
    ), [currentPage, totalPages, handlePageChange, getPageNumbers]); // Added getPageNumbers to dependencies

    const renderCellContent = useCallback((header: string, value: any, row: Record<string, any>) => {
        if (columnRenderers[header]) {
            return columnRenderers[header](value, row);
        }

        // Handle Date objects explicitly if they are passed as such
        if (value instanceof Date) {
            return format(value, "PP"); // Example: Oct 20, 2024
        }
        
        // Handle ISO date strings (like "2025-06-05T20:04:19.123Z" or "2025-06-05")
        // Check if it looks like a date string and attempt to format it
        if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(\.\d{3})?Z?)?$/.test(value)) {
            try {
                return format(new Date(value), "dd MMM yyyy"); // e.g., 05 Jun 2025
            } catch (e) {
                // Fallback if parsing fails
                return value;
            }
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
                <div className="relative col-span-full md:col-span-1 flex items-end">
                    <input
                        type="text"
                        className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="Search..."
                        value={searchText}
                        onChange={handleSearchChange} // Use new handleSearchChange
                    />
                    <FiSearch className="absolute left-3 bottom-3 text-gray-400" /> {/* Adjusted icon position */}
                </div>
                {filterConfig.map((filter) => (
                    <div key={filter.key} className="flex flex-col">
                        <label htmlFor={`filter-${filter.key}`} className="mb-1 text-sm font-medium text-gray-700">
                            {filter.label}
                        </label>
                        {renderFilterInput(filter)}
                    </div>
                ))}
            </div>

            {displayData.length === 0 ? (
                <div className="text-center py-8 text-gray-500">
                    {searchText || Object.values(filters).some(f => f && f !== "all") ? "No results found for your criteria" : "No data available"}
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full table-auto">
                        <thead>
                            <tr className="bg-gray-50">
                                {/* Filter out internal keys like 'originalLeave' / 'originalPermission' from table headers */}
                                {Object.keys(displayData[0])
                                    .filter(header => !header.startsWith('original')) // Filter out internal keys
                                    .map((header) => (
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
                                    {/* Iterate over filtered headers for cells */}
                                    {Object.keys(row)
                                        .filter(header => !header.startsWith('original')) // Filter out internal keys
                                        .map((header, cellIndex) => (
                                            <td
                                                key={cellIndex}
                                                className="px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                            >
                                                {renderCellContent(header, row[header], row)}
                                            </td>
                                        ))}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Only render pagination if there are actual pages to show */}
            {totalPages > 1 && renderPagination}
            
            <div className="mt-4 text-sm text-gray-500 text-center">
                Showing {displayData.length > 0 ? (currentPage - 1) * currentItemsPerPage + 1 : 0} to {Math.min(currentPage * currentItemsPerPage, currentTotalCount)} of {currentTotalCount} results
                {(searchText || Object.values(filters).some(f => f && f !== "all")) && ` (filtered)`}
            </div>
        </div>
    );
};

export default DataTable;