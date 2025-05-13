import { CSVLink } from 'react-csv';
import { FaFileExport } from 'react-icons/fa';

interface ExportButtonProps {
  data: any[];
  filename?: string;
  className?: string;
}

const ExportButton = ({ 
  data, 
  filename = 'export.csv',
  className = 'bg-green-500 hover:bg-green-600 text-white font-medium py-2 px-4 rounded-md flex items-center gap-2 transition-colors duration-200'
}: ExportButtonProps) => {
  return (
    <CSVLink 
      data={data}
      filename={filename}
      className={className}
    >
      {/* <FaFileExport size={20} /> */}
      <span>Export CSV</span>
    </CSVLink>
  );
};

export default ExportButton;