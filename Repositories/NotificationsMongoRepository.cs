using RealTimeDriverTracking.Domain;

namespace RealTimeDriverTracking.Repositories;

public class NotificationsMongoRepository : INotificationsMongoRepository
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
}