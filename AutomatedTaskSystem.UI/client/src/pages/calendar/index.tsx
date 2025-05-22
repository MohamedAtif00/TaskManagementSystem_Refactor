import { useState, useEffect } from "react";
import { format, parse } from "date-fns";
// import { Link } from "react-router-dom";
import LEAVE, { IGetAllLeavesRequest, IGetLeaveRequestForCalander, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import PERMISSION, {  IPermission, PermissionRequestStatus, PermissionType } from "../../lib/API/Permission";
import ExportButton from "../../components/button/ExportButton";
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";

const Calendar = () => {
  // State management
  const [activeTab, setActiveTab] = useState("vacancy");
  const [vacancies, setVacancies] = useState<IGetLeaveRequestForCalander[]>([]);
  const [filteredVacancies, setFilteredVacancies] = useState<IGetLeaveRequestForCalander[]>([]);
  const [permissions, setPermissions] = useState<IPermission[]>([]);
  const [filteredPermissions, setFilteredPermissions] = useState<IPermission[]>([]);
  const [loading, setLoading] = useState({
    vacancies: true,
    permissions: true
  });
  const auth = useAppSelector((s) => s.authSlice);

  
  // Filter states
  const [vacancyFilters, setVacancyFilters] = useState({
    fromDate: "",
    toDate: "",
    status: "all",
    type: "all"
  });

  const [permissionFilters, setPermissionFilters] = useState({
    date: "",
    type: "all",
    status: "all"
  });
  

  useEffect(() => {
    let filtered = [...vacancies];

    if (vacancyFilters.fromDate) {
      filtered = filtered.filter(v => new Date(v.dateCreated) >= new Date(vacancyFilters.fromDate));
    }

    if (vacancyFilters.toDate) {
      filtered = filtered.filter(v => new Date(v.dateCreated) <= new Date(vacancyFilters.toDate));
    }

    if (vacancyFilters.status !== 'all') {
      filtered = filtered.filter(v => v.status === vacancyFilters.status);
    }

    if (vacancyFilters.type !== 'all') {
      filtered = filtered.filter(v => v.type === vacancyFilters.type);
    }

    setFilteredVacancies(filtered);
}, [vacancyFilters, vacancies]);



  const [searchQuery, setSearchQuery] = useState("");
  
  // Date format helper
  const formatDate = (dateStr:string) => {
    if (!dateStr) return "";
    try {
      const date = new Date(dateStr);
      return format(date, "dd/MM/yyyy");
    } catch (e) {
      console.error("Date formatting error:", e);
      return dateStr;
    }
  };

  // Fetch vacancies data
  const fetchVacancies = async () => {
    setLoading(prev => ({ ...prev, vacancies: true }));
    try {
      // Actual API call to get leaves
      const response = await LEAVE.GET_ALL_DB({userId:auth.id,role:auth.role});
      debugger
      if (response && response.data ) {
        console.log(response);
        
        const transformedData = response.data.map((item:IGetAllLeavesRequest) => ({
          id: item.id,
          username: item.user?.name,
          requestDate: formatDate(item.dateCreated),
          type: item.type || "Annual",
          status: item.status || "Pending",
          duration: item.duration,
          dateCreated: item.dateCreated,
          user: item.user,
          startDate: item.startDate,
          endDate: item.endDate,
          reason: item.reason,
          vacancyDate: formatDate(item.startDate)
        }));

        setVacancies(transformedData);
        setFilteredVacancies(transformedData);
      } else {
        console.error("Invalid response format from LEAVE.GET_ALL_DB");
        setVacancies([]);
        setFilteredVacancies([]);
      }
    } catch (error) {
      console.error("Error fetching vacancies:", error);
      setVacancies([]);
      setFilteredVacancies([]);
    } finally {
      setLoading(prev => ({ ...prev, vacancies: false }));
    }
  };

  // Fetch permissions data
  const fetchPermissions = async () => {
    setLoading(prev => ({ ...prev, permissions: true }));
    try {
      // Actual API call to get permissions
      const response = await PERMISSION.GET_ALL();
      console.log(response,"response");
      if (response && response.data && !response.error) {
        const transformedData = response.data.map((item:IPermission) => ({
          id: item.id,
          username: item.user.name,
          dateCreated: formatDate(item.dateCreated),
          type: item.type,
          status: item.status,
          permissionDate: formatDate(item.permissionDate),
          from:item.fromTime,
          to:item.toTime,
          user: item.user,
          reason: item.reason
        }));

        setPermissions(transformedData);
        setFilteredPermissions(transformedData);
        console.log(permissions);
        console.log(transformedData);
        
        
      } else {
        console.error("Invalid response format from PERMISSION.GET_ALL");
        setPermissions([]);
        setFilteredPermissions([]);
      }
    } catch (error) {
      console.error("Error fetching permissions:", error);
      setPermissions([]);
      setFilteredPermissions([]);
    } finally {
      setLoading(prev => ({ ...prev, permissions: false }));
    }
  };

  // Initial data loading
  useEffect(() => {
    fetchVacancies();
    fetchPermissions();
  }, []);

  // Handle tab change
  const handleTabChange = (tab:"vacancy"|"permission") => {
    setActiveTab(tab);
  };

  // Filter vacancies
  const filterVacancies = () => {
    let filtered = [...vacancies];
    
    // Filter by date range
    if (vacancyFilters.fromDate && vacancyFilters.toDate) {
      const fromDate = new Date(vacancyFilters.fromDate);
      const toDate = new Date(vacancyFilters.toDate);
      
      filtered = filtered.filter(vacancy => {
        const startDate = new Date(vacancy.startDate);
        return startDate >= fromDate && startDate <= toDate;
      });
    }
    
    // Filter by status
    if (vacancyFilters.status !== "all") {
      filtered = filtered.filter(vacancy => 
        vacancy.status.toLowerCase() === vacancyFilters.status.toLowerCase()
      );
    }
    
    // Filter by type
    if (vacancyFilters.type !== "all") {
      filtered = filtered.filter(vacancy => 
        vacancy.type === vacancyFilters.type
      );
    }
    
    setFilteredVacancies(filtered);
  };

  // Filter permissions
  const filterPermissions = () => {
    debugger
    let filtered = [...permissions];
    
    // Filter by date
    if (permissionFilters.date) {
      const filterDate = new Date(permissionFilters.date).setHours(0, 0, 0, 0);
      
      filtered = filtered.filter(permission => {
        const permDate = new Date(permission.dateCreated).setHours(0, 0, 0, 0);
        return permDate === filterDate;
      });
    }
    
    // Filter by status
    if (permissionFilters.status !== "all") {
      filtered = filtered.filter(permission => 
        permission.status?.toLowerCase() === permissionFilters.status.toLowerCase()
      );
    }
    
    // Filter by type
    if (permissionFilters.type !== "all") {
      filtered = filtered.filter(permission => 
        permission.type.toLowerCase() === permissionFilters.type.toLowerCase()
      );
    }
    
    // Filter by search query
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase().trim();
      filtered = filtered.filter(permission => 
        permission.user.name.toLowerCase().includes(query) || 
        (permission.user.id && permission.user.id.toString().includes(query))
      );
    }
    
    setFilteredPermissions(filtered);
  };

  // Update the permission filters useEffect
  useEffect(() => {
    let filtered = [...permissions];

    // Filter by date
    if (permissionFilters.date) {
      const filterDate = format(new Date(permissionFilters.date), 'dd/MM/yyyy');
      filtered = filtered.filter(permission => 
        permission.permissionDate === filterDate
      );
    }

    // Filter by status (case insensitive)
    if (permissionFilters.status !== 'all') {
      filtered = filtered.filter(permission => 
        permission.status?.toLowerCase() === permissionFilters.status.toLowerCase()
      );
    }

    // Filter by type (case insensitive)
    if (permissionFilters.type !== 'all') {
      filtered = filtered.filter(permission => 
        permission.type.toLowerCase() === permissionFilters.type.toLowerCase()
      );
    }

    // Filter by search query
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase().trim();
      filtered = filtered.filter(permission => 
        permission.user.name.toLowerCase().includes(query) || 
        (permission.user.id && permission.user.id.toString().includes(query))
      );
    }

    setFilteredPermissions(filtered);
  }, [permissionFilters, permissions, searchQuery]);

