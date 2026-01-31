import { NextPage } from "next";
import Head from "next/head";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/router";
import NotificationIcon from "../../assets/Icons/Notification";
import API from "../../lib/API";
import { formatDistanceToNow } from "date-fns";
import { useAppSelector } from "../../app/hooks";
import { LeaveRequestStatus } from "../../lib/API/Leave";
import { toast } from "react-toastify";
import { PermissionOpinion } from "../../lib/API/Permission";

// --- Types ---

type NotificationCategory = "All" | "Unread" | "Leaves" | "Work Updates";

type NotificationType = "project" | "sprint" | "leave" |"Task" |"system";

type TimeFilterKey = "last_7_days" | "last_30_days" | "last_90_days" | "all_time";



interface NotificationItem {
	id: number;
	category: NotificationCategory;
	type: NotificationType;
	title: string;
	message: string;
	createdAt: string; // human readable e.g. "36 mins ago"
	hasActions?: boolean;
	status?: "pending" | "accepted" | "declined";
	isRead: boolean;
	relatedEntityId?: number | null;
	additionalData?: string | null;
}
 

const mapCategoryToApi = (category: NotificationCategory): string | undefined => {
    switch (category) {
        case "Leaves":
            return "Leaves";
        case "Work Updates":
            return "WorkUpdates";
        case "Unread":
            return "Unread";
        case "All":
        default:
            return undefined;
    }
};

let mapIsReadToApi = null;

const mapTimeFilterToApi = (filter: TimeFilterKey): string => {
    switch (filter) {
        case "last_7_days":
            return "Last7Days";
        case "last_30_days":
            return "Last30Days";
        case "last_90_days":
            return "Last90Days";
        case "all_time":
        default:
            return "AllTime";
    }
};

const mapCategoryFromApi = (category: string): NotificationCategory => {
    switch (category) {
        case "Leaves":
            return "Leaves";
        case "WorkUpdates":
            return "Work Updates";
        default:
            return "All";
    }
};

const mapTypeFromApi = (type: string): NotificationType => {
    switch (type) {
        case "Project":
            return "project";
        case "Sprint":
            return "sprint";
        case "Leave":
            return "leave";
        case "Task":
            return "Task";
        case "System":
        default:
            return "system";
    }
};

const mapStatusFromApi = (
    status?: string | null
): "pending" | "accepted" | "declined" | undefined => {
    if (!status) return undefined;
    switch (status) {
        case "Pending":
            return "pending";
        case "Accepted":
            return "accepted";
        case "Declined":
            return "declined";
        default:
            return undefined;
    }
};


const timeFilters: { key: TimeFilterKey; label: string }[] = [
    { key: "last_7_days", label: "Last 7 days" },
    { key: "last_30_days", label: "Last 30 days" },
    { key: "last_90_days", label: "Last 90 days" },
    { key: "all_time", label: "All time" },
];

