import { useEffect, useState } from 'react';
import { useRouter } from 'next/router';
import { format } from 'date-fns';
import { useAppSelector } from '../../../../app/hooks';
import { url } from '../../../../lib/API'; // Assuming 'url' is your base API URL
import { toast } from 'react-toastify';
import TableLoader from '../../../../components/loader/table-loader';
import WORK_FROM_HOME, { IGetWorkFromHomeRequestForDetails, IGetOpinionWorkFromHome, WorkFromHomeStatus, ICreateWorkFromHomeOpinion } from '../../../../lib/API/workFromHome';
// import { User } from 'lucide-react'; // Uncomment if you use this icon

const WorkFromHomeRequestDetails = () => {
    const router = useRouter();
    const { calendarId } = router.query; // The ID of the Work From Home request

    const auth = useAppSelector((s) => s.authSlice);

    const [request, setRequest] = useState<IGetWorkFromHomeRequestForDetails | null>(null);
    const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [commentable, setCommentable] = useState(true); // Can the current user submit an opinion?

    const [approvals, setApprovals] = useState<IGetOpinionWorkFromHome[] | null>(null);

    const [isLoadingDetails, setIsLoadingDetails] = useState(true);

    useEffect(() => {
        let isMounted = true;
        const numericCalendarId = Number(calendarId);

        // Only proceed if calendarId is a valid number
        if (calendarId && !isNaN(numericCalendarId) && numericCalendarId > 0) {
            setIsLoadingDetails(true);

            const doFetch = async () => {
                try {
                    // Use the WORK_FROM_HOME service to fetch details
                    const response = await WORK_FROM_HOME.GET_SINGLE(numericCalendarId);

                    if (isMounted) {
                        if (response && !response.error && response.data) {
                            setRequest(response.data);
                            setApprovals(response.data.opinions ?? null);
                            // Determine if the current user can comment (i.e., hasn't commented yet)
                            const canComment = !response.data.opinions?.some(op => op.user?.id === auth.id);
                            setCommentable(canComment);
                        } else {
                            setRequest(null);
                            setApprovals(null);
                            setCommentable(true); // Default if no data or error
                            toast.error(response?.message || "Failed to fetch work from home details.");
                            console.warn(response?.message || "Failed to fetch work from home details or no data received.");
                        }
                    }
                } catch (error) {
                    if (isMounted) {
                        setRequest(null);
                        setApprovals(null);
                        setCommentable(true);
                        toast.error("An error occurred while fetching work from home details.");
                        console.error('Error fetching work from home request:', error);
                    }
                } finally {
                    if (isMounted) {
                        setIsLoadingDetails(false);
                    }
                }
            };

            doFetch();

        } else if (calendarId) { // calendarId exists but is not a valid number
            toast.error("Invalid Work From Home ID.");
            setIsLoadingDetails(false);
            setRequest(null);
            setApprovals(null);
            setCommentable(true);
        }

        return () => {
            isMounted = false;
        };
    }, [calendarId, auth.id]); // Added auth.id to dependencies for `commentable` logic


    const handleSubmitOpinion = async (status: WorkFromHomeStatus.Approved | WorkFromHomeStatus.Rejected) => {
        setIsSubmitting(true);
        try {
            if (!request?.id) {
                toast.error("Work From Home request ID is missing.");
                return;
            }

            const opinionData: ICreateWorkFromHomeOpinion = {
                workFromHomeId: request.id,
                comment: comment,
                isApproved: status === WorkFromHomeStatus.Approved,
            };

            const response = await WORK_FROM_HOME.CREATE_OPINION(opinionData);
            debugger

            if (response.error) {
                toast.error(response.message || "Failed to submit opinion.");
            } else {
                toast.success(response.message || "Opinion submitted successfully!");
            }
            // Refetch after submitting opinion to get the latest data
            const numericCalendarId = Number(calendarId);
            if (!isNaN(numericCalendarId) && numericCalendarId > 0) {
                const refetchResponse = await WORK_FROM_HOME.GET_SINGLE(numericCalendarId);
                if (isSubmitting && refetchResponse && !refetchResponse.error && refetchResponse.data) {
                    setRequest(refetchResponse.data);
                    setApprovals(refetchResponse.data.opinions ?? null);
                    const canComment = !refetchResponse.data.opinions?.some(op => op.user?.id === auth.id);
                    setCommentable(canComment);
                } else if (isSubmitting) {
                    toast.error(refetchResponse?.message || "Failed to refresh work from home details after submitting opinion.");
                }
            } else if (isSubmitting) {
                setRequest(null);
                setApprovals(null);
                setCommentable(true);
            }

            // Clear form
            setComment('');
        } catch (error) {
            toast.error("An error occurred while submitting your opinion.");
            console.error('Error submitting opinion:', error);
        } finally {
            setIsSubmitting(false);
        }
    };

    // Function to get status color based on status
    const getStatusColor = (status: WorkFromHomeStatus | boolean): string => {
        // If status is boolean (from approval.isApproved), map to string
        const statusString = typeof status === 'boolean' ? (status ? "Approved" : "Rejected") : status;
        switch (statusString) {
            case "Approved":
                return "text-green-600";
            case "Rejected":
                return "text-red-600";
            case "Pending":
            default:
                return "text-yellow-600";
        }
    };

    const overallLoading = isLoadingDetails || isSubmitting;


    return (
        <div className="bg-gray-50 min-h-screen p-6 w-full">

            {overallLoading && (
                <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-50 rounded-lg">
                    <TableLoader />
                </div>
            )}
            <div className={`max-w-5xl mx-auto ${overallLoading ? 'blur-sm pointer-events-none' : ''}`}>
                {/* Header */}
                <div className="flex items-center mb-8">
                    {/* <User className="w-6 h-6 mr-2" /> */}
                    <h1 className="text-xl font-medium">Calendar {'>'} Work From Home Request Details</h1>
                </div>

                <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
                    {/* User Info */}
                    <div className="flex justify-between items-start mb-8">
                        <div>
                            <h2 className="text-xl font-semibold">{request?.user?.name}</h2>
                            <p className="text-gray-500">{request?.user?.hrCode}</p> {/* Use HR code as an identifier */}

                            <div className="grid grid-cols-2 gap-x-16 gap-y-2 mt-4">
                                <div>
                                    <span className="text-gray-500">Status: </span>
                                    <span className="text-blue-600">Active</span> {/* Assuming user status is always active here */}
                                </div>
                                <div>
                                    <span className="text-gray-500">Department: </span>
                                    <span className="text-blue-600">{request?.user?.group?.name}</span>
                                </div>
                                <div>
                                    <span className="text-gray-500">Type: </span>
                                    <span className="text-blue-600">{request?.user?.accountType}</span> {/* Assuming AccountType is 'Internal' or similar */}
                                </div>
                                <div>
                                    <span className="text-gray-500">Access type: </span>
                                    <span className="text-blue-600">Member</span> {/* This should probably be request?.user?.role if you want to show their access type */}
                                </div>
                                <div>
                                    <span className="text-gray-500">Email: </span>
                                    <span className="text-blue-600">{request?.user?.email ?? ""}</span>
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

                        {/* Leave Balance (Keep for context, though not directly related to WFH) */}
                        <div className="flex space-x-4">
                            <div className="text-center border-r pr-6">
                                <h3 className="font-medium">Annual</h3>
                                <p className="text-blue-600 font-bold text-xl">{request?.user?.vacation?.annual} <span className="text-sm text-gray-500">/{request?.user?.vacation?.annual_MAX}</span></p>
                            </div>
                            <div className="text-center border-r pr-6">
                                <h3 className="font-medium">Sick</h3>
                                <p className="text-blue-600 font-bold text-xl">{request?.user?.vacation?.sick} </p>
                            </div>
                            <div className="text-center">
                                <h3 className="font-medium">Emergency</h3>
                                <p className="text-blue-600 font-bold text-xl">{request?.user?.vacation?.emergency} <span className="text-sm text-gray-500">/{request?.user?.vacation?.emergency_MAX}</span></p>
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
                                                ? format(new Date(request.dateCreated), "dd MMM, yyyy HH:mm") // Added HH:mm for full timestamp
                                                : "N/A"}
                                        </span>
                                    </div>
                                    {/* No attachment for WFH, so this part is removed */}
                                    {/* {request?.type == "Sick" && ... } */}

                                    {/* WFH is not a "type" enum like leave, so this might not apply */}
                                    {/* <div>
                                        <span className="text-gray-500">Type: </span>
                                        <span className="text-blue-600">{request?.type}</span>
                                    </div> */}
                                    <div>
                                        <span className="text-gray-500">Final status: </span>
                                        <span className={`${getStatusColor(request?.status ?? WorkFromHomeStatus.Pending)}`}>{request?.status}</span>
                                    </div>
                                    <div>
                                        <span className="text-gray-500">Work From Home Date: </span>
                                        <span>
                                            {request?.date
                                                ? format(new Date(request.date), "dd MMM, yyyy")
                                                : "N/A"}
                                        </span>
                                    </div>
                                    {/* WFH doesn't have a 'duration' in the same way leave does */}
                                    {/* <div>
                                        <span className="text-gray-500">Duration: </span>
                                        <span className="text-blue-600">{request?.duration} Day</span>
                                    </div> */}
                                    <div>
                                        <span className="text-gray-500">Note for Manager: </span>
                                        <span className="text-blue-600">{request?.noteForManager || "N/A"}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                {/* Opinions / Approvals */}
                <h2 className="text-lg font-semibold mt-8 mb-4">Opinions</h2>
                {approvals?.length === 0 && <p className="text-gray-500 mb-4">No opinions yet.</p>}
                {approvals?.map((approval, index) => (
                    <div key={index} className="border border-gray-200 rounded-lg p-6 mb-4" style={{ borderWidth: '2px', borderColor: 'black', borderStyle: 'solid' }}>
                        <div className="grid grid-cols-2 gap-4 mb-4">
                            <div>
                                <span className="text-gray-500">User Name: </span>
                                <span className="text-blue-600">{approval.user?.name}</span>
                            </div>
                            <div>
                                <span className="text-gray-500">Role: </span>
                                <span className="text-blue-600">
                                    {approval.user?.role === 0 ? "Project Manager" :
                                        approval.user?.role === 1 ? "Section Head" :
                                            approval.user?.role === 2 ? "Team Leader" : "Owner"}
                                </span>
                            </div>
                            <div>
                                <span className="text-gray-500">Status: </span>
                                <span className={`${getStatusColor(approval.isApproved)}`}>{approval.isApproved ? "Approved" : "Rejected"}</span>
                            </div>
                            <div>
                                <span className="text-gray-500">Date: </span>
                                <span className="text-blue-600">{format(new Date(approval.dateCreated), "dd MMM, yyyy HH:mm")}</span>
                            </div>
                        </div>
                        <div>
                            <span className="text-gray-500">Comment: </span>
                            <span className="text-blue-600">{approval.comment || "No comment provided."}</span>
                        </div>
                    </div>
                ))}
                {/* Your Opinion Section */}
                {/* Only show opinion section if the user is a manager/owner (roles < 3 or role == 4)
                    AND they haven't already given an opinion
                    AND the request status is still Pending */}
                {((auth.role < 3 || auth.role === 4) &&
                    commentable &&
                    request?.status === WorkFromHomeStatus.Pending) && (
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
                                    onClick={() => handleSubmitOpinion(WorkFromHomeStatus.Approved)}
                                    disabled={isSubmitting}
                                >
                                    Approve
                                </button>
                                <button
                                    className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50"
                                    onClick={() => handleSubmitOpinion(WorkFromHomeStatus.Rejected)}
                                    disabled={isSubmitting}
                                >
                                    Reject
                                </button>
                            </div>
                        </div>
                    )}
            </div>
        </div>
    );
};

export default WorkFromHomeRequestDetails;