import React, { useState, useCallback } from 'react';
import { FaFileExport } from 'react-icons/fa';

interface ServerExportButtonProps {
    /**
     * An async function that fetches the data to be exported.
     * It should return an array of objects.
     */
    fetchDataFunction: () => Promise<any[]>;
    /**
     * The desired filename for the exported CSV file.
     * @default 'export.csv'
     */
    filename?: string;
    /**
     * The text label displayed on the button.
     * @default 'Export Data'
     */
    label?: string;
    /**
     * Optional Tailwind CSS classes to apply to the button.
     */
    className?: string;
}

/**
 * A reusable button component that fetches data from a specified source
 * and then exports it as a CSV file.
 */
const ServerExportButton: React.FC<ServerExportButtonProps> = ({
    fetchDataFunction,
    filename = 'export.csv',
    label = 'Export Data',
    className = 'bg-green-600 hover:bg-green-700 text-white font-medium py-2 px-4 rounded-md flex items-center gap-2 transition-colors duration-200'
}) => {
    const [loading, setLoading] = useState(false);

    /**
     * Helper function to convert an array of objects to CSV string and trigger download.
     * This function is self-contained within the component for convenience,
     * but could be moved to a shared utility file if used elsewhere.
     */
    const exportDataToCSV = useCallback((data: any[], exportFilename: string) => {
        if (!data || data.length === 0) {
            // Use a custom modal or toast notification instead of alert()
            console.warn("No data to export!");
            return;
        }

        // Get headers from the first object's keys
        const headers = Object.keys(data[0]);
        const csvRows = [
            headers.map(header => `"${header.replace(/"/g, '""')}"`).join(','), // Header row, escaped
            ...data.map(row => headers.map(fieldName => {
                const value = row[fieldName];
                // Handle null/undefined, and escape quotes within cell values
                const escapedValue = value === null || value === undefined ? '' : String(value).replace(/"/g, '""');
                return `"${escapedValue}"`;
            }).join(','))
        ];

        const csvString = csvRows.join('\n');
        const blob = new Blob([csvString], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');

        // Feature detection for download attribute
        if (link.download !== undefined) {
            const url = URL.createObjectURL(blob);
            link.setAttribute('href', url);
            link.setAttribute('download', exportFilename);
            link.style.visibility = 'hidden'; // Hide the link
            document.body.appendChild(link);
            link.click(); // Programmatically click the link
            document.body.removeChild(link); // Clean up
            URL.revokeObjectURL(url); // Release the object URL
        } else {
            // Fallback for browsers that don't support download attribute (less common now)
            window.open(URL.createObjectURL(blob));
        }
    }, []); // No dependencies needed as it's a pure function

    const handleClick = useCallback(async () => {
        setLoading(true);
        try {
            const dataToExport = await fetchDataFunction();
            exportDataToCSV(dataToExport, filename);
        } catch (error) {
            console.error("Error during export:", error);
            // You might want to show an error message to the user here
        } finally {
            setLoading(false);
        }
    }, [fetchDataFunction, filename, exportDataToCSV]);

    return (
        <button
            onClick={handleClick}
            disabled={loading}
            className={`rounded-lg ${className} ${loading ? 'opacity-70 cursor-not-allowed' : ''}`}
        >
            {loading ? (
                <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Exporting...
                </>
            ) : (
                <>
                    <FaFileExport size={20} />
                    <span>{label}</span>
                </>
            )}
        </button>
    );
};

export default ServerExportButton;