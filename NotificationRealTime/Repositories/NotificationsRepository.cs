using NotificationRealTime.Domain;

namespace NotificationRealTime.Repositories;

public class NotificationsRepository : INotificationsRepository
{
    private static readonly List<User> Users = [];

    public async Task<Notification> AddMessage(string userId, string message, string? url = default)
    {
        var user = Users.FirstOrDefault(x => x.Id == userId);
        if (user is not null)
        {
            var notification = user.AddNotification(message, url);
            return notification;
        }
        else
        {
            user = new User(userId);
            var notification = user.AddNotification(message, url);
            Users.Add(user);
            return notification;
        }
    }

    public async Task<List<Notification>> GetNotificationsByUser(string userId)
    {
        var user = Users.FirstOrDefault(x => x.Id == userId);
        if (user is not null)
            return user.Notifications.Where(x => !x.Read).ToList();

        return [];
    }

    public async Task DeleteMessage(string userId, string messageId)
    {
        var user = Users.First(x => x.Id == userId);
        user.SetMessageAsRead(messageId);
    }

    public async Task<Notification?> GetAsync(string userId, string messageId)
    {
        var user = Users.FirstOrDefault(x => x.Id == userId);
        if (user is null) return null;

        var notification = user.Notifications.FirstOrDefault(x => x.Id == messageId);
        if (notification is null) return null;

        return notification;
    }

    public async Task Update(string userId, Notification notification)
    {
        var user = Users.First(x => x.Id == userId);
        var notificationFound = user.Notifications.FirstOrDefault(x => x.Id == notification.Id);
        notificationFound = notification;
    }
}