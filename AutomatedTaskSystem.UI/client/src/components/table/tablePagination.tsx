import React, { useState, useCallback, useMemo, ReactNode, useEffect, useRef } from "react";
import {
    FiChevronLeft,
    FiChevronRight,
    FiChevronsLeft,
    FiChevronsRight,
    FiSearch,
} from "react-icons/fi";
import { format } from "date-fns";
import TableLoader from "../loader/table-loader";


// Helper for deep comparison (move this to a utility file if used elsewhere)
const deepEqual = (obj1: any, obj2: any): boolean => {
    if (obj1 === obj2) return true;
    if (typeof obj1 !== 'object' || obj1 === null || typeof obj2 !== 'object' || obj2 === null) return false;

    const keys1 = Object.keys(obj1);
    const keys2 = Object.keys(obj2);

    if (keys1.length !== keys2.length) return false;

    for (const key of keys1) {
        if (!keys2.includes(key) || !deepEqual(obj1[key], obj2[key])) {
            return false;
        }
    }
    return true;
};

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
    totalCount: number;
    onPageChange: (page: number, itemsPerPage: number, filters: Record<string, any>, searchText: string) => void;
    filterConfig?: FilterConfig[];
    itemsPerPage?: number;
    loading?: boolean;
    columnRenderers?: Record<string, (value: any, row?: Record<string, any>) => ReactNode>;
    serverSide?: boolean;
    showSearchInput?: boolean;
    shouldFocusSearch?: boolean;
    // New checkbox-related props
    enableSelection?: boolean;
    selectedRows?: Set<string | number>;
    onSelectionChange?: (selectedRows: Set<string | number>) => void;
    rowIdKey?: string; // Key to use as unique identifier for each row (defaults to array index)
    selectionMode?: 'multiple' | 'single'; // Defaults to 'multiple'
}

interface PaginationButtonProps {
    onClick: () => void;
    disabled: boolean;
    children: ReactNode;
    ariaLabel: string;
    isActive?: boolean;
}

