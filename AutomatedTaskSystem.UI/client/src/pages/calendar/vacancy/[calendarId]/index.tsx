import { useEffect, useState } from 'react';
// import LEAVE, {  IGetLeaveRequestForDetails, IGetOpinion, IOpinion, LeaveRequestStatus } from '../../../../lib/API/Leave';
// import { useParams } from 'next/navigation';
import { useRouter } from 'next/router';
import { format } from 'date-fns';
import { useAppSelector } from '../../../../app/hooks';
import LEAVE, { IGetLeaveRequestForDetails, IGetOpinion, IOpinion, LeaveRequestStatus } from '../../../../lib/API/Leave';
import AttachmentViewer, { Attachment } from '../../../../components/pageComponent/leave/attachmentViewer';
import { url } from '../../../../lib/API';
// import { User } from 'lucide-react';


const LeaveRequestDetails = () => {

   const router = useRouter();
    const { calendarId } = router.query;

  const auth = useAppSelector((s) => s.authSlice);
  // Mock data based on the image
  const [request, setRequest] = useState<IGetLeaveRequestForDetails | null>(null);
  const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
  const [commentable,setCommentable] = useState(true)

  const [approvals, setApprovals] = useState<IGetOpinion[] | null>(null);
  const [attachments,setAttachments] = useState<Attachment[]>([])



  useEffect(() => {
    //console.log(calendarId);
    
    let isMounted = true;
        if (calendarId) {
            fetchLeaveRequest(Number(calendarId)).then(({ leaveDetails }) => {
              if (isMounted&&leaveDetails != false && leaveDetails?.data) {
                setRequest(leaveDetails.data);
                setApprovals(leaveDetails.data.opinions)
                //console.log(leaveDetails,"after assign");
                //console.log(leaveDetails.data.opinions,"opnions");
                //console.log(auth.id);
                
                if((leaveDetails as IGetLeaveRequestForDetails).opinions?.some(x => x.user.id == auth.id))
                {
                    //console.log("commentable");
                    
                    setCommentable(false)
                    
                }
              }
            });
        }
        return () => {
            isMounted = false;
        };
    }, [calendarId]);

        const fetchLeaveRequest = async (id: number) => {
        try {
            const leaveDetails = await LEAVE.GET_LEAVE_BY_USER(Number(calendarId));
            //console.log(leaveDetails);
            
            return { leaveDetails };
        } catch (error) {
            console.error('Error fetching leave request:', error);
            return { leaveDetails: null, opinions: [] };
        }
    };


    const handleSubmitOpinion = async (status: LeaveRequestStatus.Approved | LeaveRequestStatus.Rejected) => {
        setIsSubmitting(true);
        try {
            let op:IOpinion = {
                leaveRequestId: request?.id??0,
                comment:comment,
                status:status,
                isApproved:status == "Approved"?true:false,
                user:{id:auth.id,name:auth.name,role:auth.role}
            }
            await LEAVE.CREATE_OPINION(op)

            // Optional: refetch the leave request to update approvals
            const { leaveDetails } = await fetchLeaveRequest(Number(calendarId));
            if (leaveDetails && leaveDetails.data) {
                setRequest(leaveDetails.data);
                setApprovals(leaveDetails?.data.opinions);
            } else {
                // Otherwise, set the state to null (e.g., if API returned no data, or an error)
                setRequest(null);
            }
            // Clear form
            setComment('');
        } catch (error) {
            console.error('Error submitting opinion:', error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDownloadAttachment = async (attachment: Attachment) => {
      try {
        const response = await fetch(`/api/leave-requests/medical-certificate/${request?.id}`, {
          method: 'GET',
        });

        if (!response.ok) throw new Error("Failed to download file");

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);

        const link = document.createElement("a");
        link.href = url;
        link.download = attachment.name || "attachment";
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error("Error downloading attachment:", error);
        alert("Failed to download file.");
      }
    };
            


  // Function to get status color based on status
  const getStatusColor = (status: LeaveRequestStatus): string => {
    switch(status) {
      case "Approved":
        return "text-green-600";
      case "Rejected":
        return "text-red-600";
      case "Pending":
      default:
        return "text-yellow-600";
    }
  };

   

  const handleClick = async () => {
    

        const success = await LEAVE.GET_SICK_ATTACHMENT(Number(calendarId));
        if (!success) {
          alert("فشل تحميل المرفق الطبي");

      
    }
  }

  const handleDownloadMedicalCertificate = async () => {
  try {
    const response = await fetch(`${url}/leave/medical-certificate/${calendarId}`, {
      method: "GET",
    });

    if (!response.ok) {
      throw new Error("Failed to fetch medical certificate");
    }

    const blob = await response.blob();
    const contentDisposition = response.headers.get("Content-Disposition");
    const fileNameMatch = contentDisposition?.match(/filename="?([^"]+)"?/);
    const fileName = fileNameMatch?.[1] || "medical-certificate";

    // Trigger download
    const downloadUrl = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = downloadUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(downloadUrl);

    return true;
  } catch (error) {
    console.error("Error downloading medical certificate:", error);
    alert("Failed to download medical certificate");
    return false;
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
                  {request?.type == "Sick" && <div className="row-span-5">
                    <span className="text-gray-500">Attachment: </span>
                    {/* <AttachmentViewer attachments={attachments} onDownload={handleDownloadAttachment} /> */}
                    {/* <button
                        onClick={handleClick}
                        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
                      >
                        تحميل المرفق الطبي
                    </button> */}
                    <button
                        onClick={handleDownloadMedicalCertificate}
                        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                      >
                        Download Medical Certificate
                      </button>

                  </div>}
                  <div>
                    <span className="text-gray-500">Type: </span>
                    <span className="text-blue-600">{request?.type}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Final status: </span>
                    <span className={`${getStatusColor(request?.status??LeaveRequestStatus.Pending )}`}>{request?.status}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Starts at: </span>
                    <span>
                                        {request?.startDate
                        ? format(new Date(request.startDate), "dd MMM yyyy")
                        : "N/A"}
                    </span>
                  </div>
                  <div>
                    <span className="text-gray-500">Ends at: </span>
                     <span>
                                        {request?.endDate
                        ? format(new Date(request.endDate), "dd MMM yyyy")
                        : "N/A"}
                    </span>
                  </div>
                  <div>
                    <span className="text-gray-500">Vacancy Duration: </span>
                    <span className="text-blue-600">{request?.duration} Days</span>
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
                  <span className={`${getStatusColor(approval.isApproved?LeaveRequestStatus.Approved:LeaveRequestStatus.Rejected)}`}>{approval.isApproved?"Approved":"Rejected"}</span>
                </div>
              </div>
              <div>
                <span className="text-gray-500">Comment: </span>
                <span className="text-blue-600">{approval.comment}</span>
              </div>
            </div>
          ))}
          {((auth.role < 3 || auth.role == 4 ) && !request?.opinions.some(x => x.user.id == auth.id))&& (
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
                    onClick={() => handleSubmitOpinion(LeaveRequestStatus.Approved)}
                    disabled={isSubmitting }
                >
                    Approve
                </button>
                <button
                    className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50"
                    onClick={() => handleSubmitOpinion(LeaveRequestStatus.Rejected)}
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

export default LeaveRequestDetails;