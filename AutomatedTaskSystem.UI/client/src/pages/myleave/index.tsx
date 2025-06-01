import React, { useState, useEffect, useCallback, useMemo } from "react";
import { format } from "date-fns";

import { PlusIcon } from "@heroicons/react/24/outline";
import { useAppSelector } from "../../app/hooks";
import API from "../../lib/API";
import LEAVE, { ICreateLeave, IGetLeaveRequest, LeaveRequestStatus, LeaveRequestType } from "../../lib/API/Leave";
import Permission, { ICreatePermission, IPermission, PermissionRequestStatus, PermissionType } from "../../lib/API/Permission";
import { FiX } from "react-icons/fi";
import FileUpload from "../../components/pageComponent/leave/fileUpload";
// Removed redundant PERMISSION import, Permission is already imported above
// import PERMISSION from "../../lib/API/Permission";
import { toast } from "react-toastify";
import DataTable from "../../components/table/tablePagination"; // Assuming this is your DataTable component

type CancelTarget = {
  id: number;
  type: 'leave' | 'permission';
};

// IUser interface might be defined elsewhere, ensure it's available
interface IUser {
  id: number;
  name: string;
  code?: string;
  group?: { name: string };
  email?: string;
  hrCode?: string;
  phone?: string;
  role?: number;
  vacation?: {
    annual?: number;
    annual_MAX?: number;
    sick?: number;
    emergency?: number;
    emergency_MAX?: number;
  };
  permission?: number;
  permission_MAX?: number;
}


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
  // const [supportingDocuments, setSupportingDocuments] = useState<File[]>([]); // This was unused
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const auth = useAppSelector((e) => e.authSlice);

  const handleMedicalCertificateChange = (file: File | null) => {
    setMedicalCertificate(file);
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
    if (formData.type === LeaveRequestType.Sick) {
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
    if (!auth?.id || !validateForm()) return;
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
      const response = await LEAVE.CREATE(leaveData);
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
    } catch (err) {
      console.error('Error creating leave request:', err);
      toast.error('حدث خطأ. يرجى المحاولة مرة أخرى.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetForm = () => {
    setFormData({ type: '' as LeaveRequestType, startDate: '', endDate: '', reason: '', noteToManager: '' });
    setMedicalCertificate(null);
    // setSupportingDocuments([]);
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

  if (!isOpen) return null;
  const minEndDate = formData.startDate || '';
  const isSickLeave = formData.type === LeaveRequestType.Sick;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-10 mx-auto p-5 border w-full max-w-2xl shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">إنشاء طلب إجازة</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-500" disabled={isSubmitting}>
            <FiX className="h-6 w-6" />
          </button>
        </div>
        {error && <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">{error}</div>}
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">نوع الإجازة <span className="text-red-500">*</span></label>
            <select
              className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
              value={formData.type}
              onChange={(e) => {
                setFormData({ ...formData, type: e.target.value as LeaveRequestType });
                if (e.target.value !== LeaveRequestType.Sick) setMedicalCertificate(null);
              }}
              required disabled={isSubmitting}
            >
              <option value="">اختر نوع الإجازة</option>
              <option value={LeaveRequestType.Annual}>إجازة سنوية</option>
              <option value={LeaveRequestType.Sick}>إجازة مرضية</option>
              <option value={LeaveRequestType.Emergency}>إجازة طارئة</option>
            </select>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ البداية <span className="text-red-500">*</span></label>
              <input type="date" className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.startDate} onChange={handleStartDateChange} required disabled={isSubmitting} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ النهاية <span className="text-red-500">*</span></label>
              <input type="date" className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.endDate} onChange={(e) => setFormData(prev => ({ ...prev, endDate: e.target.value }))} min={minEndDate} required disabled={isSubmitting} />
            </div>
          </div>
          {isSickLeave && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                الشهادة الطبية
                {formData.startDate && formData.endDate && Math.ceil((new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 3600 * 24)) + 1 > 3 && <span className="text-red-500"> *</span>}
              </label>
              <FileUpload onFileChange={handleMedicalCertificateChange} accept=".pdf,.jpg,.jpeg,.png" maxSize={10 * 1024 * 1024} label="اختر الشهادة الطبية" />
              {formData.startDate && formData.endDate && Math.ceil((new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 3600 * 24)) + 1 > 3 && <p className="text-xs text-gray-600 mt-1">الشهادة الطبية مطلوبة للإجازات المرضية أكثر من 3 أيام</p>}
            </div>
          )}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">السبب <span className="text-red-500">*</span></label>
            <textarea className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" rows={3} value={formData.reason} onChange={(e) => setFormData({ ...formData, reason: e.target.value })} required disabled={isSubmitting}></textarea>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظة للمدير</label>
            <textarea className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" rows={3} value={formData.noteToManager} onChange={(e) => setFormData({ ...formData, noteToManager: e.target.value })} disabled={isSubmitting}></textarea>
          </div>
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50" disabled={isSubmitting}>إلغاء</button>
            <button type="button" onClick={handleSubmit} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-black hover:bg-gray-600 disabled:opacity-50" disabled={isSubmitting}>{isSubmitting ? 'جاري الإرسال...' : 'إرسال'}</button>
          </div>
        </div>
      </div>
    </div>
  );
};