const DataTable: React.FC<DataTableProps> = ({
    data = [],
    totalCount = 0,
    onPageChange,
    filterConfig = [],
    itemsPerPage: propItemsPerPage = 10,
    loading = false,
    columnRenderers = {},
    serverSide = false,
    showSearchInput = true,
    shouldFocusSearch = false,
    // New props with defaults
    enableSelection = false,
    selectedRows = new Set(),
    onSelectionChange,
    rowIdKey,
    selectionMode = 'multiple',
}) => {
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [currentItemsPerPage, setCurrentItemsPerPage] = useState<number>(propItemsPerPage);
    const [filters, setFilters] = useState<Record<string, any>>({});
    const [searchText, setSearchText] = useState<string>("");

    const searchInputRef = useRef<HTMLInputElement>(null);

    const lastSentPageRef = useRef<number>(1);
    const lastSentItemsPerPageRef = useRef<number>(propItemsPerPage);
    const lastSentFiltersRef = useRef<Record<string, any>>({});
    const lastSentSearchTextRef = useRef<string>("");
    const isInitialMount = useRef(true);

    // Helper function to get unique row identifier
    const getRowId = useCallback((row: Record<string, any>, index: number): string | number => {
        return rowIdKey && row[rowIdKey] !== undefined ? row[rowIdKey] : index;
    }, [rowIdKey]);

    useEffect(() => {
        if (shouldFocusSearch && searchInputRef.current) {
            searchInputRef.current.focus();
        }
    }, [shouldFocusSearch]);

    useEffect(() => {
        if (isInitialMount.current && serverSide) {
            isInitialMount.current = false;
            return;
        }

        const hasPageChanged = currentPage !== lastSentPageRef.current;
        const hasItemsPerPageChanged = currentItemsPerPage !== lastSentItemsPerPageRef.current;
        const hasSearchTextChanged = searchText !== lastSentSearchTextRef.current;
        const hasFiltersChanged = !deepEqual(filters, lastSentFiltersRef.current);

        if (!hasPageChanged && !hasItemsPerPageChanged && !hasSearchTextChanged && !hasFiltersChanged) {
            return;
        }

        const handler = setTimeout(() => {
            if (serverSide) {
                lastSentPageRef.current = currentPage;
                lastSentItemsPerPageRef.current = currentItemsPerPage;
                lastSentFiltersRef.current = filters;
                lastSentSearchTextRef.current = searchText;

                onPageChange(currentPage, currentItemsPerPage, filters, searchText);
            }
        }, 300);

        if (!serverSide) {
            onPageChange(currentPage, currentItemsPerPage, filters, searchText);
        }

        return () => {
            clearTimeout(handler);
        };

    }, [currentPage, currentItemsPerPage, filters, searchText, serverSide, onPageChange]);

    useEffect(() => {
        if (propItemsPerPage !== currentItemsPerPage) {
            setCurrentItemsPerPage(propItemsPerPage);
            setCurrentPage(1);
        }
    }, [propItemsPerPage, currentItemsPerPage]);

    const filteredData = useMemo(() => {
        if (serverSide) return data;

        let currentFilteredData = data;

        if (searchText.trim()) {
            currentFilteredData = currentFilteredData.filter(row =>
                Object.values(row).some(value =>
                    String(value).toLowerCase().includes(searchText.toLowerCase())
                )
            );
        }

        Object.keys(filters).forEach(filterKey => {
            const filterValue = filters[filterKey];
            if (filterValue !== "" && filterValue !== undefined && filterValue !== null && filterValue !== "all") {
                currentFilteredData = currentFilteredData.filter(row => {
                    const rowValue = row[filterKey];
                    if (rowValue === undefined || rowValue === null) return false;

                    const filterConfigItem = filterConfig.find(f => f.key === filterKey);

                    if (filterConfigItem?.type === "date") {
                        try {
                            return format(new Date(rowValue), 'yyyy-MM-dd') === format(new Date(filterValue), 'yyyy-MM-dd');
                        } catch (e) {
                            console.warn(`Invalid date value for filter key ${filterKey}:`, rowValue, filterValue);
                            return false;
                        }
                    }

                    return String(rowValue).toLowerCase().includes(String(filterValue).toLowerCase());
                });
            }
        });

        return currentFilteredData;
    }, [data, searchText, filters, serverSide, filterConfig]);

    const totalPages = useMemo(() => {
        if (serverSide) {
            return Math.ceil(totalCount / currentItemsPerPage);
        } else {
            return Math.ceil(filteredData.length / currentItemsPerPage);
        }
    }, [totalCount, currentItemsPerPage, filteredData.length, serverSide]);

    const handlePageChange = useCallback(
        (page: number) => {
            if (page < 1 || page > totalPages) return;
            setCurrentPage(page);
        },
        [totalPages]
    );

    const handleFilterChange = useCallback(
        (key: string, value: any) => {
            setCurrentPage(1);
            setFilters(prevFilters => {
                const newFilters = { ...prevFilters, [key]: value };
                if (deepEqual(prevFilters, newFilters)) {
                    return prevFilters;
                }
                return newFilters;
            });
        },
        []
    );

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setCurrentPage(1);
        setSearchText(e.target.value);
    }, []);

    const paginatedData = useMemo(() => {
        if (serverSide) return data;
        const startIndex = (currentPage - 1) * currentItemsPerPage;
        const endIndex = startIndex + currentItemsPerPage;
        return filteredData.slice(startIndex, endIndex);
    }, [data, filteredData, currentPage, currentItemsPerPage, serverSide]);

    // Selection handlers
    const handleRowSelect = useCallback((rowId: string | number, isSelected: boolean) => {
        debugger
        if (!onSelectionChange) return;
        
        const newSelection = new Set(selectedRows);
        
        if (selectionMode === 'single') {
            // For single selection, clear all and add only this one if selected
            newSelection.clear();
            if (isSelected) {
                newSelection.add(rowId);
            }
        } else {
            // For multiple selection
            if (isSelected) {
                newSelection.add(rowId);
            } else {
                newSelection.delete(rowId);
            }
        }
        
        onSelectionChange(newSelection);
    }, [selectedRows, onSelectionChange, selectionMode]);

    const handleSelectAll = useCallback((isSelected: boolean) => {
        if (!onSelectionChange || selectionMode === 'single') return;

        const newSelection = new Set<string | number>();
        
        if (isSelected) {
            // Add all visible rows to selection
            const dataToUse = serverSide ? data : paginatedData;
            dataToUse.forEach((row, index) => {
                const rowId = getRowId(row, index);
                newSelection.add(rowId);
            });
        }
        
        onSelectionChange(newSelection);
    }, [onSelectionChange, serverSide, data, paginatedData, getRowId, selectionMode]);

    // Check if all visible rows are selected
    const isAllSelected = useMemo(() => {
        if (!enableSelection || selectionMode === 'single') return false;
        
        const dataToUse = serverSide ? data : paginatedData;
        if (dataToUse.length === 0) return false;
        
        return dataToUse.every((row, index) => {
            const rowId = getRowId(row, index);
            return selectedRows.has(rowId);
        });
    }, [enableSelection, selectionMode, serverSide, data, paginatedData, selectedRows, getRowId]);

    // Check if some (but not all) visible rows are selected
    const isIndeterminate = useMemo(() => {
        if (!enableSelection || selectionMode === 'single') return false;
        
        const dataToUse = serverSide ? data : paginatedData;
        if (dataToUse.length === 0) return false;
        
        const selectedCount = dataToUse.filter((row, index) => {
            const rowId = getRowId(row, index);
            return selectedRows.has(rowId);
        }).length;
        
        return selectedCount > 0 && selectedCount < dataToUse.length;
    }, [enableSelection, selectionMode, serverSide, data, paginatedData, selectedRows, getRowId]);

    const getPageNumbers = useMemo(() => {
        const pageNumbers = [];
        const maxPagesToShow = 5;
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
        isActive = false,
    }) => (
        <button
            onClick={onClick}
            disabled={disabled}
            className={`px-4 py-2 mx-1 rounded-lg transition-colors duration-200
                ${disabled
                    ? "bg-gray-200 text-gray-500 cursor-not-allowed"
                    : isActive
                        ? "bg-blue-600 text-white shadow-md"
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

            {getPageNumbers.map((pageNumber) => (
                <PaginationButton
                    key={pageNumber}
                    onClick={() => handlePageChange(pageNumber)}
                    disabled={false}
                    ariaLabel={`Page ${pageNumber}`}
                    isActive={pageNumber === currentPage}
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
    ), [currentPage, totalPages, handlePageChange, getPageNumbers]);

    const renderCellContent = useCallback((header: string, value: any, row: Record<string, any>) => {
        if (columnRenderers[header]) {
            return columnRenderers[header](value, row);
        }

        if (value instanceof Date) {
            return format(value, "dd MMM yyyy");
        }

        if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(\.\d{3})?Z?)?$/.test(value)) {
            try {
                return format(new Date(value), "dd MMM yyyy");
            } catch (e) {
                return value;
            }
        }

        return String(value);
    }, [columnRenderers]);

    const displayData = serverSide ? data : paginatedData;
    const currentTotalCount = serverSide ? totalCount : filteredData.length;

    return (
        <div className="w-full bg-white rounded-lg shadow-lg p-6 relative">
            {loading && (
                <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10 rounded-lg">
                    <TableLoader />
                </div>
            )}

            <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
                {showSearchInput && (
                    <div className="relative col-span-full md:col-span-1 flex items-end">
                        <input
                            type="text"
                            ref={searchInputRef}
                            className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Search..."
                            value={searchText}
                            onChange={handleSearchChange}
                        />
                        <FiSearch className="absolute left-3 bottom-3 text-gray-400" />
                    </div>
                )}
                {filterConfig.map((filter) => (
                    <div key={filter.key} className="flex flex-col">
                        <label htmlFor={`filter-${filter.key}`} className="mb-1 text-sm font-medium text-gray-700">
                            {filter.label}
                        </label>
                        {renderFilterInput(filter)}
                    </div>
                ))}
            </div>

            {enableSelection && selectedRows.size > 0 && (
                <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                    <span className="text-sm text-blue-800">
                        {selectedRows.size} row{selectedRows.size !== 1 ? 's' : ''} selected
                    </span>
                </div>
            )}

            {displayData.length === 0 ? (
                <div className="text-center py-8 text-gray-500">
                    {searchText || Object.values(filters).some(f => f && f !== "all") ? "No results found for your criteria" : "No data available"}
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full table-auto">
                        <thead>
                            <tr className="bg-gray-50">
                                {enableSelection && (
                                    <th className="px-6 py-3 text-left">
                                        {selectionMode === 'multiple' && (
                                            <input
                                                type="checkbox"
                                                checked={isAllSelected}
                                                ref={(input) => {
                                                    if (input) input.indeterminate = isIndeterminate;
                                                }}
                                                onChange={(e) => handleSelectAll(e.target.checked)}
                                                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                                                aria-label="Select all rows"
                                            />
                                        )}
                                    </th>
                                )}
                                {Object.keys(displayData[0])
                                    .filter(header => !header.startsWith('original'))
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
                            {displayData.map((row, rowIndex) => {
                                const rowId = getRowId(row, rowIndex);
                                const isSelected = selectedRows.has(rowId);
                                
                                return (
                                    <tr 
                                        key={rowIndex} 
                                        className={`hover:bg-gray-50 transition-colors ${isSelected ? 'bg-blue-50' : ''}`}
                                    >
                                        {enableSelection && (
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <input
                                                    type="checkbox"
                                                    checked={isSelected}
                                                    onChange={(e) => handleRowSelect(rowId, e.target.checked)}
                                                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                                                    aria-label={`Select row ${rowIndex + 1}`}
                                                />
                                            </td>
                                        )}
                                        {Object.keys(row)
                                            .filter(header => !header.startsWith('original'))
                                            .map((header, cellIndex) => (
                                                <td
                                                    key={cellIndex}
                                                    className="px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                >
                                                    {renderCellContent(header, row[header], row)}
                                                </td>
                                            ))}
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {totalPages > 1 && renderPagination}

            <div className="mt-4 text-sm text-gray-500 text-center">
                Showing {displayData.length > 0 ? (currentPage - 1) * currentItemsPerPage + 1 : 0} to {Math.min(currentPage * currentItemsPerPage, currentTotalCount)} of {currentTotalCount} results
                {(searchText || Object.values(filters).some(f => f && f !== "all")) && ` (filtered)`}
            </div>
        </div>
    );
};

export default DataTable;