const NotificationsPage: NextPage = () => {
    const [activeCategory, setActiveCategory] = useState<NotificationCategory>("All");
    const [activeTimeFilter, setActiveTimeFilter] = useState<TimeFilterKey>("last_30_days");
    const [actionState, setActionState] = useState<
        Record<number, "pending" | "accepted" | "declined">
    >({});
	const [notifications, setNotifications] = useState<NotificationItem[]>([]);
	const [loading, setLoading] = useState<boolean>(false);
	const [error, setError] = useState<string | null>(null);
	const [actionError, setActionError] = useState<string | null>(null);
	const [actionLoading, setActionLoading] = useState<Record<number, boolean>>({});
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	const categories: NotificationCategory[] = [
		"All",
		"Unread",
		"Leaves",
		"Work Updates",
	];

	const getNotificationTarget = (n: NotificationItem): string | null => {
        
		// If we have a related entity id, try to route to a specific detail page
		if (n.relatedEntityId != null) {
			switch (n.type) {
				case "project":
					return  `/projects/${n.relatedEntityId}`;
				case "sprint":
					return `/sprints/${n.relatedEntityId}`;
                case "Task":
					// Parse additionalData to get projectId for proper routing
					if (n.additionalData) {
						try {
							const additionalData = JSON.parse(n.additionalData);
							if (additionalData.projectId) {
								return `/tasks/${additionalData.projectId}/board?taskId=${n.relatedEntityId}`;
							}
						} catch (e) {
							console.error("Failed to parse notification additionalData:", e);
						}
					}
					// Fallback to user-tasks if we don't have the required data
					return `/tasks/${n.relatedEntityId}`;
				case "leave": {
					const text = `${n.title} ${n.message}`.toLowerCase();
					// Current persistent leave notifications are for Work From Home requests
					if (text.includes("work from home")) {
						return `/calendar/workFromHome/${n.relatedEntityId}`;
					}
					// Fallback to generic leave page
					return "/myleave";
				}
				default:
					break;
			}
		}

		// Fallback routes by type to relevant sections
		switch (n.type) {
			case "project":
				return "/project-overview";
			case "sprint":
				return "/sprints";
			case "leave":
				return "/myleave";
			case "Task":
				return "/user-tasks";
			default:
				return null;
		}
	};

	const handleNotificationClick = (n: NotificationItem) => {
		const target = getNotificationTarget(n);
		if (!target) return;
		handleMarkAsRead(n.id);
		router.push(target);
	};

	const handleMarkAsRead = async (id: number,accepted?:boolean) => {
		try {
			
			const res = await API.NOTIFICATIONS.MARK_AS_READ(id,accepted);

			if (!res) {
				// setActionError(
				// 	res?.message ||
				// 		`Failed to mark notification as read.`
				// );
				toast.error( `Failed to mark notification as read.`)
				return;
			}

			setNotifications((prev) =>
				prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
			);
		} catch (err: any) {
			setActionError(
				err?.message ||
					`Failed to mark notification as read.`
			);
		} finally {
			setActionLoading((prev) => ({ ...prev, [id]: false }));
		}
	};	

    useEffect(() => {
        let isMounted = true;

        const fetchNotifications = async () => {
            setLoading(true);
            setError(null);

            try {
                const categoryParam = mapCategoryToApi(activeCategory);
                // const isReadParam = mapIsReadToApi(activeRead)
                const timeFilterParam = mapTimeFilterToApi(activeTimeFilter);

                const res = await API.NOTIFICATIONS.GET_MY_NOTIFICATIONS({
                    category: categoryParam != "Unread"? categoryParam : undefined,
                    timeFilter: timeFilterParam,
                    isRead: categoryParam == "Unread"? false : undefined,
                    page: 1,
                    pageSize: 20
                });

                if (!isMounted) return;

                if (!res || res.error || !res.data) {
                    setError(res?.message || "Failed to load notifications.");
                    setNotifications([]);
                    return;
                }

		        const items = res.data.items || [];
		        const mapped: NotificationItem[] = items.map((n: any) => ({
		            id: n.id,
		            title: n.title,
		            message: n.message,
		            category: mapCategoryFromApi(n.category),
		            type: mapTypeFromApi(n.type),
		            createdAt: formatDistanceToNow(new Date(n.createdAt), {
		                addSuffix: true,
		            }),
		            hasActions: n.hasActions,
		            status: mapStatusFromApi(n.status),
		            isRead: n.isRead,
		            relatedEntityId: n.relatedEntityId ?? null,
		            additionalData: n.additionalData ?? null,
		        }));

                setNotifications(mapped);
            } catch (err: any) {
                if (!isMounted) return;
                setError(err?.message || "Failed to load notifications.");
                setNotifications([]);
            } finally {
                if (isMounted) {
                    setLoading(false);
                }
            }
        };

        fetchNotifications();

        return () => {
            isMounted = false;
        };
    }, [activeCategory, activeTimeFilter]);

   const visibleNotifications = useMemo(() => {
    let data = [...notifications];

    // Handle "Unread" separately since it's based on isRead property, not category
    if (activeCategory === "Unread") {
        data = data.filter((n) => !n.isRead);
    } else if (activeCategory !== "All") {
        data = data.filter((n) => n.category === activeCategory);
    }

    return data.map((n) => ({
        ...n,
        status: actionState[n.id] ?? n.status ?? "pending",
    }));
}, [activeCategory, actionState, notifications]);

	    const handleNotificationAction = async (
		id: number,
		action: "accept" | "reject"
		) => {
		if (auth.role === 3) {
			setActionError("You are not authorized to take action on this notification.");
			return;
		}

		// Find the notification to check its status
		const notification = notifications.find(n => n.id === id);
		
		// Prevent action if not pending
		if (notification?.status !== "pending") {
			setActionError("This request has already been processed.");
			return;
		}

		setActionError(null);
		setActionLoading((prev) => ({ ...prev, [id]: true }));

		try {
			// Find the notification to determine its type
			const notification = notifications.find(n => n.id === id);
			
			if (!notification) {
			setActionError("Notification not found.");
			return;
			}

			// Determine the status based on action
			const status = action === "accept" 
			? LeaveRequestStatus.Approved 
			: LeaveRequestStatus.Rejected;

			let res;

			// Handle based on notification type
			if (notification.type === "leave") {
			// Check if it's a Work From Home request or regular leave
			const isWorkFromHome = notification.message.toLowerCase().includes("work from home") ||
									notification.title.toLowerCase().includes("work from home");

			if (notification.relatedEntityId) {
				// Handle regular leave request
				const leaveOpinion: IOpinion = {
				leaveRequestId: notification.relatedEntityId,
				comment: action === "accept" ? "Approved" : "Rejected",
				status: status,
				isApproved: action === "accept",
				user: {
					id: auth.id,
					name: auth.name,
					role: auth.role
				}
				};

				res = await API.LEAVE.CREATE_OPINION(leaveOpinion);
			} else {
				setActionError("Invalid notification data.");
				return;
			}
			} else {
			// For other notification types, you can add specific handling here
			setActionError("This notification type doesn't support actions yet.");
			return;
			}
			
			// Handle response
			if (!res || res.error || !res.data) {
			// setActionError(
			// 	res?.message || 
			// 	`Failed to ${action === "accept" ? "accept" : "reject"} notification.`
				toast.error(res?.message || `Failed to ${action === "accept" ? "accept" : "reject"} notification.`)
			// );
			return;
			}else{
				toast.success(res.message);
				handleMarkAsRead(id,action === "accept" ? true : false);
			}

			// Update notification state
			const mappedStatus = action === "accept" ? "accepted" : "declined";
			
			setNotifications((prev) =>
			prev.map((n) =>
				n.id === id
				? {
					...n,
					hasActions: false, // Remove actions after processing
					status: mappedStatus,
					}
				: n
			)
			);

			setActionState((prev) => ({
			...prev,
			[id]: mappedStatus,
			}));

		} catch (err: any) {
			setActionError(
			err?.message ||
			`Failed to ${action === "accept" ? "accept" : "reject"} notification.`
			);
		} finally {
			setActionLoading((prev) => ({ ...prev, [id]: false }));
		}
		};
    

    return (
        <div className="grow overflow-y-auto bg-slate-100">
            <Head>
                <title>ATS - Notifications</title>
                <meta name="description" content="Notifications center for ATS" />
            </Head>

            <div className="mx-auto max-w-5xl  px-4 sm:px-6 lg:px-8">
                {/* Header */}
                <div className="bg-white border border-gray-200 rounded-t-md px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-2">
                        <div className="w-5 h-5 text-gray-700">
                            <NotificationIcon color="#111827" />
                        </div>

	                {actionError && (
	                    <div className="bg-white border border-red-200 border-t-0 rounded-b-md px-6 py-2 text-center text-red-600 text-xs">
	                        {actionError}
	                    </div>
	                )}
                        <h1 className="text-xl font-semibold text-gray-900">Notifications</h1>
                    </div>
                </div>

                {/* Filters bar */}
                <div className="bg-inherit border-x border-b border-gray-200 px-6 pt-2 pb-3 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                    <div className="flex flex-wrap gap-4 text-sm font-medium">
                        {categories.map((cat) => (
                            <button
                                key={cat}
                                onClick={() => setActiveCategory(cat)}
                                className={`pb-1 border-b-2 transition-colors ${
                                    activeCategory === cat
                                        ? "border-blue-500 text-blue-600"
                                        : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
                                }`}
                            >
                                {cat}
                            </button>
                        ))}
                    </div>

                    <div className="flex items-center justify-end">
                        <div className="relative inline-block text-left">
                            <select
                                value={activeTimeFilter}
                                onChange={(e) =>
                                    setActiveTimeFilter(e.target.value as TimeFilterKey)
                                }
                                className="appearance-none pl-3 pr-9 py-1.5 text-sm font-medium border border-blue-300 rounded-md bg-inherit text-gray-700  focus:outline-none focus:ring-2  cursor-pointer"
                            >
                                {timeFilters.map((tf) => (
                                    <option key={tf.key} value={tf.key}>
                                        {tf.label}
                                    </option>
                                ))}
                            </select>
                            <span className="pointer-events-none absolute inset-y-0 right-2 flex items-center text-gray-400">
                                <svg width="11" height="5" viewBox="0 0 11 5" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path fill-rule="evenodd" clip-rule="evenodd" d="M0.119628 0.174623C0.16238 0.124741 0.214543 0.0837735 0.273135 0.0540611C0.331727 0.0243486 0.395601 0.00647388 0.461105 0.00145893C0.526608 -0.00355601 0.592459 0.00438726 0.654891 0.0248341C0.717324 0.045281 0.775115 0.0778305 0.824962 0.120623L5.1663 3.84129L9.50763 0.120623C9.60884 0.0393757 9.73769 0.000671316 9.86693 0.0126919C9.99616 0.0247126 10.1157 0.0865161 10.2002 0.185037C10.2846 0.283559 10.3275 0.411077 10.3197 0.540634C10.3119 0.670191 10.254 0.791634 10.1583 0.87929L5.49163 4.87929C5.40102 4.95693 5.28562 4.99961 5.1663 4.99961C5.04697 4.99961 4.93157 4.95693 4.84096 4.87929L0.174295 0.87929C0.0737515 0.792967 0.0115881 0.670261 0.00146281 0.538132C-0.00866252 0.406003 0.0340785 0.275258 0.120295 0.174623" fill="#3B82F6"/>
                                </svg>
                            </span>
                        </div>
                    </div>
                </div>

                {/* Notifications list */}
	                <div className="bg-slate-100 pt-4">
	                    {loading ? (
	                        <div className="bg-white border border-gray-200 rounded-md px-6 py-6 text-center text-gray-500 text-sm">
	                            Loading notifications...
	                        </div>
	                    ) : error ? (
	                        <div className="bg-white border border-red-200 rounded-md px-6 py-6 text-center text-red-600 text-sm">
	                            {error}
	                        </div>
	                    ) : visibleNotifications.length === 0 ? (
	                        <div className="bg-white border border-dashed border-gray-300 rounded-md px-6 py-12 text-center text-gray-500 text-sm">
	                            No notifications to show for the selected filters.
	                        </div>
	                    ) : (
	                        <div className="space-y-3">
                            {visibleNotifications.map((n) => {
                                const isLeave = n.type === "leave";
                                // const isProjectOrSprint = n.type === "project" || n.type === "sprint";

                                // const isSeen = n.isRead;

                                const bgClass = !n.isRead
                                    ? "bg-white":"bg-slate-50 text-gray-100"

                                const textClass = !n.isRead
                                    ? "text-gray-900":"text-slate-300"

                                const messageClass = !n.isRead ? "text-gray-900":"text-slate-300"

		                                return (
		                                    <div
		                                        key={n.id}
		                                        onClick={getNotificationTarget(n) ? () => handleNotificationClick(n) : undefined}
		                                        className={`${bgClass} border border-gray-200 rounded-md px-6 py-4 flex flex-col gap-2 hover:shadow-sm transition-shadow ${
		                                            getNotificationTarget(n) ? "cursor-pointer" : ""
		                                        }`}
		                                    >
                                                <div className={`flex items-center justify-between text-xs ${textClass}`} >
                                                    <span className={`font-medium ${textClass}`}>
                                                        {n.title}
                                                    </span>
                                                    <span>{n.createdAt}</span>
                                                </div>
                                                <div className={`text-sm ${messageClass} font-medium`}>
                                                    {n.message}
                                                </div>

		                                        {n.hasActions && n.status == "pending" && auth.role !== 3 && (
													<div className="mt-3 flex justify-end gap-3">
														<button
														onClick={(e) => {
															e.stopPropagation();
															handleNotificationAction(n.id, "reject");
														}}
														disabled={
															!!actionLoading[n.id] ||
															n.status !== "pending"
														}
														className={`px-4 py-1.5 text-sm font-semibold rounded-full border transition-colors ${							
															 actionLoading[n.id]
															? "bg-red-50 text-red-500 border-red-200 opacity-60 cursor-not-allowed"
															: "bg-red-50 text-red-500 border-red-200 hover:bg-red-100"
														}`}
														>
														{actionLoading[n.id] ? "Processing..." :"Decline"}
														</button>
														<button
														onClick={(e) => {
															e.stopPropagation();
															handleNotificationAction(n.id, "accept");
														}}
														disabled={
															!!actionLoading[n.id] ||
															n.status !== "pending"
														}
														className={`px-4 py-1.5 text-sm font-semibold rounded-full border transition-colors ${
															n.status === "accepted"
															? "bg-green-100 text-green-600 border-green-300 cursor-not-allowed"
															: n.status !== "pending"
															? "bg-gray-100 text-gray-400 border-gray-300 cursor-not-allowed"
															: actionLoading[n.id]
															? "bg-blue-500 text-white border-blue-500 opacity-60 cursor-not-allowed"
															: "bg-blue-500 text-white border-blue-500 hover:bg-blue-600"
														}`}
														>
														{actionLoading[n.id] ? "Processing..." : n.status === "accepted" ? "Accepted" : "Accept"}
														</button>
													</div>
													)}
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default NotificationsPage;

