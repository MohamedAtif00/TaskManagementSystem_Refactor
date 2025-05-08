import React, { useState } from "react";
import FilterIcon from "../../assets/Icons/Filter";

// Define the type for the props of the FilterableTable
type FilterableTableProps<T> = {
  data: T[]; // Data to be displayed in the table
  columns: Array<{ header: string; accessor: keyof T }>; // Column headers and accessors
  filters: any; // The filters applied to the data
  onFilterChange: (filters: any) => void; // Callback to handle filter changes
  getFilteredData: (filters: any) => T[]; // Function to get filtered data
  renderRow: (row: T) => JSX.Element; // Function to render each row
  filterComponents?: JSX.Element; // Optional filter components to render
};

const FilterableTable = <T,>({
  data,
  columns,
  filters,
  onFilterChange,
  getFilteredData,
  renderRow,
  filterComponents,
}: FilterableTableProps<T>) => {
  const [showFilters, setShowFilters] = useState(false);

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-medium text-gray-900">Requests</h2>
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="flex items-center px-3 py-2 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <FilterIcon color="#aaa" className="mr-2 w-10 h-10 border-none" />
            Filters
          </button>
        </div>

        {showFilters && <div className="mt-4">{filterComponents}</div>}
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {columns.map((column) => (
                <th
                  key={column.accessor as string}
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {getFilteredData(filters).map(renderRow)}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default FilterableTable;