const PermissionModal = ({ isOpen, onClose, onSuccess }: { isOpen: boolean, onClose: () => void, onSuccess: () => void }) => {
  const [formData, setFormData] = useState({ type: '' as PermissionType, date: '', from: '', to: '', reason: '', PermissionDate: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const auth = useAppSelector((e) => e.authSlice);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!auth?.id) return;
    setError('');
    setIsSubmitting(true);
    try {
      const payload: ICreatePermission = { userId: auth.id, ...formData };
      const response = await Permission.CREATE(payload);
      if (response && !response.error && response.data) {
        toast.success(response.message || 'تم إنشاء طلب الإذن بنجاح');
        onSuccess();
        onClose();
        setFormData({ type: '' as PermissionType, date: '', from: '', to: '', reason: '', PermissionDate: '' });
      } else {
        const errorMsg = response?.message || 'فشل إنشاء طلب الإذن. يرجى المحاولة مرة أخرى.';
        setError(errorMsg);
        toast.error(errorMsg);
      }
    } catch (err) {
      console.error('Error creating permission request:', err);
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
          <button onClick={onClose} className="text-gray-400 hover:text-gray-500"><FiX size={20} /></button>
        </div>
        {error && <div className="mb-4 p-2 bg-red-100 border border-red-400 text-red-700 rounded">{error}</div>}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">نوع الإذن</label>
            <select className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.type} onChange={(e) => setFormData({ ...formData, type: e.target.value as PermissionType })} required>
              <option value="">اختر نوع الإذن</option>
              <option value={PermissionType.WorkAssignment}>مأمورية عمل</option>
              <option value={PermissionType.EarlyDeparture}>انصراف مبكر</option>
              <option value={PermissionType.LateArrival}>تأخير</option>
              <option value={PermissionType.Departure}>انصراف</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ اليوم</label>
            <input type="date" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.PermissionDate} onChange={(e) => setFormData({ ...formData, PermissionDate: e.target.value })} required />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">من الساعة</label>
              <input type="time" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.from} onChange={(e) => setFormData({ ...formData, from: e.target.value })} required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">إلى الساعة</label>
              <input type="time" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={formData.to} onChange={(e) => setFormData({ ...formData, to: e.target.value })} required />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">سبب الإذن / المأمورية</label>
            <textarea className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" rows={3} placeholder="اكتب السبب" value={formData.reason} onChange={(e) => setFormData({ ...formData, reason: e.target.value })} required></textarea>
          </div>
          <div className="flex justify-around space-x-3">
            <button type="submit" className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-black hover:bg-blue-700 disabled:opacity-50" disabled={isSubmitting}>{isSubmitting ? 'جاري الإرسال...' : 'ارسال'}</button>
            <button type="button" onClick={onClose} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50" disabled={isSubmitting}>الغاء</button>
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
  const [activeTab, setActiveTab] = useState<"leaves" | "permissions">("leaves");
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState({ status: '', startDate: '', endDate: '', type: '' });

  const [allLeaves, setAllLeaves] = useState<IGetLeaveRequest[]>([]);
  const [totalLeavesCount, setTotalLeavesCount] = useState<number>(0);
  const [allPermissions, setAllPermissions] = useState<IPermission[]>([]);
  const [totalPermissionsCount, setTotalPermissionsCount] = useState<number>(0);

  const [isLoading, setIsLoading] = useState(false);
  const [user, setUser] = useState<IUser | null>(null);

  const [isCancelModalOpen, setIsCancelModalOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<CancelTarget | null>(null);

  const [leaveDataTablePage, setLeaveDataTablePage] = useState<number>(1);
  const [leaveDataTableItemsPerPage, setLeaveDataTableItemsPerPage] = useState<number>(10);
  const [leaveDataTableSearchText, setLeaveDataTableSearchText] = useState<string>("");
  // leaveDataTableFilters can be used if DataTable has its own filter inputs via filterConfig
  // const [leaveDataTableFilters, setLeaveDataTableFilters] = useState<Record<string, any>>({});

  const [permissionDataTablePage, setPermissionDataTablePage] = useState<number>(1);
  const [permissionDataTableItemsPerPage, setPermissionDataTableItemsPerPage] = useState<number>(10);
  const [permissionDataTableSearchText, setPermissionDataTableSearchText] = useState<string>("");
  // const [permissionDataTableFilters, setPermissionDataTableFilters] = useState<Record<string, any>>({});


  const LeaveBalanceCard = ({ title, used, total, color }: { title: string, used: number, total: number, color: string }) => (
    <div className="bg-white p-4 rounded-lg shadow-md">
      <h3 className="text-gray-700 font-medium mb-2">{title}</h3>
      <div className="flex items-center">
        <div className="w-full bg-gray-200 rounded-full h-2.5">
          <div className={`h-2.5 rounded-full ${color}`} style={{ width: total > 0 ? `${(used / total) * 100}%` : '0%' }}></div>
        </div>
        {title !== 'Sick' ? <span className="ml-2 text-sm text-gray-600">{used}/{total}</span> : <span className="ml-2 text-sm text-gray-600">{used}</span>}
      </div>
    </div>
  );

  const StatusBadge = ({ status }: { status: LeaveRequestStatus | PermissionRequestStatus }) => {
    const getStatusColor = (s: LeaveRequestStatus | PermissionRequestStatus) => {
      switch (s) {
        case LeaveRequestStatus.Approved: case PermissionRequestStatus.Approved: return "bg-green-100 text-green-800";
        case LeaveRequestStatus.Pending: case PermissionRequestStatus.Pending: return "bg-yellow-100 text-yellow-800";
        case LeaveRequestStatus.Rejected: case PermissionRequestStatus.Rejected: return "bg-red-100 text-red-800";
        case LeaveRequestStatus.Cancelled: case PermissionRequestStatus.Cancelled: return "bg-gray-100 text-gray-800"; // Or a more distinct color for cancelled
        default: return "bg-gray-100 text-gray-800";
      }
    };
    return <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(status)}`}>{status}</span>;
  };

  useEffect(() => {
    const fetchUserInfo = async () => {
      if (!auth.id) return;
      // setIsLoading(true); // Consider if this loading state is for user info or table data
      try {
        const userInfoResponse = await API.LEAVE.GET_USER_INFO(auth.id);
        if (userInfoResponse && 'data' in userInfoResponse) {
          setUser(userInfoResponse.data);
        }
      } catch (error) {
        console.error('Error fetching user info:', error);
      } finally {
        // setIsLoading(false);
      }
    };
    fetchUserInfo();
  }, [auth.id]);

  // Main data fetching useEffect for server-side DataTable
  useEffect(() => {
    if (!auth?.id) return;
    setIsLoading(true);

    const fetchLeaves = async () => {
      const params: Record<string, string | number | boolean | undefined> = {
        page: leaveDataTablePage,
        pageSize: leaveDataTableItemsPerPage,
        searchTerm: leaveDataTableSearchText || undefined,
        status: filters.status || undefined,
        type: filters.type || undefined,
        fromDate: filters.startDate || undefined,
        toDate: filters.endDate || undefined,
      };
      try {
        const response = await API.LEAVE.GET_ALL_BY_USER(auth.id, params);
        if (response && response.data && !response.error) {
          setAllLeaves(response.data.items ?? []);
          setTotalLeavesCount(response.data.totalCount ?? 0);
        } else {
          toast.error(response.message || "Failed to fetch leaves.");
          setAllLeaves([]);
          setTotalLeavesCount(0);
        }
      } catch (error) {
        toast.error("An error occurred while fetching leaves.");
        console.error('Error fetching leaves:', error);
      } finally {
        setIsLoading(false);
      }
    };

    const fetchPermissions = async () => {
      const params: Record<string, string | number | boolean | undefined> = {
        page: permissionDataTablePage,
        pageSize: permissionDataTableItemsPerPage,
        searchTerm: permissionDataTableSearchText || undefined,
        status: filters.status || undefined,
        type: filters.type || undefined,
        date: filters.startDate || undefined, // Assuming startDate from component filter is used for 'date'
      };
      try {
        // Assuming Permission.GET_ALL_BY_USER is adapted for pagination/filtering
        const response = await Permission.GET_ALL_BY_USER(auth.id, params);
        if (response && response.data && !response.error) {
          setAllPermissions(response.data.items ?? []);
          setTotalPermissionsCount(response.data.totalCount ?? 0);
        } else {
          toast.error(response.message || "Failed to fetch permissions.");
          setAllPermissions([]);
          setTotalPermissionsCount(0);
        }
      } catch (error) {
        toast.error("An error occurred while fetching permissions.");
        console.error('Error fetching permissions:', error);
      } finally {
        setIsLoading(false);
      }
    };

    if (activeTab === "leaves") {
      fetchLeaves();
    } else if (activeTab === "permissions") {
      fetchPermissions();
    }
  }, [
    auth.id, activeTab, filters,
    leaveDataTablePage, leaveDataTableItemsPerPage, leaveDataTableSearchText,
    permissionDataTablePage, permissionDataTableItemsPerPage, permissionDataTableSearchText
  ]);


  const refreshData = useCallback(() => {
    // This will trigger the main useEffect to refetch data with current parameters
    if (activeTab === "leaves") {
        // To ensure re-fetch, we can slightly change a dependency or use a dedicated refresh trigger
        // For simplicity, directly calling the fetch logic or relying on state change from onPageChange
        // If onPageChange is not called, we might need a refresh trigger state.
        // For now, let's assume the main useEffect will handle it if its dependencies are set correctly.
        // A more explicit way:
        const currentFilters = filters;
        const currentPage = leaveDataTablePage;
        const currentItemsPerPage = leaveDataTableItemsPerPage;
        const currentSearchText = leaveDataTableSearchText;
        // Re-set one of the states to trigger the effect, or call fetch function directly
        // This is a bit of a hack, a dedicated refresh trigger state is cleaner
        setLeaveDataTablePage(1); // Reset to page 1 on refresh or keep current
        setTimeout(() => setLeaveDataTablePage(currentPage), 0);


    } else {
        const currentFilters = filters;
        const currentPage = permissionDataTablePage;
        const currentItemsPerPage = permissionDataTableItemsPerPage;
        const currentSearchText = permissionDataTableSearchText;
        setPermissionDataTablePage(1);
        setTimeout(() => setPermissionDataTablePage(currentPage), 0);
    }
  }, [activeTab, filters, leaveDataTablePage, leaveDataTableItemsPerPage, leaveDataTableSearchText, permissionDataTablePage, permissionDataTableItemsPerPage, permissionDataTableSearchText]);


  const handleCancelAction = async (target: CancelTarget) => {
    setIsLoading(true);
    try {
      let response;
      if (target.type === 'leave') {
        response = await LEAVE.CANCEL_LEAVE(target.id);
      } else {
        response = await Permission.CANCEL_PERMISSION(target.id);
      }

      if (response && response !== false && !response.error) {
        toast.success(`${target.type.charAt(0).toUpperCase() + target.type.slice(1)} cancelled successfully.`);
        refreshData(); // Refresh data
      } else {
        toast.error( (response as any)?.message || `Failed to cancel ${target.type}.`);
      }
    } catch (error) {
      toast.error(`An unexpected error occurred while cancelling ${target.type}.`);
      console.error(`Error cancelling ${target.type}:`, error);
    } finally {
      setIsLoading(false);
      setIsCancelModalOpen(false);
    }
  };

  const formatDateForTable = (dateString: string) => dateString ? format(new Date(dateString), "dd MMM yyyy") : "N/A";

  const transformedLeaveData = useMemo(() =>
    allLeaves.map(leave => ({
      "Type": leave.type,
      "Start Date": leave.startDate,
      "End Date": leave.endDate,
      "Duration (Days)": leave.duration,
      "Status": leave.status,
      "Actions": leave.id
    })), [allLeaves]);

  const transformedPermissionData = useMemo(() =>
    allPermissions.map(permission => ({
      "Type": permission.type,
      "Date": permission.permissionDate,
      "Reason": permission.reason,
      "Status": permission.status,
      "Actions": permission.id
    })), [allPermissions]);

  const leaveColumnRenderers = {
    "Start Date": (value: string) => formatDateForTable(value),
    "End Date": (value: string) => formatDateForTable(value),
    "Status": (value: LeaveRequestStatus) => <StatusBadge status={value} />,
    "Actions": (value: number, row: any) => (
      row.Status !== LeaveRequestStatus.Cancelled && row.Status !== LeaveRequestStatus.Rejected && (
        <button onClick={() => { setCancelTarget({ id: value, type: 'leave' }); setIsCancelModalOpen(true); }} className="text-red-600 hover:text-red-800">Cancel</button>
      )
    ),
  };

  const permissionColumnRenderers = {
    "Date": (value: string) => formatDateForTable(value),
    "Status": (value: PermissionRequestStatus) => <StatusBadge status={value} />,
    "Actions": (value: number, row: any) => (
      row.Status !== PermissionRequestStatus.Cancelled && row.Status !== PermissionRequestStatus.Rejected && (
        <button onClick={() => { setCancelTarget({ id: value, type: 'permission' }); setIsCancelModalOpen(true); }} className="text-red-600 hover:text-red-800">Cancel</button>
      )
    ),
  };

  const handleLeaveTableChange = (page: number, itemsPerPage: number, _dtFilters: Record<string, any>, searchText: string) => {
    setLeaveDataTablePage(page);
    setLeaveDataTableItemsPerPage(itemsPerPage);
    setLeaveDataTableSearchText(searchText);
    // setLeaveDataTableFilters(dtFilters); // If using DataTable's own filters
  };

  const handlePermissionTableChange = (page: number, itemsPerPage: number, _dtFilters: Record<string, any>, searchText: string) => {
    setPermissionDataTablePage(page);
    setPermissionDataTableItemsPerPage(itemsPerPage);
    setPermissionDataTableSearchText(searchText);
    // setPermissionDataTableFilters(dtFilters); // If using DataTable's own filters
  };

  return (
    <div className="min-h-screen bg-gray-50 w-full">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <h1 className="ml-3 text-xl font-semibold text-gray-900">My Leave</h1>
            </div>
            <button onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)} className="md:hidden">
              <FiX className={`h-6 w-6 ${isMobileMenuOpen ? 'block' : 'hidden'}`} />
              {/* <FiMenu className={`h-6 w-6 ${!isMobileMenuOpen ? 'block' : 'hidden'}`} /> */}
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <LeaveBalanceCard title="Annual" used={user?.vacation?.annual ?? 0} total={user?.vacation?.annual_MAX ?? 0} color="bg-blue-500" />
          <LeaveBalanceCard title="Sick" used={user?.vacation?.sick ?? 0} total={100} color="bg-green-500" /> {/* Assuming total 100 for sick if not in API */}
          <LeaveBalanceCard title="Emergency" used={user?.vacation?.emergency ?? 0} total={user?.vacation?.emergency_MAX ?? 0} color="bg-yellow-500" />
          <LeaveBalanceCard title="Permissions" used={user?.permission ?? 0} total={user?.permission_MAX ?? 0} color="bg-purple-500" />
        </div>

        <div className="mb-6">
          <div className="border-b border-gray-200">
            <div className="flex justify-between items-center">
              <nav className="-mb-px flex space-x-8">
                <button onClick={() => setActiveTab("leaves")} className={activeTab === "leaves" ? "border-blue-500 text-blue-600 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"}>Leaves</button>
                <button onClick={() => setActiveTab("permissions")} className={activeTab === "permissions" ? "border-blue-500 text-blue-600 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"}>Permissions</button>
              </nav>
              {activeTab === "leaves" ? (
                <button onClick={() => setIsLeaveModalOpen(true)} className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors">
                  <PlusIcon className="w-5 h-5 mr-2" /> New Leave Request
                </button>
              ) : (
                <button onClick={() => setIsPermissionModalOpen(true)} className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors">
                  <PlusIcon className="w-5 h-5 mr-2" /> New Permission Request
                </button>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="p-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-medium text-gray-900">{activeTab === "leaves" ? "Leave Applications" : "Permission Requests"}</h2>
              <button onClick={() => setShowFilters(!showFilters)} className="flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                {/* <FiFilter className="mr-2 h-4 w-4" />  */}
                Filter
              </button>
            </div>
            {showFilters && (
              <div className="mt-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 items-end">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={filters.status} onChange={(e) => setFilters({ ...filters, status: e.target.value })}>
                    <option value="">All</option>
                    {Object.values(LeaveRequestStatus).map(status => <option key={status} value={status}>{status}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                  <select className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={filters.type} onChange={(e) => setFilters({ ...filters, type: e.target.value })}>
                    <option value="">All</option>
                    {activeTab === "leaves" ? Object.values(LeaveRequestType).map(type => <option key={type} value={type}>{type}</option>)
                      : Object.values(PermissionType).map(type => <option key={type} value={type}>{type}</option>)}
                  </select>
                </div>
                {activeTab === "leaves" ? (
                  <>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                      <input type="date" className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={filters.startDate} onChange={(e) => setFilters({ ...filters, startDate: e.target.value })} />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                      <input type="date" className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={filters.endDate} onChange={(e) => setFilters({ ...filters, endDate: e.target.value })} min={filters.startDate} />
                    </div>
                  </>
                ) : (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Date</label>
                    <input type="date" className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" value={filters.startDate} onChange={(e) => setFilters({ ...filters, startDate: e.target.value, endDate: '' })} />
                  </div>
                )}
                <div className="lg:col-start-4 flex justify-end">
                  <button onClick={() => setFilters({ status: '', startDate: '', endDate: '', type: '' })} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">Clear Filters</button>
                </div>
              </div>
            )}
          </div>

          <div className="overflow-x-auto">
            {activeTab === "leaves" ? (
              <DataTable
                data={transformedLeaveData}
                totalCount={totalLeavesCount}
                onPageChange={handleLeaveTableChange}
                itemsPerPage={leaveDataTableItemsPerPage}
                loading={isLoading}
                columnRenderers={leaveColumnRenderers}
                serverSide={true} // Enable server-side operations
              />
            ) : (
              <DataTable
                data={transformedPermissionData}
                totalCount={totalPermissionsCount}
                onPageChange={handlePermissionTableChange}
                itemsPerPage={permissionDataTableItemsPerPage}
                loading={isLoading}
                columnRenderers={permissionColumnRenderers}
                serverSide={true} // Enable server-side operations
              />
            )}
          </div>
        </div>
      </main>

      <LeaveModal isOpen={isLeaveModalOpen} onClose={() => setIsLeaveModalOpen(false)} onSuccess={refreshData} />
      <PermissionModal isOpen={isPermissionModalOpen} onClose={() => setIsPermissionModalOpen(false)} onSuccess={refreshData} />

      {isCancelModalOpen && cancelTarget && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg shadow-xl max-w-md w-full">
            <h3 className="text-lg font-medium leading-6 text-gray-900 mb-4">Confirm Cancellation</h3>
            <p className="text-sm text-gray-500 mb-6">Are you sure you want to cancel this {cancelTarget.type} request?</p>
            <div className="flex justify-end space-x-3">
              <button onClick={() => setIsCancelModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50">No, Keep It</button>
              <button onClick={() => handleCancelAction(cancelTarget)} className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700">Yes, Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default LeaveManagement;