// Remove the handleApplyFilter function since we're using useEffect now
// Remove the filterPermissions function since it's replaced by the useEffect

  // Handle apply filter
  const handleApplyFilter = () => {
    if (activeTab === "vacancy") {
      filterVacancies();
    } else {
      filterPermissions();
    }
  };

  // Status color helper
  const getStatusColor = (status:LeaveRequestStatus | null) => {
    switch (status?.toLowerCase()) {
      case 'approved':
      case 'accepted':
        return 'text-green-500';
      case 'rejected':
        return 'text-red-500';
      case 'pending':
        return 'text-blue-500';
      default:
        return '';
    }
  };

  // Render the Vacancy tab content
  const renderVacancyTab = () => (
    <>
    <div className="flex flex-col justify-center items-center align-middle">
      <div className="mb-6 w-10/12">
        <div className="grid grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Vacancy From</label>
            <input
              type="date"
              className="w-full p-2 border border-gray-300 rounded-md"
              value={vacancyFilters.fromDate}
              onChange={(e) => setVacancyFilters({...vacancyFilters, fromDate: e.target.value})}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Vacancy To</label>
            <input
              type="date"
              className="w-full p-2 border border-gray-300 rounded-md"
              value={vacancyFilters.toDate}
              onChange={(e) => setVacancyFilters({...vacancyFilters, toDate: e.target.value})}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <div className="relative">
              <select
                className="w-full p-2 border border-gray-300 rounded-md appearance-none"
                value={vacancyFilters.status}
                onChange={(e) => setVacancyFilters({...vacancyFilters, status: e.target.value})}
              >
                <option value="all">All</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
              <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                <svg className="fill-current h-4 w-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                  <path d="M9.293 12.95l.707.707L15.657 8l-1.414-1.414L10 10.828 5.757 6.586 4.343 8z"/>
                </svg>
              </div>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
            <div className="relative">
              <select
                className="w-full p-2 border border-gray-300 rounded-md appearance-none"
                value={vacancyFilters.type}
                onChange={(e) => setVacancyFilters({...vacancyFilters, type: e.target.value})}
              >
                <option value="all">All</option>
                <option value="Annual">Annual</option>
                <option value="Sick">Sick</option>
                <option value="Emergency">Emergency</option>
              </select>
              <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                <svg className="fill-current h-4 w-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                  <path d="M9.293 12.95l.707.707L15.657 8l-1.414-1.414L10 10.828 5.757 6.586 4.343 8z"/>
                </svg>
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-end mt-4 space-x-2">
          <ExportButton data={filteredVacancies}/>
          {/* <button 
            className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600"
            onClick={handleApplyFilter}
          >
            Apply filter
          </button> */}
        </div>
      </div>

      <div className="shadow-md rounded-lg overflow-hidden w-10/12">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User name</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Request date</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Vacancy type</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Final Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Vacancy date</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {loading.vacancies ? (
              <tr>
                <td colSpan={6} className="px-6 py-4 text-center">Loading...</td>
              </tr>
            ) : filteredVacancies.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-4 text-center">No vacancies found</td>
              </tr>
            ) : (
              filteredVacancies.map((vacancy) => (
                <tr key={vacancy.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">{vacancy.user?.name}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{format(new Date(vacancy.dateCreated), "dd MMM yyyy")}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{vacancy.type}</td>
                  <td className={`px-6 py-4 whitespace-nowrap ${getStatusColor(vacancy.status)}`}>
                    {vacancy.status}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">{format(new Date(vacancy.startDate), "dd MMM yyyy")}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                     <Link href={`/calendar/vacancy/${vacancy.id}`} className="text-gray-600 hover:text-gray-900">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                        <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                        <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                      </svg>
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
    </>
  );


  // Render the Permissions tab content
  const renderPermissionsTab = () => (
    <>
     <div className="flex flex-col justify-center items-center align-middle">
        <div className="mb-6 w-10/12">
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">Search</label>
            <input
              type="text"
              placeholder="Search by member name or ID"
              className="w-full p-2 border border-gray-300 rounded-md"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
          
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Permission date</label>
              <input
                type="date"
                className="w-full p-2 border border-gray-300 rounded-md"
                value={permissionFilters.date}
                onChange={(e) => setPermissionFilters({...permissionFilters, date: e.target.value})}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
              <div className="relative">
                <select
                  className="w-full p-2 border border-gray-300 rounded-md appearance-none"
                  value={permissionFilters.type}
                  onChange={(e) => setPermissionFilters({...permissionFilters, type: e.target.value})}
                >
                  <option value="all">All</option>
                  <option value="EarlyDeparture">Early Departure</option>
                  <option value="LateArrival">Late Arrival</option>
                  <option value="WorkAssignment">Work Assignment</option>
                  <option value="Departure">Departure</option>
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                  <svg className="fill-current h-4 w-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                    <path d="M9.293 12.95l.707.707L15.657 8l-1.414-1.414L10 10.828 5.757 6.586 4.343 8z"/>
                  </svg>
                </div>
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <div className="relative">
                <select
                  className="w-full p-2 border border-gray-300 rounded-md appearance-none"
                  value={permissionFilters.status}
                  onChange={(e) => setPermissionFilters({...permissionFilters, status: e.target.value})}
                >
                  <option value="all">All</option>
                  <option value="pending">Pending</option>
                  <option value="approved">Approved</option>
                  <option value="rejected">Rejected</option>
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-700">
                  <svg className="fill-current h-4 w-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                    <path d="M9.293 12.95l.707.707L15.657 8l-1.414-1.414L10 10.828 5.757 6.586 4.343 8z"/>
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <div className="flex justify-end mt-4 space-x-2">
            <ExportButton data={filteredPermissions}/>
            <button 
              className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600"
              onClick={handleApplyFilter}
            >
              Apply filter
            </button>
          </div>
        </div>

        <div className="shadow-md rounded-lg overflow-hidden w-10/12">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Request date</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Permission Type</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Final Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Permission date</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {loading.permissions ? (
                <tr>
                  <td colSpan={6} className="px-6 py-4 text-center">Loading...</td>
                </tr>
              ) : filteredPermissions.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-4 text-center">No permissions found</td>
                </tr>
              ) : (
                filteredPermissions.map((permission) => (
                  <tr key={permission.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">{permission.user.name}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{permission.dateCreated}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{permission.type}</td>
                    <td className={`px-6 py-4 whitespace-nowrap ${getStatusColor(permission?.status || null)}`}>{permission.status}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{permission.permissionDate}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <Link href={`/calendar/permission/${permission.id}`} className="text-gray-600 hover:text-gray-900">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                        <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                        <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                      </svg>
                    </Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );

  return (
    <div className="w-full">
      <div className="flex items-center mb-6">
        <div className="text-xl font-semibold flex items-center">
          <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          Calendar
        </div>
      </div>

      <div className="border-t border-gray-200 my-4"></div>

      <div className="mb-4">
        <div className="flex border-b border-gray-200">
          <button
            className={`px-4 py-2 font-medium focus:outline-none ${
              activeTab === "vacancy"
                ? "border-b-2 border-blue-500 text-blue-500"
                : "text-gray-500 hover:text-gray-700"
            }`}
            onClick={() => handleTabChange("vacancy")}
          >
            Vacancy
          </button>
          <button
            className={`px-4 py-2 font-medium focus:outline-none ${
              activeTab === "permission"
                ? "border-b-2 border-blue-500 text-blue-500"
                : "text-gray-500 hover:text-gray-700"
            }`}
            onClick={() => handleTabChange("permission")}
          >
            Permissions
          </button>
        </div>
      </div>

      {activeTab === "vacancy" ? renderVacancyTab() : renderPermissionsTab()}
    </div>
  );
};

export default Calendar;