import { url } from "./index";
import authService from "../Auth";

// Shared response wrapper interfaces
declare interface ResponseService<T> {
  data?: T;
  error?: boolean;
  message?: string;
}

declare interface PageList<T> {
  items: T;
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  category: string;
  type: string;
  createdAt: string;
  hasActions: boolean;
  status?: string | null;
  isRead: boolean;
  relatedEntityId?: number | null;
  additionalData?: string | null;
}

export interface GetUserNotificationsParams {
  category?: string;
  timeFilter?: string;
  isRead?: boolean;
  type?: string;
  flagged?: boolean;
  page?: number;
  pageSize?: number;
}

const NOTIFICATIONS = {
  GET_UNREAD_COUNT: async (): Promise<ResponseService<number>> => {
    try {
      const headers = authService.authHeader();

      if (!headers) {
        return {
          error: true,
          message: "User is not authenticated.",
        };
      }

      const res = await fetch(`${url}/notifications/unread-count`, {
        headers: {
          "Content-Type": "application/json",
          ...headers,
        },
      });

      if (!res.ok) {
        const errorResponse = await res
          .json()
          .catch(() => ({ message: res.statusText }));

        return {
          error: true,
          message:
            errorResponse.message ||
            `Failed to fetch unread notification count: HTTP status ${res.status}`,
        };
      }

      const data: ResponseService<number> = await res.json();
      return data;
    } catch (error) {
      return {
        error: true,
        message:
          error instanceof Error
            ? error.message
            : "Failed to fetch unread notification count.",
      };
    }
  },

  GET_MY_NOTIFICATIONS: async (
    params?: GetUserNotificationsParams
  ): Promise<ResponseService<PageList<NotificationDto[]>>> => {
    try {
      const headers = authService.authHeader();

      if (!headers) {
        return {
          error: true,
          message: "User is not authenticated.",
        };
      }

      const filteredParams = Object.entries(params || {}).reduce(
        (acc, [key, value]) => {
          if (
            value !== undefined &&
            value !== null &&
            String(value).trim() !== ""
          ) {
            acc[key] = String(value);
          }
          return acc;
        },
        {} as Record<string, string>
      );

      const query =
        Object.keys(filteredParams).length > 0
          ? `?${new URLSearchParams(filteredParams).toString()}`
          : "";

      const res = await fetch(`${url}/notifications${query}`, {
        headers: {
          "Content-Type": "application/json",
          ...headers,
        },
      });

      if (!res.ok) {
        const errorResponse = await res
          .json()
          .catch(() => ({ message: res.statusText }));

        return {
          error: true,
          message:
            errorResponse.message ||
            `Failed to fetch notifications: HTTP status ${res.status}`,
        };
      }

      const data: ResponseService<PageList<NotificationDto[]>> = await res.json();
      return data;
    } catch (error) {
      return {
        error: true,
        message:
          error instanceof Error
            ? error.message
            : "Failed to fetch notifications.",
      };
    }
	  },

	  ACT_ON_NOTIFICATION: async (
	    id: number,
	    action: "accept" | "reject",
	    comment?: string
	  ): Promise<ResponseService<NotificationDto>> => {
	    try {
	      const headers = authService.authHeader();

	      if (!headers) {
	        return {
	          error: true,
	          message: "User is not authenticated.",
	        };
	      }

	      const res = await fetch(`${url}/notifications/${id}/action`, {
	        method: "POST",
	        headers: {
	          "Content-Type": "application/json",
	          ...headers,
	        },
	        body: JSON.stringify({ action, comment }),
	      });

	      if (!res.ok) {
	        const errorResponse = await res
	          .json()
	          .catch(() => ({ message: res.statusText }));

	        return {
	          error: true,
	          message:
	            errorResponse.message ||
	            `Failed to perform notification action: HTTP status ${res.status}`,
	        };
	      }

	      const data: ResponseService<NotificationDto> = await res.json();
	      return data;
	    } catch (error) {
	      return {
	        error: true,
	        message:
	          error instanceof Error
	            ? error.message
	            : "Failed to perform notification action.",
	      };
	    }
	  },
    MARK_AS_READ: async (
      id: number,
      accepted?: boolean // Use '?' for optionality
    ): Promise<ResponseService<NotificationDto>> => {
      try {
        const headers = authService.authHeader();

        if (!headers) {
          return {
            error: true,
            message: "User is not authenticated.",
          };
        }

        // 1. Build the Query String
        const queryParams = new URLSearchParams();
        if (accepted !== undefined && accepted !== null) {
            queryParams.append("accepted", accepted.toString());
        }

        // 2. Attach query string to the URL if it has content
        const queryString = queryParams.toString();
        const finalUrl = queryString 
            ? `${url}/notifications/${id}/mark-as-read?${queryString}` 
            : `${url}/notifications/${id}/mark-as-read`;

        const res = await fetch(finalUrl, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            ...headers,
          },
        });

        if (!res.ok) {
          const errorResponse = await res
            .json()
            .catch(() => ({ message: res.statusText }));

          return {
            error: true,
            message:
              errorResponse.message ||
              `Failed to update notification: HTTP status ${res.status}`,
          };
        }

        const data: ResponseService<NotificationDto> = await res.json();
        return data;
      } catch (error) {
        return {
          error: true,
          message:
            error instanceof Error
              ? error.message
              : "Failed to mark notification as read.",
        };
      }
    },
    MARK_ALL_AS_READ: async (): Promise<ResponseService<number>> => {
      try {
        const headers = authService.authHeader();

        if (!headers) {
          return {
            error: true,
            message: "User is not authenticated.",
          };
        }

        const res = await fetch(`${url}/notifications/mark-all-as-read`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            ...headers,
          },
        });

        if (!res.ok) {
          const errorResponse = await res
            .json()
            .catch(() => ({ message: res.statusText }));

          return {
            error: true,
            message:
              errorResponse.message ||
              `Failed to mark all notifications as read: HTTP status ${res.status}`,
          };
        }

        const data: ResponseService<number> = await res.json();
        return data;
      } catch (error) {
        return {
          error: true,
          message:
            error instanceof Error
              ? error.message
              : "Failed to mark all notifications as read.",
        };
      }
    },
};

// export type { NotificationDto };
export default NOTIFICATIONS;

