namespace NotificationRealTimeSocket.Domain;

public class User(string id)
{
    public string Id { get; set; } = id;
    public List<Notification> Notifications { get; set; } = [];

    public Notification AddNotification(string message, string? url = null)
    {
        var newNotification = new Notification(message, url);
        Notifications.Add(newNotification);
        return newNotification;
    }

    public void SetMessageAsRead(string messageId)
    {
        var notification = Notifications.First(x => x.Id == messageId);
        notification.Read = true;
    }
}

public class Notification(string message, string? url = null)
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Message { get; set; } = message;
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public bool Read { get; set; }
    public string? Url { get; set; } = url;
    public string Status { get; set; } = "Processing";
}