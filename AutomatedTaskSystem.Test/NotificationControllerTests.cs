using AutomatedTaskSystem.Controllers;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.NotificationDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.Permission;
using AutomatedTaskSystem.Services.WorkFromHome;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.NotificationStatus;
using AutomatedTaskSystem.Models.Enums.NotificationType;
using AutomatedTaskSystem.Services.Notification;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;
using Task = System.Threading.Tasks.Task;

namespace AutomatedTaskSystem.Test;

public class NotificationControllerTests
{
    private class FakeTokenService : ITokenService
    {
        private readonly ResponseService<string> _result;

        public FakeTokenService(string? userId, bool error = false, string? message = null)
        {
            if (error || userId is null)
            {
                _result = new ResponseService<string>
                {
                    Error = true,
                    Message = message ?? "Invalid Request."
                };
            }
            else
            {
                _result = new ResponseService<string>
                {
                    Error = false,
                    Message = message ?? "User id",
                    Data = userId
                };
            }
        }

        public string CreateToken(User user) => string.Empty;

        public RefreshToken GenerateRefreshToken(User user) => new()
        {
            User = user,
            UserId = user.Id
        };

        public ResponseService<string> GetUserIdFromToken() => _result;
    }

    private class FakeNotificationService : INotificationService
    {
        private readonly List<Notification> _notifications;

        public FakeNotificationService(IEnumerable<Notification> notifications)
        {
            _notifications = notifications.ToList();
        }

        public Task<bool> ErrorNotification(string userId, string errorMessage, string? details = null)
            => Task.FromResult(true);

        public Task<bool> NotifyOwnerOfNewPendingLeaveRequest(int newLeaveRequestId)
            => Task.FromResult(true);

        public Task<bool> NotifyUserOfTaskAssignment(int assignedUserId, int taskId, int? assignedByUserId = null)
            => Task.FromResult(true);

        public Task<bool> NotifyUserOfProjectAssignment(int assignedUserId, int projectId, int? assignedByUserId = null)
            => Task.FromResult(true);

        public Task<bool> NotifyOwnerOfProjectClosed(int projectId, bool closedManually)
            => Task.FromResult(true);

        public Task<bool> NotifyOwnerOfProjectCompleted(int projectId)
            => Task.FromResult(true);

        public Task<Notification?> CreateNotification(
            int userId,
            string title,
            string message,
            NotificationCategoryEnum category,
            NotificationTypeEnum type,
            int? relatedEntityId = null,
            bool hasActions = false,
            NotificationStatusEnum? status = null,
            string? additionalData = null)
        {
            var notification = new Notification
            {
                Id = _notifications.Count == 0 ? 1 : _notifications.Max(n => n.Id) + 1,
                UserId = userId,
                Title = title,
                Message = message,
                Category = category,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = relatedEntityId,
                HasActions = hasActions,
                Status = status
            };

            _notifications.Add(notification);
            return Task.FromResult<Notification?>(notification);
        }

        public Task<List<Notification>> GetUserNotifications(
            int userId,
            NotificationCategoryEnum? category = null,
            NotificationTimeRange? timeFilter = null,
            bool? isRead = null)
        {
            var query = _notifications.Where(n => n.UserId == userId);

            if (category.HasValue)
            {
                query = query.Where(n => n.Category == category.Value);
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            // In-memory ordering to mimic production behaviour (newest first)
            query = query.OrderByDescending(n => n.CreatedAt);

            return Task.FromResult(query.ToList());
        }

        public Task<bool> MarkAsRead(int notificationId, int userId, bool? accepted = null)
            => Task.FromResult(true);

        public Task<bool> UpdateNotificationStatus(int notificationId, NotificationStatusEnum status)
            => Task.FromResult(true);
    }

    private static NotificationController CreateController(
        INotificationService notificationService,
        ITokenService tokenService)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotificationController(
            notificationService,
            tokenService,
            new DataContext(options),
            Mock.Of<ILeaveRequestService>(),
            Mock.Of<IPermissionService>(),
            Mock.Of<IWorkFromHomeService>());
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsPagedList_ForAuthenticatedUser()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var notifications = new[]
        {
            new Notification
            {
                Id = 1,
                UserId = 1,
                Title = "Old",
                Message = "Old notification",
                Category = NotificationCategoryEnum.General,
                Type = NotificationTypeEnum.System,
                CreatedAt = now.AddDays(-1)
            },
            new Notification
            {
                Id = 2,
                UserId = 1,
                Title = "New",
                Message = "Newest notification",
                Category = NotificationCategoryEnum.General,
                Type = NotificationTypeEnum.System,
                CreatedAt = now
            },
            new Notification
            {
                Id = 3,
                UserId = 2,
                Title = "Other user",
                Message = "Should be filtered out",
                Category = NotificationCategoryEnum.General,
                Type = NotificationTypeEnum.System,
                CreatedAt = now
            }
        };

        var controller = CreateController(
            new FakeNotificationService(notifications),
            new FakeTokenService("1"));

        // Act
        var result = await controller.GetMyNotifications(
            category: null,
            timeFilter: null,
            isRead: null,
            page: 1,
            pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseService<PageList<GetNotificationDto>>>(okResult.Value);

        Assert.False(response.Error);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.totalCount);
        Assert.Equal(1, response.Data.page);
        Assert.Equal(10, response.Data.pageSize);
        Assert.Equal(1, response.Data.totalPages);
        Assert.Equal(2, response.Data.items.Count);

        // Newest notification should come first
        Assert.Equal("New", response.Data.items[0].Title);
        Assert.Equal("Old", response.Data.items[1].Title);
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsBadRequest_WhenTokenServiceReturnsError()
    {
        var controller = CreateController(
            new FakeNotificationService(Array.Empty<Notification>()),
            new FakeTokenService(userId: null, error: true, message: "Invalid Request."));

        var result = await controller.GetMyNotifications(
            category: null,
            timeFilter: null,
            isRead: null,
            page: 1,
            pageSize: 10);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errorBody = Assert.IsType<BaseResponseService>(badRequest.Value);

        Assert.True(errorBody.Error);
        Assert.Equal("Invalid Request.", errorBody.Message);
    }
}

