import React, { useState, useEffect, useCallback } from "react";
import { format } from "date-fns";

import { PlusIcon } from "@heroicons/react/24/outline";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import { IGetLeaveRequest, LeaveRequestStatus } from "../../lib/API/Leave";
import Permission, { ICreatePermission, IPermission, PermissionType } from "../../lib/API/Permission";

// Added: New LeaveModal component
const LeaveModal = ({ isOpen, onClose, onSuccess }:{isOpen:boolean, onClose:()=>void, onSuccess:()=>void}) => {
  const [formData, setFormData] = useState({
    type: '',
    startDate: '',
    endDate: '',
    reason: '',
    noteToManager:''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const auth = useAppSelector((e) => e.authSlice);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation()
    if (!auth?.id) return;

    // Validate dates
    if (formData.startDate && formData.endDate && new Date(formData.endDate) < new Date(formData.startDate)) {
      setError('تاريخ النهاية لا يمكن أن يكون قبل تاريخ البداية');
      return;
    }

    setError('');
    setIsSubmitting(true);

    try {
      const leave = {
        userId: auth.id,
        type: formData.type as "Annual" | "Sick" | "Emergency",
        startDate: formData.startDate,
        endDate: formData.endDate,
        reason: formData.reason,
        noteForManager:formData.noteToManager
      };

      const response = await API.LEAVE.CREATE(leave);
      if (response) {
        onSuccess();
        onClose();
        setFormData({ type: '', startDate: '', endDate: '', reason: '' ,noteToManager:''});
      } else {
        setError('Failed to create leave request. Please try again.');
      }
    } catch (error) {
      console.error('Error creating leave request:', error);
      setError('An error occurred. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStartDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newStartDate = e.target.value;
    setFormData(prev => ({
      ...prev,
      startDate: newStartDate,
      // Reset endDate if it's now before the new start date
      endDate: prev.endDate && new Date(prev.endDate) < new Date(newStartDate) ? '' : prev.endDate
    }));
  };

  const handleEndDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newEndDate = e.target.value;
    setFormData(prev => ({
      ...prev,
      endDate: newEndDate
    }));
  };

  if (!isOpen) return null;

  // Calculate min date for end date input
  const minEndDate = formData.startDate || '';

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">إنشاء طلب إجازة</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500"
          >
            {/* <FiX size={20} /> */}
          </button>
        </div>
        {error && (
          <div className="mb-4 p-2 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">نوع الإجازة</label>
            <select 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: e.target.value })}
              required
            >
              <option value="">اختر نوع الإجازة</option>
              <option value="Annual">إجازة سنوية</option>
              <option value="Sick">إجازة مرضية</option>
              <option value="Emergency">إجازة طارئة</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ البداية</label>
            <input 
              type="date" 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.startDate}
              onChange={handleStartDateChange}
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ النهاية</label>
            <input 
              type="date" 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.endDate}
              onChange={handleEndDateChange}
              min={minEndDate}
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">السبب</label>
            <textarea 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              rows={3}
              value={formData.reason}
              onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
              required
            ></textarea>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">ملاحظ للمدير</label>
            <textarea 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              rows={3}
              value={formData.noteToManager}
              onChange={(e) => setFormData({ ...formData, noteToManager: e.target.value })}
              required
            ></textarea>
          </div>
          <div className="flex justify-around space-x-3">
            <button
              type="submit"
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-black hover:bg-gray-600 disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'جاري الإرسال...' : 'إرسال'}
            </button>
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
              disabled={isSubmitting}
            >
              إلغاء
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Added: New PermissionModal component
const PermissionModal = ({ isOpen, onClose, onSuccess }: {isOpen: boolean, onClose: () => void, onSuccess: () => void}) => {
  const [formData, setFormData] = useState({
    type: '' as PermissionType,
    date: '',
    from: '',
    to: '',
    reason: '',
    PermissionDate:''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const auth = useAppSelector((e) => e.authSlice);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!auth?.id) return;

    setError('');
    setIsSubmitting(true);

    try {
    const payload: ICreatePermission = {
        userId: auth.id,
        type: formData.type,
        date: formData.date,
        from: formData.from, // ✅ Correct casing
        to: formData.to,     // ✅ Correct casing
        reason: formData.reason,
        PermissionDate:formData.PermissionDate
        };



      const response = await Permission.CREATE(payload);
      debugger
      if (response) {
        onSuccess();
        onClose();
        setFormData({ 
          type: '' as PermissionType,
          date: '',
          from: '',
          to: '',
          reason: '' ,
          PermissionDate:''
        });
      } else {
        setError('فشل إنشاء طلب الإذن. يرجى المحاولة مرة أخرى.');
      }
    } catch (error) {
      console.error('Error creating permission request:', error);
      setError('حدث خطأ. يرجى المحاولة مرة أخرى.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">نموذج طلب إذن</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500"
          >
            {/* <FiX size={20} /> */}
          </button>
        </div>
        {error && (
          <div className="mb-4 p-2 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">نوع الإذن</label>
            <select 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: e.target.value as PermissionType })}
              required
            >
              <option value="">اختر نوع الإذن</option>
              <option value={PermissionType.WorkAssignment}>مأمورية عمل</option>
              <option value={PermissionType.EarlyDeparture}>انصراف مبكر</option>
              <option value={PermissionType.LateArrival}>تأخير</option>
              <option value={PermissionType.Departure}>انصراف</option>
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ اليوم</label>
            <input 
              type="date" 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.PermissionDate}
              onChange={(e) => setFormData({ ...formData, PermissionDate: e.target.value })}
              required
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">من الساعة</label>
              <input 
                type="time" 
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                value={formData.from}
                onChange={(e) => setFormData({ ...formData, from: e.target.value })}
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">إلى الساعة</label>
              <input 
                type="time" 
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                value={formData.to}
                onChange={(e) => setFormData({ ...formData, to: e.target.value })}
                required
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">سبب الإذن / المأمورية</label>
            <textarea 
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              rows={3}
              placeholder="اكتب السبب"
              value={formData.reason}
              onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
              required
            ></textarea>
          </div>

          <div className="flex justify-around space-x-3">
            <button
              type="submit"
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-black hover:bg-blue-700 disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'جاري الإرسال...' : 'ارسال'}
            </button>
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white  hover:bg-gray-50"
              disabled={isSubmitting}
            >
              الغاء
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const LeaveManagement = () => {
  const [isLeaveModalOpen, setIsLeaveModalOpen] = useState(false);
  const [isPermissionModalOpen, setIsPermissionModalOpen] = useState(false);
  const auth = useAppSelector((e) => e.authSlice);
  const [activeTab, setActiveTab] = useState("leaves");
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState({
    status: '',
    startDate: '',
    endDate: '',
    type: ''
  });
  const [allLeaves, setAllLeaves] = useState<IGetLeaveRequest[]>([]);
  const [allPermissions, setAllPermissions] = useState<IPermission[]>([]);
  const [filteredLeaves, setFilteredLeaves] = useState<IGetLeaveRequest[]>([]);
  const [filteredPermissions, setFilteredPermissions] = useState<IPermission[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [user,setUser] = useState<IUser|null>(null);

 
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
        {title != 'Sick' ?
        <span className="ml-2 text-sm text-gray-600">{used}/{total}</span>
        : <span className="ml-2 text-sm text-gray-600">{used}</span>
        }
      </div>
    </div>
  );

  const StatusBadge = ({ status }: { status: LeaveRequestStatus }) => {
    const getStatusColor = (status: LeaveRequestStatus) => {
      switch (status) {
        case "Approved":
          return "bg-green-100 text-green-800";
        case "Pending":
          return "bg-yellow-100 text-yellow-800";
        case "Rejected":
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

  useEffect(()=>{
    const fetchData = async ()=>{
      if(!auth.id) return;

      setIsLoading(true);
      try{
        const user = await API.LEAVE.GET_USER_INFO(auth.id);
        debugger
        if (user && 'data' in user) {
          setUser(user.data);
          console.log(allLeaves);
        }
      }catch(error){
        console.error('Error fetching data:', error);
      }finally{
        setIsLoading(false)
      }
    }

    fetchData()
  },[auth.id])

  // Function to fetch leaves and permissions
  useEffect(() => {
    
    const fetchData = async () => {
  if (!auth?.id) return;

  setIsLoading(true);
    try {
      // Fetch leaves
      const leavesResponse = await API.LEAVE.GET_ALL_BY_USER(auth.id);
      debugger
      if (leavesResponse && 'data' in leavesResponse) {
        setAllLeaves(leavesResponse.data);
        setFilteredLeaves(leavesResponse.data);
        console.log(allLeaves);
      }

   
      // Fetch permissions with better error handling
      const permissionsResponse = await Permission.GET_ALL_BY_USER(auth.id);
      console.log('Raw permissions response:', permissionsResponse);
      debugger  
      if (permissionsResponse && !permissionsResponse.error) {
        // Ensure data exists and is an array
        if (Array.isArray(permissionsResponse.data)) {
          setAllPermissions(permissionsResponse.data);
          setFilteredPermissions(permissionsResponse.data);
          console.log(allPermissions);
          
        } else {
          console.error('Permissions data is not an array:', permissionsResponse.data);
        }
      } else {
        console.error('Error in permissions response:', permissionsResponse);
      }
    } catch (error) {
      console.error('Error fetching data:', error);
    }
     finally {
      setIsLoading(false);
    }

  };

    fetchData();
  }, [auth?.id]); // Only re-run if user ID changes

  // Apply filters whenever filters state changes
  useEffect(() => {
    // Filter leaves
    const filterLeaves = () => {
        
     let filtered = Array.isArray(allLeaves) ? [...allLeaves] : [];


      if (filters.status) {
        filtered = filtered.filter(leave => leave.status === filters.status);
      }

      if (filters.type) {
        filtered = filtered.filter(leave => leave.type === filters.type);
      }

      if (filters.startDate) {
        filtered = filtered.filter(leave => {
          const leaveStart = new Date(leave.startDate);
          const filterStart = new Date(filters.startDate);
          return leaveStart >= filterStart;
        });
      }

      if (filters.endDate) {
        filtered = filtered.filter(leave => {
          const leaveEnd = new Date(leave.endDate);
          const filterEnd = new Date(filters.endDate);
          return leaveEnd <= filterEnd;
        });
      }

      setFilteredLeaves(filtered);
    };

    // Filter permissions
    const filterPermissions = () => {
      // let filtered = [...allPermissions];
       let filtered = Array.isArray(allPermissions) ? [...allPermissions] : [];

      if (filters.status) {
        filtered = filtered.filter(permission => permission.status === filters.status);
      }

      if (filters.type) {
        filtered = filtered.filter(permission => permission.type === filters.type);
      }

      if (filters.startDate) {
        filtered = filtered.filter(permission => {
          const permissionDate = new Date(permission.permissionDate);
          const filterDate = new Date(filters.startDate);
          return permissionDate.toDateString() === filterDate.toDateString();
        });
      }

      setFilteredPermissions(filtered);
    };

    // Apply appropriate filter based on active tab
    if (activeTab === "leaves") {
      filterLeaves();
    } else {
      filterPermissions();
    }
  }, [filters, allLeaves, allPermissions, activeTab]);

  // Add refresh function
  const refreshData = useCallback(async () => {
    if (!auth?.id) return;
    setIsLoading(true);
    try {
      const leavesResponse = await API.LEAVE.GET_ALL_BY_USER(auth.id);
      if (leavesResponse && 'data' in leavesResponse) {
        setAllLeaves(leavesResponse.data);
        setFilteredLeaves(leavesResponse.data);
      }

      const permissionsResponse = await Permission.GET_ALL_BY_USER(auth.id);
      console.log(permissionsResponse);
      
      if (permissionsResponse) {
        setAllPermissions(permissionsResponse);
        setFilteredPermissions(permissionsResponse);
      }
    } catch (error) {
      console.error('Error refreshing data:', error);
    } finally {
      setIsLoading(false);
    }
  }, [auth?.id]);

  return (
    <div className="min-h-screen bg-gray-50 w-full">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <h1 className="ml-3 text-xl font-semibold text-gray-900">My Leave</h1>
            </div>
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="md:hidden"
            >
              {/* {isMobileMenuOpen ? <FiX size={24} /> : <FiMenu size={24} />} */}
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <LeaveBalanceCard title="Annual" used={user?.vacation?.annual??0} total={user?.vacation?.annual_MAX??0} color="bg-blue-500" />
          <LeaveBalanceCard title="Sick" used={user?.vacation?.sick??0} total={100} color="bg-green-500" />
          <LeaveBalanceCard title="Emergency" used={user?.vacation?.emergency??0} total={user?.vacation?.emergency_MAX??0} color="bg-yellow-500" />
          <LeaveBalanceCard title="Permissions" used={user?.permission??0} total={user?.permission_MAX??0} color="bg-purple-500" />
        </div>

        <div className="mb-6">
          <div className="border-b border-gray-200">
            <div className="flex justify-between items-center">
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
              {/* Updated: Conditional button rendering based on active tab */}
              {activeTab === "leaves" ? (
                <button
                  onClick={() => setIsLeaveModalOpen(true)}
                  className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                >
                  <PlusIcon  className="w-10 h-10"/>
                  New Leave Request
                </button>
              ) : (
                <button
                  onClick={() => setIsPermissionModalOpen(true)}
                  className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                >
                  <PlusIcon className="w-10 h-10"/>
                  New Permission Request
                </button>
              )}
            </div>
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
                className="flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
              >
                {/* <FiFilter className="mr-2" /> */}
                Filter
              </button>
            </div>

            {/* Filter Section */}
            {showFilters && (
              <div className="mt-4 flex flex-wrap gap-4 items-end">
                {/* Status Filter */}
                <div className="flex-1 min-w-[200px]">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select 
                    className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    value={filters.status}
                    onChange={(e) => setFilters({...filters, status: e.target.value})}
                  >
                    <option value="">All</option>
                    <option value="Approved">Approved</option>
                    <option value="Pending">Pending</option>
                    <option value="Rejected">Rejected</option>
                  </select>
                </div>

                {/* Type Filter */}
                <div className="flex-1 min-w-[200px]">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                  <select 
                    className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    value={filters.type}
                    onChange={(e) => setFilters({...filters, type: e.target.value})}
                  >
                    <option value="">All</option>
                    {activeTab === "leaves" ? (
                      <>
                        <option value="Annual Leave">Annual Leave</option>
                        <option value="Sick Leave">Sick Leave</option>
                        <option value="Emergency Leave">Emergency Leave</option>
                      </>
                    ) : (
                      <>
                        <option value="Morning">Morning Permission</option>
                        <option value="Evening">Evening Permission</option>
                      </>
                    )}
                  </select>
                </div>

                {/* Date Range Filters */}
                {activeTab === "leaves" ? (
                  <>
                    <div className="flex-1 min-w-[200px]">
                      <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                      <input 
                        type="date" 
                        className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                        value={filters.startDate}
                        onChange={(e) => setFilters({...filters, startDate: e.target.value})}
                      />
                    </div>
                    <div className="flex-1 min-w-[200px]">
                      <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                      <input 
                        type="date" 
                        className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                        value={filters.endDate}
                        onChange={(e) => setFilters({...filters, endDate: e.target.value})}
                      />
                    </div>
                  </>
                ) : (
                  <div className="flex-1 min-w-[200px]">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Date</label>
                    <input 
                      type="date" 
                      className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                      value={filters.startDate}
                      onChange={(e) => setFilters({...filters, startDate: e.target.value})}
                    />
                  </div>
                )}

                {/* Clear Filters Button */}
                <div className="flex-none">
                  <button
                    onClick={() => setFilters({ status: '', startDate: '', endDate: '', type: '' })}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                  >
                    Clear Filters
                  </button>
                </div>
              </div>
            )}
          </div>

          <div className="overflow-x-auto">
            {isLoading ? (
              <div className="flex justify-center items-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
              </div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    {activeTab === "leaves" ? (
                      <>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Start Date</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">End Date</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Duration</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                      </>
                    ) : (
                      <>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {activeTab === "leaves"
                    ? filteredLeaves?.map((leave) => (
                        <tr key={leave.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{leave.type}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{format(new Date(leave.startDate), "dd MMM yyyy")}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{format(new Date(leave.endDate), "dd MMM yyyy")}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{leave.duration} days</td>
                          {auth &&
                            <td className="px-6 py-4 whitespace-nowrap">
                              <StatusBadge status={leave.status} />
                            </td>
                          }
                        </tr>
                      ))
                    :Array.isArray(filteredPermissions) && filteredPermissions.map((permission) => (
                      <tr key={permission.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {permission.type}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {format(new Date(permission.permissionDate), "dd MMM yyyy")}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {permission.reason}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <StatusBadge status={permission.status} />
                        </td>
                      </tr>
                    ))}

                </tbody>
              </table>
            )}
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
            
          </div>
        </div>
      </main>

      {/* Updated: Added both modals with separate states */}
      <LeaveModal 
        isOpen={isLeaveModalOpen} 
        onClose={() => setIsLeaveModalOpen(false)} 
        onSuccess={refreshData}
      />
      <PermissionModal 
        isOpen={isPermissionModalOpen} 
        onClose={() => setIsPermissionModalOpen(false)}
        onSuccess={refreshData}
      />
    </div>
  );
};

export default LeaveManagement;