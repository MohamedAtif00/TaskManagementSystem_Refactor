import React, { useState, useEffect } from "react";
import { FiMenu, FiX, FiFilter, FiChevronLeft, FiChevronRight, FiPlus } from "react-icons/fi";
import { format } from "date-fns";
import LeftArrowIcon from "../../assets/Icons/LeftArrow";
import RightArrowIcon from "../../assets/Icons/RightArrow";
import FilterIcon from "../../assets/Icons/Filter";
import { PlusIcon } from "@heroicons/react/24/outline";
import { IGetPermission, PermissionType } from "../../lib/API/Permission";
import { LeaveRequestStatus } from "../../lib/API/Leave";
import { useAppSelector } from "../../app/hooks";


// const mockPermissionData: IGetPermission[] = [
//   {
//     id: 1,
//     type: PermissionType.Morning,
//     date: "2024-01-10",
//     reason: "Morning appointment",
//     status: "Approved" as LeaveRequestStatus,
//     createdAt: "2024-01-09"
//   },
//   {
//     id: 2,
//     type: PermissionType.Morning,
//     date: "2024-01-12",
//     reason: "Doctor visit",
//     status: "Pending" as LeaveRequestStatus,
//     createdAt: "2024-01-11"
//   }
// ];

// Added: New LeaveModal component
const LeaveModal = ({ isOpen, onClose }:{isOpen:boolean,onClose:()=>void}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">إنشاء طلب إجازة</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500"
          >
            <FiX size={20} />
          </button>
        </div>
        <form className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">نوع الإجازة</label>
            <select className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
              <option>إجازة سنوية</option>
              <option>إجازة مرضية</option>
              <option>إجازة طارئة</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ البداية</label>
            <input type="date" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">تاريخ النهاية</label>
            <input type="date" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">السبب</label>
            <textarea className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" rows={3}></textarea>
          </div>
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              إلغاء
            </button>
            <button
              type="submit"
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
            >
              إرسال
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Added: New PermissionModal component
const PermissionModal = ({ isOpen, onClose }:{isOpen:boolean,onClose:()=>void}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white" dir="rtl">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-medium">إنشاء طلب إذن</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-500"
          >
            <FiX size={20} />
          </button>
        </div>
        <form className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">نوع الإذن</label>
            <select className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
              <option>إذن صباحي</option>
              <option>إذن مسائي</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">التاريخ</label>
            <input type="date" className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">السبب</label>
            <textarea className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" rows={3}></textarea>
          </div>
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              إلغاء
            </button>
            <button
              type="submit"
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
            >
              إرسال
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const LeaveManagement = () => {
  // Updated: Added state for permission modal
  const [isLeaveModalOpen, setIsLeaveModalOpen] = useState(false);
  const [isPermissionModalOpen, setIsPermissionModalOpen] = useState(false);
  const auth = useAppSelector((e) => e.authSlice);
  const [activeTab, setActiveTab] = useState("leaves");
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  // import { useAppSelector } from "../../../../app/hooks";

  const mockLeaveData = [
    {
      id: 1,
      type: "Annual Leave",
      startDate: "2024-01-15",
      endDate: "2024-01-17",
      duration: "3 days",
      status: "Approved" as LeaveRequestStatus
    },
    {
      id: 2,
      type: "Sick Leave",
      startDate: "2024-01-20",
      endDate: "2024-01-20",
      duration: "1 day",
      status: "Pending" as LeaveRequestStatus
    }
  ];

  // Updated: Modified mock permission data to include types
  const mockPermissionData: IGetPermission[] = [
    {
      id: 1,
      type: PermissionType.Morning,
      date: "2024-01-10",
      reason: "Morning appointment",
      status: "Approved" as LeaveRequestStatus,
      createdAt: "2024-01-09"
    },
    {
      id: 2,
      type: PermissionType.Morning,
      date: "2024-01-12",
      reason: "Doctor visit",
      status: "Pending" as LeaveRequestStatus,
      createdAt: "2024-01-11"
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

  return (
    <div className="min-h-screen bg-gray-50 w-full">
      {/* <header className="bg-white shadow-sm">
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
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="md:hidden"
            >
              {isMobileMenuOpen ? <FiX size={24} /> : <FiMenu size={24} />}
            </button>
          </div>
        </div>
      </header> */}

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <LeaveBalanceCard title="Annual Leaves" used={2} total={20} color="bg-blue-500" />
          <LeaveBalanceCard title="Sick Leaves" used={1} total={10} color="bg-green-500" />
          <LeaveBalanceCard title="Emergency Leaves" used={0} total={5} color="bg-yellow-500" />
          <LeaveBalanceCard title="Permissions" used={3} total={5} color="bg-purple-500" />
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
              <button className="flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                <FilterIcon className="mr-2 w-10 h-10" />
                Filter
              </button>
            </div>
          </div>

          <div className="overflow-x-auto">
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
                  ? mockLeaveData.map((leave) => (
                      <tr key={leave.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{leave.type}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{format(new Date(leave.startDate), "dd MMM yyyy")}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{format(new Date(leave.endDate), "dd MMM yyyy")}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{leave.duration}</td>
                        {auth &&
                          <td className="px-6 py-4 whitespace-nowrap">
                            <StatusBadge status={leave.status} />
                          </td>
                        }
                      </tr>
                    ))
                  : mockPermissionData.map((permission) => (
                      <tr key={permission.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{permission.type}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{format(new Date(permission.date), "dd MMM yyyy")}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{permission.reason}</td>
                        {auth &&
                          <td className="px-6 py-4 whitespace-nowrap" >
                            {permission.status && <StatusBadge status={permission.status} />}
                          </td>
                        }
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

      {/* Updated: Added both modals with separate states */}
      <LeaveModal isOpen={isLeaveModalOpen} onClose={() => setIsLeaveModalOpen(false)} />
      <PermissionModal isOpen={isPermissionModalOpen} onClose={() => setIsPermissionModalOpen(false)} />
    </div>
  );
};

export default LeaveManagement;