using System;
using AutomatedTaskSystem.Models.Enums.NotificationCategory;
using AutomatedTaskSystem.Models.Enums.NotificationStatus;
using AutomatedTaskSystem.Models.Enums.NotificationType;

namespace AutomatedTaskSystem.Models
{
	public class Notification
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public User User { get; set; } = null!;

		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;

		public NotificationCategoryEnum Category { get; set; }
		public NotificationTypeEnum Type { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public bool IsRead { get; set; } = false;

		public int? RelatedEntityId { get; set; }

		public bool HasActions { get; set; } = false;
		public NotificationStatusEnum? Status { get; set; }

		/// <summary>
		/// JSON string containing additional metadata for the notification.
		/// For task notifications, this may include projectId for proper routing.
		/// </summary>
		public string? AdditionalData { get; set; }
	}
}
