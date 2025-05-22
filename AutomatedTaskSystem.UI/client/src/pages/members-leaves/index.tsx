import { useState, useEffect } from "react";
import { useRouter } from "next/router";
import API from "../../lib/API";
import ExportButton from "../../components/button/ExportButton";



type SortDirection = 'asc' | 'desc';
type SortField = 'hrCode' | 'name' | 'annual_leave' | 'sick_leave' | 'emergency_leave' | 'permission';

const MembersLeavesPage = () => {
  const router = useRouter();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedYear, setSelectedYear] = useState("2023");
  const [selectedMonth, setSelectedMonth] = useState("October");
  const [memberLeaves, setMemberLeaves] = useState<IUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sortField, setSortField] = useState<SortField>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  const fetchMemberLeaves = async () => {
    try {
      setLoading(true);
      const response = await API.RESOURCES.USERS.GET_ALL();
      if (response && !response.error && response.data) {
        setMemberLeaves(response.data);
        console.log(response.data,"data");
        
        setError(null);
      } else {
        setError("Failed to fetch member leaves data");
        setMemberLeaves([]);
      }
    } catch (err) {
      setError("Failed to fetch member leaves data");
      setMemberLeaves([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMemberLeaves();
  }, [searchTerm, selectedYear, selectedMonth]);

  const handleExportCSV = async () => {
    try {
      // TODO: Implement CSV export functionality
      alert('CSV export functionality will be implemented soon');
    } catch (err) {
      console.error('Failed to export CSV:', err);
      alert('Failed to export CSV file');
    }
  };

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getSortedData = () => {
    return [...memberLeaves].sort((a, b) => {
      let aValue, bValue;

      switch(sortField) {
        case 'annual_leave':
        case 'emergency_leave':
        case 'permission':
        case 'sick_leave':
          // Handle numeric fields
          aValue = Number(a[sortField]) || 0;
          bValue = Number(b[sortField]) || 0;
          break;
        case 'hrCode':
        case 'name':
          // Handle string fields
          aValue = String(a[sortField]).toLowerCase();
          bValue = String(b[sortField]).toLowerCase();
          break;
        default:
          aValue = a[sortField];
          bValue = b[sortField];
      }

      if (sortDirection === 'asc') {
        return aValue > bValue ? 1 : aValue < bValue ? -1 : 0;
      } else {
        return aValue < bValue ? 1 : aValue > bValue ? -1 : 0;
      }
    });
  };

  const renderSortIcon = (field: SortField) => {
    if (sortField !== field) return '↕️';
    return sortDirection === 'asc' ? '↑' : '↓';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Loading...</div>
      </div>
    );
  }

  return (
    <div className="p-6 w-full">
      <div className="flex items-center mb-6 bg-white">
        <h1 className="text-xl font-semibold">Members Leaves</h1>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-100 text-red-700 rounded">
          {error}
        </div>  
      )}

      <div className="flex gap-4 mb-4">
        <div className="flex-1">
          <input
            type="text"
            placeholder="Search by member name or email id"
            className="w-full p-2 border rounded"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div>
          <select
            value={selectedYear}
            onChange={(e) => setSelectedYear(e.target.value)}
            className="p-2 border rounded"
          >
            <option value="2023">2023</option>
            <option value="2024">2024</option>
          </select>
        </div>
        <div>
          <select
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(e.target.value)}
            className="p-2 border rounded"
          >
            <option value="January">January</option>
            <option value="February">February</option>
            <option value="March">March</option>
            <option value="April">April</option>
            <option value="May">May</option>
            <option value="June">June</option>
            <option value="July">July</option>
            <option value="August">August</option>
            <option value="September">September</option>
            <option value="October">October</option>
            <option value="November">November</option>
            <option value="December">December</option>
          </select>
        </div>
        <ExportButton data={memberLeaves}/>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-50 border-b">
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('hrCode')}
              >
                HR Code {renderSortIcon('hrCode')}
              </th>
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('name')}
              >
                Name {renderSortIcon('name')}
              </th>
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('annual_leave')}
              >
                Annual {renderSortIcon('annual_leave')}
              </th>
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('sick_leave')}
              >
                Sick {renderSortIcon('sick_leave')}
              </th>
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('emergency_leave')}
              >
                Emergency {renderSortIcon('emergency_leave')}
              </th>
              <th 
                className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase cursor-pointer hover:bg-gray-100"
                onClick={() => handleSort('permission')}
              >
                Permission {renderSortIcon('permission')}
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {getSortedData().map((member, index) => (
              <tr 
                key={index} 
                className="hover:bg-gray-50 cursor-pointer"
                onClick={() => router.push(`/resources/users/${member.id}`)}
              >
                <td className="px-6 py-4 text-sm text-gray-900">{member.hrCode}</td>
                <td className="px-6 py-4 text-sm text-gray-900">{member.name}</td>
                <td className="px-6 py-4 text-sm text-gray-900">{member.annual_leave}/{member.annual_leave_MAX}</td>
                <td className="px-6 py-4 text-sm text-gray-900">{member.sick_leave}</td>
                <td className="px-6 py-4 text-sm text-gray-900">{member.emergency_leave}/{member.emergency_leave_MAX}</td>
                <td className="px-6 py-4 text-sm text-gray-900">{member.permission}/{member.permission_MAX}</td>
              </tr>
            ))}
            {memberLeaves.length === 0 && (
              <tr>
                <td colSpan={6} className="px-6 py-4 text-center text-sm text-gray-500">
                  No member leaves found
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MembersLeavesPage;