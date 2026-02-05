using RC.Application.DTOs.Common;
using RC.Application.Interfaces.Repositories;
using RC.Application.Interfaces.Services;
using RC.Domain.Entities;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendNotificationAsync(string userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.InApp,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(userId);

                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                });

                return ApiResponse<IEnumerable<NotificationDto>>.SuccessResponse(notificationDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<NotificationDto>>.ErrorResponse($"Error retrieving notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error marking notification: {ex.Message}");
            }
        }
    }

}
