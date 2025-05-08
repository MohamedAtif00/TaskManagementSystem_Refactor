import React, { useState, useEffect } from "react";
import { format } from "date-fns";
import LeftArrowIcon from "../../assets/Icons/LeftArrow";
import RightArrowIcon from "../../assets/Icons/RightArrow";
import FilterIcon from "../../assets/Icons/Filter";
import { FormControl, InputLabel, MenuItem, Select, SelectChangeEvent, TextField, TextFieldProps } from "@mui/material";
import { DatePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { Dayjs } from "dayjs";


type LeaveRequestStatus = "approved" | "pending" | "rejected" | string; // Adjust if you have a stricter enum or union

interface StatusBadgeProps {
  status: LeaveRequestStatus;
}



type Leave = {
    id: number;
    type: string;
    startDate: string;
    endDate: string;
    duration: string;
    status: string;
  };
  
  type Permission = {
    id: number;
    type: string;
    date: string;
    time: string;
    duration: string;
    status: string;
  };

  type CombinedData = (Leave | Permission)[];


const thClass = "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"

const LeaveManagement = () => {
  const [activeTab, setActiveTab] = useState("leaves");
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState<{
    fromDate: Dayjs | null;
    toDate: Dayjs | null;
    type: string;
  }>({
    fromDate: null,
    toDate: null,
    type: '',
  });

  
  const mockLeaveData = [
    {
      id: 1,
      type: "Annual Leave",
      startDate: "2024-01-15",
      endDate: "2024-01-17",
      duration: "3 days",
      status: "Approved"
    },
    {
      id: 2,
      type: "Sick Leave",
      startDate: "2024-01-20",
      endDate: "2024-01-20",
      duration: "1 day",
      status: "Pending"
    }
  ];

  const mockPermissionData = [
    {
      id: 1,
      type: "Short Leave",
      date: "2024-01-10",
      time: "14:00-16:00",
      duration: "2 hours",
      status: "Approved"
    }
  ];

  const LeaveBalanceCard = ({ title, used, total, color }:{title:string,used:number,total:number,color:string}) => (
    <div className="bg-white p-4 rounded-lg shadow-md">
      <h3 className="text-gray-700 font-medium mb-2">{title}</h3>
      <div className="flex items-center">
        <div className="w-full bg-gray-200 rounded-full h-2.5">
          <div
            className={`h-2.5 rounded-full ${color}`}
            style={{ width: `${(used / total) * 100}%` }}
          ></div>
        </div>
        <span className="ml-2 text-sm text-gray-600">{used}/{total}</span>
      </div>
    </div>
  );

  const StatusBadge = ({ status }: StatusBadgeProps) => {
    const getStatusColor = (status: string) => {
      switch (status.toLowerCase()) {
        case "approved":
          return "bg-green-100 text-green-800";
        case "pending":
          return "bg-yellow-100 text-yellow-800";
        case "rejected":
          return "bg-red-100 text-red-800";
        default:
          return "bg-gray-100 text-gray-800";
      }
    };

    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(status)}`}>
        {status}
      </span>
    );
  };

  // const handleFilterChange = (e:React.ChangeEvent<HTMLInputElement>) => {
  //   const { name, value } = e.target;
  //   setFilters(prev => ({
  //     ...prev,
  //     [name]: value
  //   }));
  // };

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLInputElement> | SelectChangeEvent
  ) => {
    const { name, value } = e.target;
    setFilters((prev) => ({ ...prev, [name]: value }));
  };
  
  const handleDateChange = (name: 'fromDate' | 'toDate', value: Dayjs | null) => {
    setFilters((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleResetFilters = () => {
    setFilters({
      fromDate: null,
      toDate: null,
      type: '',
    });
  };
  

  const getFilteredData = (): CombinedData => {
    let data: CombinedData = activeTab === "leaves" ? mockLeaveData : mockPermissionData;
  
    if (filters.fromDate) {
      data = data.filter(item => {
        const date = "startDate" in item ? item.startDate : item.date;
        return new Date(date) >= filters.fromDate!.toDate();  // ✅ Convert Dayjs to Date
      });
    }
  
    if (filters.toDate) {
      data = data.filter(item => {
        const date = "endDate" in item ? item.endDate : item.date;
        return new Date(date) <= filters.toDate!.toDate();  // ✅ Convert Dayjs to Date
      });
    }
  
    if (filters.type) {
      data = data.filter(item => item.type.toLowerCase().includes(filters.type.toLowerCase()));
    }
  
    return data;
  };
  

  return (
    <div className="min-h-screen bg-gray-50 w-full">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <img
                src="https://images.unsplash.com/photo-1599305445671-ac291c95aaa9"
                alt="Company Logo"
                className="h-8 w-auto"
              />
              <h1 className="ml-3 text-xl font-semibold text-gray-900">Leave Management</h1>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-full mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <LeaveBalanceCard title="Annual Leaves" used={2} total={20} color="bg-blue-500" />
          <LeaveBalanceCard title="Sick Leaves" used={1} total={10} color="bg-green-500" />
          <LeaveBalanceCard title="Emergency Leaves" used={0} total={5} color="bg-yellow-500" />
          <LeaveBalanceCard title="Permissions" used={3} total={5} color="bg-purple-500" />
        </div>

        <div className="mb-6">
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex space-x-8">
              <button
                onClick={() => setActiveTab("leaves")}
                className={activeTab === "leaves" ? "border-blue-500 text-blue-600 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"}
              >
                Leaves
              </button>
              <button
                onClick={() => setActiveTab("permissions")}
                className={activeTab === "permissions" ? "border-blue-500 text-blue-600 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"}
              >
                Permissions
              </button>
            </nav>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="p-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-medium text-gray-900">
                {activeTab === "leaves" ? "Leave Applications" : "Permission Requests"}
              </h2>
              <button 
                onClick={() => setShowFilters(!showFilters)}
                className="flex items-center px-3 py-2  rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
              >
                <FilterIcon color="#aaa" className="mr-2 w-10 h-10 border-none "  />
                
              </button>
            </div>

            {showFilters && (
              <LocalizationProvider dateAdapter={AdapterDayjs}  >

                  <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4 border ">
                    <DatePicker
                      label="From Date"
                      value={filters.fromDate}
                      onChange={(newValue) => handleDateChange('fromDate', newValue)}
                      slotProps={{
                        textField: {
                          fullWidth: true,
                          size: 'small',
                        },
                      }}
                    />

                    <DatePicker
                      label="To Date"
                      value={filters.toDate}
                      onChange={(newValue) => handleDateChange('toDate', newValue)}
                      slotProps={{
                        textField: {
                          fullWidth: true,
                          size: 'small',
                        },
                      }}
                    />

                    <FormControl fullWidth size="small">
                      <InputLabel>Type</InputLabel>
                      <Select
                        label="Type"
                        name="type"
                        value={filters.type}
                        onChange={handleFilterChange}
                      >
                        <MenuItem value="">All</MenuItem>
                        <MenuItem value="Sick">Sick</MenuItem>
                        <MenuItem value="Vacation">Vacation</MenuItem>
                        <MenuItem value="Permission">Permission</MenuItem>
                      </Select>
                    </FormControl>
                  </div>
                  
                  {/* Reset Filter Button */}
                  <div className="mt-4 flex justify-end">
                    <button
                      onClick={handleResetFilters}
                      className="px-4 py-2 text-sm font-medium text-white bg-red-500 hover:bg-red-700 rounded-md"
                    >
                      Reset Filters
                    </button>
                  </div>
              
              </LocalizationProvider>
            )}



          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  {activeTab === "leaves" ? (
                    <>
                      <th className={thClass}>Type</th>
                      <th className={thClass}>Start Date</th>
                      <th className={thClass}>End Date</th>
                      <th className={thClass}>Duration</th>
                      <th className={thClass}>Status</th>
                    </>
                  ) : (
                    <>
                      <th className={thClass}>Type</th>
                      <th className={thClass}>Date</th>
                      <th className={thClass}>Time</th>
                      <th className={thClass}>Duration</th>
                      <th className={thClass}>Status</th>
                    </>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {activeTab === "leaves"
                    ? (getFilteredData() as Leave[]).map((leave) => (
                        <tr key={leave.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{leave.type}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {format(new Date(leave.startDate), "dd MMM yyyy")}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {format(new Date(leave.endDate), "dd MMM yyyy")}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{leave.duration}</td>
                        <td className="px-6 py-4 whitespace-nowrap">
                            <StatusBadge status={leave.status} />
                        </td>
                        </tr>
                    ))
                    : (getFilteredData() as Permission[]).map((permission) => (
                        <tr key={permission.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{permission.type}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {format(new Date(permission.date), "dd MMM yyyy")}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{permission.time}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{permission.duration}</td>
                        <td className="px-6 py-4 whitespace-nowrap">
                            <StatusBadge status={permission.status} />
                        </td>
                        </tr>
                    ))}
                </tbody>

            </table>
          </div>

          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="flex-1 flex justify-between sm:hidden">
              <button className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                Previous
              </button>
              <button className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                Next
              </button>
            </div>
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Showing <span className="font-medium">1</span> to <span className="font-medium">10</span> of{" "}
                  <span className="font-medium">20</span> results
                </p>
              </div>
              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                  <button className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                    <span className="sr-only">Previous</span>
                    <LeftArrowIcon className="h-5 w-5" />
                  </button>
                  <button className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                    <span className="sr-only">Next</span>
                    <RightArrowIcon className="h-5 w-5" />
                  </button>
                </nav>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default LeaveManagement;