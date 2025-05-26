import React, { useState, useEffect, useCallback } from "react";
import { format } from "date-fns";

import { PlusIcon } from "@heroicons/react/24/outline";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import LEAVE, { ICreateLeave, IGetLeaveRequest, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import Permission, { ICreatePermission, IPermission, PermissionType } from "../../lib/API/Permission";
import { FiX } from "react-icons/fi";
import FileUpload from "../../components/pageComponent/leave/fileUpload";
import PERMISSION from "../../lib/API/Permission";
import { toast } from "react-toastify";

type CancelTarget = {
  id: number;
  type: 'leave' | 'permission';
};



const LeaveModal = ({ 
  isOpen, 
  onClose, 
  onSuccess 
}: { 
  isOpen: boolean; 
  onClose: () => void; 
  onSuccess: () => void;
}) => {
  const [formData, setFormData] = useState({
    type: '' as LeaveRequestType,
    startDate: '',
    endDate: '',
    reason: '',
    noteToManager: ''
  });
  
  const [medicalCertificate, setMedicalCertificate] = useState<File | null>(null);
  const [supportingDocuments, setSupportingDocuments] = useState<File[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  
  // Mock auth - replace with your actual auth
  const auth = useAppSelector((e) => e.authSlice);

  const handleMedicalCertificateChange = (file: File | null) => {
    setMedicalCertificate(file);
  };

  const handleSupportingDocumentChange = (file: File | null) => {
    if (file) {
      setSupportingDocuments(prev => [...prev, file]);
    }
  };

  const removeSupportingDocument = (index: number) => {
    setSupportingDocuments(prev => prev.filter((_, i) => i !== index));
  };

  const validateForm = (): boolean => {
    setError('');

    if (!formData.type || !formData.startDate || !formData.endDate || !formData.reason) {
      setError('يرجى ملء جميع الحقول المطلوبة');
      return false;
    }

    if (new Date(formData.endDate) < new Date(formData.startDate)) {
      setError('تاريخ النهاية لا يمكن أن يكون قبل تاريخ البداية');
      return false;
    }

    if (formData.type === 'Sick') {
      const daysDifference = Math.ceil(
        (new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 3600 * 24)
      ) + 1;

      if (daysDifference > 3 && !medicalCertificate) {
        setError('الإجازة المرضية لأكثر من 3 أيام تتطلب شهادة طبية');
        return false;
      }
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!auth?.id) return;
    if (!validateForm()) return;

    setIsSubmitting(true);

    try {
      const leaveData: ICreateLeave = {
        userId: auth.id,
        type: formData.type,
        startDate: formData.startDate,
        endDate: formData.endDate,
        reason: formData.reason,
        noteToManager: formData.noteToManager,
        medicalCertificate: medicalCertificate || undefined,
      };

      const response = await LEAVE.CREATE(leaveData); // returns { data, error, message }
      debugger
      if (response?.error) {
        toast.error(response.message || 'حدث خطأ. يرجى المحاولة مرة أخرى.');
      } else if (response?.data) {
        toast.success('تم إنشاء طلب الإجازة بنجاح.');
        onSuccess();
        onClose();
        resetForm();
      } else {
        toast.warning('فشل في إنشاء طلب الإجازة. يرجى المحاولة مرة أخرى.');
      }
    } catch (error) {
      console.error('Error creating leave request:', error);
      toast.error('حدث خطأ. يرجى المحاولة مرة أخرى.');
    } finally {
      setIsSubmitting(false);
    }
  };


  const resetForm = () => {
    setFormData({ type: '' as LeaveRequestType,  startDate: '', endDate: '', reason: '', noteToManager: '' });
    setMedicalCertificate(null);
    setSupportingDocuments([]);
    setError('');
  };

  const handleStartDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newStartDate = e.target.value;
    setFormData(prev => ({
      ...prev,
      startDate: newStartDate,
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

  const minEndDate = formData.startDate || '';
  const isSickLeave = formData.type === 'Sick';

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-10 mx-auto p-5 border w-full max-w-2xl shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">إنشاء طلب إجازة</h3>
          <button 
            onClick={onClose} 
            className="text-gray-400 hover:text-gray-500"
            disabled={isSubmitting}
          >
            <FiX className="h-6 w-6" />
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              نوع الإجازة <span className="text-red-500">*</span>
            </label>
            <select
              className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.type}
              onChange={(e) => {
                setFormData({ ...formData, type: e.target.value as LeaveRequestType});
                if (e.target.value !== 'Sick') {
                  setMedicalCertificate(null);
                }
              }}
              required
              disabled={isSubmitting}
            >
              <option value="">اختر نوع الإجازة</option>
              <option value="Annual">إجازة سنوية</option>
              <option value="Sick">إجازة مرضية</option>
              <option value="Emergency">إجازة طارئة</option>
            </select>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                تاريخ البداية <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                value={formData.startDate}
                onChange={handleStartDateChange}
                required
                disabled={isSubmitting}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                تاريخ النهاية <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                value={formData.endDate}
                onChange={handleEndDateChange}
                min={minEndDate}
                required
                disabled={isSubmitting}
              />
            </div>
          </div>

          {isSickLeave && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                الشهادة الطبية
                {formData.startDate && formData.endDate && 
                 Math.ceil((new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 3600 * 24)) + 1 > 3 && 
                 <span className="text-red-500"> *</span>
                }
              </label>
              <FileUpload
                onFileChange={handleMedicalCertificateChange}
                accept=".pdf,.jpg,.jpeg,.png"
                maxSize={10 * 1024 * 1024}
                label="اختر الشهادة الطبية"
              />
              {formData.startDate && formData.endDate && 
               Math.ceil((new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 3600 * 24)) + 1 > 3 && (
                <p className="text-xs text-gray-600 mt-1">
                  الشهادة الطبية مطلوبة للإجازات المرضية أكثر من 3 أيام
                </p>
              )}
            </div>
          )}

          {/* <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              مستندات إضافية (اختيارية)
            </label>
            <FileUpload
              onFileChange={handleSupportingDocumentChange}
              accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
              maxSize={5 * 1024 * 1024}
              label="اختر مستند إضافي"
            />
            
            {supportingDocuments.length > 0 && (
              <div className="mt-3 space-y-2">
                <p className="text-sm font-medium text-gray-700">المستندات المرفقة:</p>
                {supportingDocuments.map((doc, index) => (
                  <div key={index} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                    <span className="text-sm text-gray-700">{doc.name}</span>
                    <button
                      type="button"
                      onClick={() => removeSupportingDocument(index)}
                      className="text-red-500 hover:text-red-700"
                      disabled={isSubmitting}
                    >
                      <FiX className="h-4 w-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div> */}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              السبب <span className="text-red-500">*</span>
            </label>
            <textarea
              className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              rows={3}
              value={formData.reason}
              onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
              required
              disabled={isSubmitting}
            ></textarea>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              ملاحظة للمدير
            </label>
            <textarea
              className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              rows={3}
              value={formData.noteToManager}
              onChange={(e) => setFormData({ ...formData, noteToManager: e.target.value })}
              disabled={isSubmitting}
            ></textarea>
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
              disabled={isSubmitting}
            >
              إلغاء
            </button>
            <button
              type="button"
              onClick={handleSubmit}
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-black hover:bg-gray-600 disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'جاري الإرسال...' : 'إرسال'}
            </button>
          </div>
        </div>
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
        from: formData.from,
        to: formData.to,
        reason: formData.reason,
        PermissionDate: formData.PermissionDate,
      };

      const response = await Permission.CREATE(payload); // expected: ResponseService<boolean>

      if (response && !response.error && response.data) {
        toast.success(response.message || 'تم إنشاء طلب الإذن بنجاح');
        onSuccess();
        onClose();
        setFormData({
          type: '' as PermissionType,
          date: '',
          from: '',
          to: '',
          reason: '',
          PermissionDate: ''
        });
      } else {
        const errorMsg = response?.message || 'فشل إنشاء طلب الإذن. يرجى المحاولة مرة أخرى.';
        setError(errorMsg);
        toast.error(errorMsg);
      }
    } catch (error) {
      console.error('Error creating permission request:', error);
      const fallbackError = 'حدث خطأ. يرجى المحاولة مرة أخرى.';
      setError(fallbackError);
      toast.error(fallbackError);
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

  // cancel vars
const [isCancelModalOpen, setIsCancelModalOpen] = useState(false);
const [cancelTarget, setCancelTarget] = useState<CancelTarget | null>(null);


 
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
        case "Cancelled":
          return "bg-red-700 text-white"
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
      }

   
      // Fetch permissions with better error handling
      const permissionsResponse = await Permission.GET_ALL_BY_USER(auth.id);
      debugger  
      if (permissionsResponse && !permissionsResponse.error) {
        // Ensure data exists and is an array
        if (Array.isArray(permissionsResponse.data)) {
          setAllPermissions(permissionsResponse.data);
          setFilteredPermissions(permissionsResponse.data);          
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

 const handleCancelLeave = async (leaveId: number) => {
    try {
      const response = await LEAVE.CANCEL_LEAVE(leaveId);

      if (response.error) {
        toast.error(response.message);
        return;
      }

      toast.success('Leave cancelled successfully.');
      setAllLeaves(prev =>
        prev.map(leave =>
          leave.id === leaveId ? { ...leave, status: 'Cancelled' } : leave
        )
      );
    } catch (error) {
      console.error('Error cancelling leave:', error);
      toast.error('An unexpected error occurred.');
    }
  };


  const handleCancelPermission = async (permissionId: number) => {
    try {
      const response = await PERMISSION.CANCEL_PERMISSION(permissionId);
      debugger
      if (response.error) {
        toast.error(response.message);
        return;
      }

      toast.success('Permission cancelled successfully.');
      setAllPermissions(prev =>
        prev.map(permission =>
          permission.id === permissionId ? { ...permission, status: 'Cancelled' } : permission
        )
      );
    } catch (error) {
      console.error('Error cancelling permission:', error);
      toast.error('An unexpected error occurred.');
    }
  };



  const handleCancel = async () => {
    if (!cancelTarget) return;

    if (cancelTarget.type === 'leave') {
      await handleCancelLeave(cancelTarget.id);
    } else if (cancelTarget.type === 'permission') {
      await handleCancelPermission(cancelTarget.id);
    }

    setIsCancelModalOpen(false);
  };




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
                        <option value="Annual">Annual Leave</option>
                        <option value="Sick">Sick Leave</option>
                        <option value="Emergency">Emergency Leave</option>
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
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
                      </>
                    ) : (
                      <>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
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
                           <td className="px-3 py-4 whitespace-nowrap  text-sm font-medium text-center">
                            {leave.status != 'Cancelled'&& leave.status != 'Rejected' && (
                             <button
                              onClick={() => {
                                setCancelTarget({ id: leave.id, type: 'leave' });
                                setIsCancelModalOpen(true);
                              }}
                            >
                              Cancel Leave
                            </button>
                            )}
                          </td>
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
                         <td className="px-3 py-4 whitespace-nowrap  text-sm font-medium text-center">
                            {permission.status != 'Cancelled'&& permission.status != 'Rejected' && (
                             <button
                              onClick={() => {
                                setCancelTarget({ id: permission.id, type: 'permission' });
                                setIsCancelModalOpen(true);
                              }}
                            >
                              Cancel Permission
                            </button>
                            )}
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
      {isCancelModalOpen && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full flex items-center justify-center">
          <div className="bg-white p-6 rounded-lg shadow-xl max-w-md w-full">
            <h3 className="text-lg font-medium leading-6 text-gray-900 mb-4">Confirm Cancellation</h3>
            <p className="text-sm text-gray-500 mb-6">
              Are you sure you want to cancel this {cancelTarget?.type} request?
            </p>
            <div className="flex justify-end space-x-3">
              <button
                onClick={() => setIsCancelModalOpen(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                No, Keep It
              </button>
              <button
                onClick={handleCancel}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700"
              >
                Yes, Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <PermissionModal 
        isOpen={isPermissionModalOpen} 
        onClose={() => setIsPermissionModalOpen(false)}
        onSuccess={refreshData}
      />
    </div>
  );
};

export default LeaveManagement;