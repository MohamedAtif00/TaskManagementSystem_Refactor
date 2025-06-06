
import { useEffect, useState } from 'react';
// import LEAVE, {  IGetLeaveRequestForDetails, IGetOpinion, IOpinion, LeaveRequestStatus } from '../../../../lib/API/Leave';
// import { useParams } from 'next/navigation';
import { useRouter } from 'next/router';
import { format } from 'date-fns';
import { useAppSelector } from '../../../../app/hooks';
import PERMISSION, { IGetOpinion, IGetPermissionDetails, IOpinion, PermissionRequestStatus } from '../../../../lib/API/Permission';
// import { User } from 'lucide-react';


const PermissionDetails = () => {

   const router = useRouter();
    const { calendarId } = router.query;

  const auth = useAppSelector((s) => s.authSlice);
  // Mock data based on the image
  const [request, setRequest] = useState<IGetPermissionDetails | null>(null);
  const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
  const [commentable,setCommentable] = useState(true)

  const [approvals, setApprovals] = useState<IGetOpinion[] | null>(null);


const parseTime = (date: string, time: string): Date => {
  return new Date(`${date}T${time}`);
};

  useEffect(() => {
    //console.log(calendarId);
    
    let isMounted = true;

    
        if (calendarId) {
          fetchPermissionRequest(Number(calendarId)).then(({ leaveDetails }) => {
              if (isMounted) {
                  if (leaveDetails && leaveDetails.data) {

                      setRequest(leaveDetails.data); // Set the single object
                      setApprovals(leaveDetails.data.opinions??null); // Access opinions directly from leaveDetails.data

                      // Access opinions from leaveDetails.data, as it's the IGetPermissionDetails object
                      if (leaveDetails.data.opinions?.some(x => x.user?.id === auth.id)) {
                          // console.log("commentable");
                          setCommentable(false);
                      } else {
                          setCommentable(true); // Default to commentable if not found
                      }
                  } else {
                      // Handle the case where leaveDetails is false or leaveDetails.data is null/undefined
                      console.warn("Failed to fetch permission details or no data received.");
                      setRequest(null);
                      setApprovals(null); // Reset approvals
                      setCommentable(true); // Reset commentable state if no data
                  }
              }
          });
      }

      // ... rest of your useEffect cleanup ...
      return () => {
          isMounted = false;
      };
    }, [calendarId]);

        const fetchPermissionRequest = async (id: number) => {
        try {
            const leaveDetails = await PERMISSION.GET_DETAILS(Number(calendarId));
            //console.log(leaveDetails);
            
            return { leaveDetails };
        } catch (error) {
            console.error('Error fetching leave request:', error);
            return { leaveDetails: null, opinions: [] };
        }
    };


    const handleSubmitOpinion = async (status: PermissionRequestStatus.Approved | PermissionRequestStatus.Rejected) => {
        setIsSubmitting(true);
        try {
          debugger
            let op:IOpinion = {
                permissionId: request?.id??0,
                comment:comment,
                status:status,
                isApproved:status == PermissionRequestStatus.Approved?true:false,
                user:{id:auth.id,name:auth.name,role:auth.role}
            }
            await PERMISSION.CREATE_OPINION(op)
            debugger
            // Optional: refetch the leave request to update approvals
            const { leaveDetails } = await fetchPermissionRequest(Number(calendarId));

            if(leaveDetails)
            setRequest(leaveDetails?.data??null);
             setApprovals(leaveDetails.data.opinions??null);
            // Clear form
            setComment('');
        } catch (error) {
            console.error('Error submitting opinion:', error);
        } finally {
            setIsSubmitting(false);
        }
    };
            


  // Function to get status color based on status
  const getStatusColor = (status: PermissionRequestStatus): string => {
    switch(status) {
      case PermissionRequestStatus.Approved:
        return "text-green-600";
      case PermissionRequestStatus.Rejected:
        return "text-red-600";
      case PermissionRequestStatus.Pending:
        return "text-yellow-600";
      case PermissionRequestStatus.Cancelled:
         return "text-red-600";
      default:
        return "text-yellow-600";
    }
  };

  return (
    <div className="bg-gray-50 min-h-screen p-6 w-full">
      <div className="max-w-5xl mx-auto">
        {/* Header */}
        <div className="flex items-center mb-8">
          {/* <User className="w-6 h-6 mr-2" /> */}
          <h1 className="text-xl font-medium">Calendar {'>'} Request Details</h1>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          {/* User Info */}
          <div className="flex justify-between items-start mb-8">
            <div>
              <h2 className="text-xl font-semibold">{request?.user?.name}</h2>
              <p className="text-gray-500">{request?.user?.code}</p>
              
              <div className="grid grid-cols-2 gap-x-16 gap-y-2 mt-4">
                <div>
                  <span className="text-gray-500">Status: </span>
                  <span className="text-blue-600">Active</span>
                </div>
                <div>
                  <span className="text-gray-500">Department: </span>
                  <span className="text-blue-600">{request?.user?.group?.name}</span>
                </div>
                <div>
                  <span className="text-gray-500">Type: </span>
                  <span className="text-blue-600">Internal</span>
                </div>
                <div>
                  <span className="text-gray-500">Access type: </span>
                  <span className="text-blue-600">Member</span>
                </div>
                <div>
                  <span className="text-gray-500">Email: </span>
                  <span className="text-blue-600">{request?.user?.email??""}</span>
                </div>
                <div>
                  <span className="text-gray-500">HR ID: </span>
                  <span className="text-blue-600">{request?.user?.hrCode}</span>
                </div>
                <div>
                  <span className="text-gray-500">Phone: </span>
                  <span className="text-blue-600">{request?.user?.phone}</span>
                </div>
                 <div>
                  <span className="text-gray-500">Role: </span>
                   <span className="text-[#5570FF]">
                                    {request?.user?.role === 0 ? "Project Manager" : 
                                    request?.user?.role === 1 ? "Section Head" :
                                    request?.user?.role === 2 ? "Team Leader" : "Member"}
                                    </span>
                </div>
              </div>
            </div>

            {/* Leave Balance */}
            <div className="flex space-x-4">
              <div className="text-center border-r pr-6">
                <h3 className="font-medium">Annual</h3>
                <p className="text-blue-600 font-bold text-xl">{request?.user?.annual_leave} <span className="text-sm text-gray-500">/{request?.user?.annual_leave_MAX}</span></p>
              </div>
              <div className="text-center border-r pr-6">
                <h3 className="font-medium">Sick</h3>
                <p className="text-blue-600 font-bold text-xl">{request?.user?.sick_leave} </p>
              </div>
              <div className="text-center">
                <h3 className="font-medium">Emergency</h3>
                <p className="text-blue-600 font-bold text-xl">{request?.user?.emergency_leave} <span className="text-sm text-gray-500">/{request?.user?.emergency_leave_MAX}</span></p>
              </div>
            </div>
          </div>

          {/* Request Details */}
          <h2 className="text-lg font-semibold mb-4">Request Details</h2>
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-6">
            <div className="flex justify-between">
              <div className="w-2/3">
                <div className="grid grid-cols-2 gap-x-8 gap-y-4">
                  <div>
                    <span className="text-gray-500">Requested at: </span>
                   <span className="text-blue-600">
                    {request?.dateCreated
                        ? format(new Date(request.dateCreated), "dd MMM yyyy")
                        : "N/A"}
                    </span>

                  </div>
                  
                  <div>
                    <span className="text-gray-500">Type: </span>
                    <span className="text-blue-600">{request?.type}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Final status: </span>
                    <span className={`${getStatusColor(request?.status??PermissionRequestStatus.Pending)}`}>{request?.status}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Starts at: </span>
                    <span>
                        {request?.fromTime
                        ? format(parseTime(request.permissionDate, request.fromTime), "hh:mm a")
                        : "N/A"}
                    </span>
                    </div>
                    <div>
                    <span className="text-gray-500">Ends at: </span>
                    <span>
                        {request?.toTime
                        ? format(parseTime(request.permissionDate, request.toTime), "hh:mm a")
                        : "N/A"}
                    </span>
                    </div>


                  <div>
                    <span className="text-gray-500">Permission Duration: </span>
                    <span className="text-blue-600">{request?.duration} </span>
                  </div>
                  <div>
                    <span className="text-gray-500">Reason: </span>
                    <span className="text-blue-600">{request?.reason}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {auth.role == 4? <div className="mt-4">
            <span className="text-gray-500">Note: </span>
            <span className="text-blue-600">{request?.noteToManager}</span>
          </div>:<></>}


          {/* Opinions / Approvals */}
          <h2 className="text-lg font-semibold mt-8 mb-4">Opinions</h2>
          {approvals?.map((approval, index) => (
            <div key={index} className="border border-gray-200 rounded-lg p-6 mb-4"  style={{borderWidth:'2px ',borderColor:'black',borderStyle:'solid'}}>
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <span className="text-gray-500">User Name: </span>
                  <span className="text-blue-600">{approval.user?.name}</span>
                </div>
                <div>
                  <span className="text-gray-500">Role: </span>
                  <span className="text-blue-600">{approval.user?.role == 0?"Cordinator":approval.user?.role == 2?"TeamLeader":"Owner"}</span>
                </div>
                <div>
                  <span className="text-gray-500">Status: </span>
                  <span className={`${getStatusColor(approval.isApproved?PermissionRequestStatus.Approved:PermissionRequestStatus.Rejected)}`}>{approval.isApproved?"Approved":"Rejected"}</span>
                </div>
              </div>
              <div>
                <span className="text-gray-500">Comment: </span>
                <span className="text-blue-600">{approval.comment}</span>
              </div>
            </div>
          ))}
          {((auth.role < 3 || auth.role == 4 ) && !request?.opinions?.some(x => x.user?.id == auth.id))&& (
            <div className="mt-6 pt-6">
                <h2 className="text-lg font-semibold mb-2">Your Opinion</h2>
                <textarea
                className="w-full p-3 border border-gray-300 rounded-md mb-4"
                placeholder="Add your comment..."
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                />
                <div className="flex space-x-4">
                <button
                    className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 disabled:opacity-50"
                    onClick={() => handleSubmitOpinion(PermissionRequestStatus.Approved)}
                    disabled={isSubmitting }
                >
                    Approve
                </button>
                <button
                    className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50"
                    onClick={() => handleSubmitOpinion(PermissionRequestStatus.Rejected)}
                    disabled={isSubmitting}
                >
                    Reject
                </button>
                </div>
            </div>
            )}

        </div>
      </div>
    </div>
  );
};

export default PermissionDetails;