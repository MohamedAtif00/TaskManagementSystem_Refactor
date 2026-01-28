using System.Security.Claims;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Dtos.NotificationDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.NotificationStatus;
using AutomatedTaskSystem.Models.Enums.NotificationType;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.Permission;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.WorkFromHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Controllers
{
    [Route("notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ITokenService _tokenService;
        private readonly DataContext _dataContext;
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkFromHomeService _workFromHomeService;

        public NotificationController(
            INotificationService notificationService,
            ITokenService tokenService,
            DataContext dataContext,
            ILeaveRequestService leaveRequestService,
            IPermissionService permissionService,
            IWorkFromHomeService workFromHomeService)
        {
            _notificationService = notificationService;
            _tokenService = tokenService;
            _dataContext = dataContext;
            _leaveRequestService = leaveRequestService;
            _permissionService = permissionService;
            _workFromHomeService = workFromHomeService;
        }

        /// <summary>
        /// Get notifications for the currently authenticated user with optional filters and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] NotificationCategoryEnum? category,
            [FromQuery] NotificationTimeRange? timeFilter,
            [FromQuery] bool? isRead,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = authRes.Message
                });
            }

            if (!int.TryParse(authRes.Data, out var userId))
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Invalid user id in token."
                });
            }

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var notifications = await _notificationService.GetUserNotifications(
                userId,
                category,
                timeFilter,
                isRead);

            var totalCount = notifications.Count;

            var pagedItems = notifications
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new GetNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Category = n.Category.ToString(),
                    Type = n.Type.ToString(),
                    CreatedAt = n.CreatedAt.ToString("o"),
                    HasActions = n.HasActions,
                    Status = n.Status?.ToString(),
                    IsRead = n.IsRead,
                    RelatedEntityId = n.RelatedEntityId,
                    AdditionalData = n.AdditionalData
                })
                .ToList();

            var pageList = new PageList<GetNotificationDto>(
                pagedItems,
                page,
                pageSize,
                totalCount);

            var response = new ResponseService<PageList<GetNotificationDto>>
            {
                Error = false,
                Message = "User notifications",
                Data = pageList
            };

            return Ok(response);
        }

        /// <summary>
        /// Act on an actionable notification for the current user (Accept / Reject).
        /// This will delegate to the appropriate domain service (Leave, Permission, Work From Home)
        /// based on the notification's category, type, and related entity id.
        /// </summary>
        [HttpPost("{id:int}/action")]
        public async Task<IActionResult> ActOnNotification(int id, [FromBody] NotificationActionDto request)
        {
            if (request is null)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Request body is required."
                });
            }

            var authRes = _tokenService.GetUserIdFromToken();
            if (authRes.Error)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = authRes.Message
                });
            }

            if (!int.TryParse(authRes.Data, out var userId))
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Invalid user id in token."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Action))
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Action is required."
                });
            }

            var normalizedAction = request.Action.Trim().ToLowerInvariant();
            bool isApproved;

            if (normalizedAction is "accept" or "approved" or "approve")
            {
                isApproved = true;
            }
            else if (normalizedAction is "reject" or "rejected" or "decline" or "declined")
            {
                isApproved = false;
            }
            else
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Unsupported action. Only 'accept' or 'reject' are allowed."
                });
            }

            // Ensure the notification belongs to the current user and is actionable
            var notification = await _dataContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification is null)
            {
                return NotFound(new BaseResponseService
                {
                    Error = true,
                    Message = "Notification not found for the current user."
                });
            }

            if (!notification.HasActions)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "This notification does not support actions."
                });
            }

            if (notification.Status is not null && notification.Status != NotificationStatusEnum.Pending)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "This notification has already been processed."
                });
            }

            if (notification.RelatedEntityId is null)
            {
                return BadRequest(new BaseResponseService
                {
                    Error = true,
                    Message = "Notification is missing the related entity identifier."
                });
            }

            var relatedId = notification.RelatedEntityId.Value;

            // Delegate to the appropriate domain service based on Category/Type
            switch (notification.Category)
            {
                case NotificationCategoryEnum.Leaves:
                    {
                        if (notification.Type != NotificationTypeEnum.Leave)
                        {
                            return BadRequest(new BaseResponseService
                            {
                                Error = true,
                                Message = "Unsupported notification type for Leaves category."
                            });
                        }

                        // Leave requests and Work From Home requests both use Category=Leaves and Type=Leave.
                        // Distinguish them by checking which entity exists.
                        var isLeaveRequest = await _dataContext.LeaveRequests
                            .AnyAsync(l => l.Id == relatedId);

                        if (isLeaveRequest)
                        {
                            var opResult = await _leaveRequestService.GiveOpinion(new CreateOpinionDto
                            {
                                LeaveRequestId = relatedId,
                                IsApproved = isApproved,
                                Comment = request.Comment
                            });

                            if (opResult.error)
                            {
                                return BadRequest(new BaseResponseService
                                {
                                    Error = true,
                                    Message = opResult.Message
                                });
                            }
                        }
                        else
                        {
                            var isWorkFromHome = await _dataContext.WorkFromHomeRequests
                                .AnyAsync(w => w.Id == relatedId);

                            if (isWorkFromHome)
                            {
                                var opResult = await _workFromHomeService.GiveWorkFromHomeOpinion(
                                    new Dtos.WorkFromHomeDtos.CreateWorkFromHomeOpinionDto
                                    {
                                        WorkFromHomeId = relatedId,
                                        IsApproved = isApproved,
                                        Comment = request.Comment
                                    });

                                if (opResult.error)
                                {
                                    return BadRequest(new BaseResponseService
                                    {
                                        Error = true,
                                        Message = opResult.Message
                                    });
                                }
                            }
                            else
                            {
                                return NotFound(new BaseResponseService
                                {
                                    Error = true,
                                    Message = "Related leave or work-from-home request was not found."
                                });
                            }
                        }

                        break;
                    }

                case NotificationCategoryEnum.General:
                    {
                        if (notification.Type != NotificationTypeEnum.System)
                        {
                            return BadRequest(new BaseResponseService
                            {
                                Error = true,
                                Message = "Unsupported notification type for General category."
                            });
                        }

                        var opResult = await _permissionService.ApproveOrRejectPermissionAsync(
                            relatedId,
                            isApproved,
                            request.Comment);

                        if (opResult.error)
                        {
                            return BadRequest(new BaseResponseService
                            {
                                Error = true,
                                Message = opResult.Message
                            });
                        }

                        break;
                    }

                default:
                    return BadRequest(new BaseResponseService
                    {
                        Error = true,
                        Message = "This type of notification cannot be acted upon."
                    });
            }

            // If we reach this point, the domain operation succeeded. Update notification status and mark as read.
            var newStatus = isApproved ? NotificationStatusEnum.Accepted : NotificationStatusEnum.Declined;
            await _notificationService.UpdateNotificationStatus(notification.Id, newStatus);
            await _notificationService.MarkAsRead(notification.Id, userId);

            // Re-load the updated notification to return to the client
            var updated = await _dataContext.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
            if (updated is null)
            {
                return Ok(new ResponseService<GetNotificationDto>
                {
                    Error = false,
                    Message = "Action completed successfully, but the updated notification could not be reloaded.",
                    Data = null
                });
            }

            var dto = new GetNotificationDto
            {
                Id = updated.Id,
                Title = updated.Title,
                Message = updated.Message,
                Category = updated.Category.ToString(),
                Type = updated.Type.ToString(),
                CreatedAt = updated.CreatedAt.ToString("o"),
                HasActions = updated.HasActions,
                Status = updated.Status?.ToString(),
                IsRead = updated.IsRead,
                RelatedEntityId = updated.RelatedEntityId,
                AdditionalData = updated.AdditionalData
            };

            var response = new ResponseService<GetNotificationDto>
            {
                Error = false,
                Message = isApproved
                    ? "Notification action processed successfully (Accepted)."
                    : "Notification action processed successfully (Rejected).",
                Data = dto
            };

            return Ok(response);
        }

        [HttpPut("{notificationId:int}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int notificationId,[FromQuery] bool accepted)
            => Ok(await _notificationService.MarkAsRead(notificationId: notificationId,Convert.ToInt32( User.FindFirst(ClaimTypes.NameIdentifier.ToString())?.Value),accepted));
        
    }
}

