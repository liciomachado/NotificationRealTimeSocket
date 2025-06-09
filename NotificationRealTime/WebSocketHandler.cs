using NotificationRealTime.Domain;
using NotificationRealTime.Repositories;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace NotificationRealTime;

public class WebSocketHandler(IConnectionMultiplexer redis, INotificationsRepository notificationsRepository)
{
    public async Task HandleWebSocketRequest(HttpContext context, string userId)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var subscriber = redis.GetSubscriber();

        // Envie notificações salvas ao novo consumidor
        var notifications = await notificationsRepository.GetNotificationsByUser(userId);
        foreach (var notification in notifications)
        {
            var messagedtoSocket = new NotificationDtoWebsocket(notification.Id, notification.Message, notification.DateCreated, notification.Url, notification.Status);
            var bufferNotifications = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messagedtoSocket));
            await webSocket.SendAsync(new ArraySegment<byte>(bufferNotifications), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // Inscreva-se no canal Redis
        await subscriber.SubscribeAsync(userId, async (channel, message) =>
        {
            if (webSocket.State != WebSocketState.Open) return;

            var notificationReceived = JsonSerializer.Deserialize<Notification>(message!)!;
            byte[] buffer;
            if (string.IsNullOrEmpty(notificationReceived.Message))
                buffer = Encoding.UTF8.GetBytes(message.ToString());
            else
            {
                var messageSend = new NotificationDtoWebsocket(notificationReceived.Id, notificationReceived.Message, notificationReceived.DateCreated, notificationReceived.Url, notificationReceived.Status);
                buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageSend));
            }

            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        });

        // Mantenha a conexão WebSocket aberta
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket client", CancellationToken.None);
            }
        }

    }
}


public record NotificationDtoWebsocket(string Id, string Message, DateTime Date, string? Url, string Status);