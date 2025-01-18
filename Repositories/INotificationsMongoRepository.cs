using NotificationRealTimeSocket.Domain;

namespace NotificationRealTimeSocket.Repositories;

public interface INotificationsMongoRepository
{
    Task<Notification> AddMessage(string userId, string message, string? url = default);

    Task<List<Notification>> GetNotificationsByUser(string userId);
    Task DeleteMessage(string userId, string messageId);
    Task<Notification?> GetAsync(string userId, string messageId);
    Task Update(string userId, Notification notification);
}