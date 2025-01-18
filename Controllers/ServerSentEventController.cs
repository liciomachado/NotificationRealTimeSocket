using Microsoft.AspNetCore.Mvc;
using NotificationRealTimeSocket.Domain;
using NotificationRealTimeSocket.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NotificationRealTimeSocket.Controllers;

[Route("[controller]")]
[ApiController]
public class ServerSentEventController(
    IConnectionMultiplexer redis,
    INotificationsMongoRepository notificationsRepository)
    : ControllerBase
{
    private static readonly List<(StreamWriter strem, string channel)> _clients = new();

    [HttpGet("stream/{userId}")]
    public async Task GetNotificationsStream(string userId, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        var subscriber = redis.GetSubscriber();
        var writer = new StreamWriter(Response.Body);

        lock (_clients)
        {
            _clients.Add((writer, userId));
        }

        // Envie notificações salvas ao novo consumidor
        var notifications = await notificationsRepository.GetNotificationsByUser(userId);

        foreach (var notification in notifications)
        {
            var messageDto = new NotificationDtoWebsocket(notification.Id, notification.Message, notification.DateCreated, notification.Url, notification.Status);
            var json = JsonSerializer.Serialize(messageDto);
            if (HttpContext.RequestAborted.IsCancellationRequested) continue;
            await writer.WriteAsync($"data: {json}\n\n");
            await writer.FlushAsync(cancellationToken);
        }

        // Inscreva-se no canal Redis
        await subscriber.SubscribeAsync(userId, async (channel, message) =>
        {
            var notificationReceived = JsonSerializer.Deserialize<Notification>(message!)!;
            var jsonObject = JsonNode.Parse(message)?.AsObject()!;
            var action = jsonObject["Action"]?.ToString();

            string json;
            if (!string.IsNullOrEmpty(action) && action == "delete")
                json = message!;
            else if (!string.IsNullOrEmpty(action) && action == "update")
            {
                json = message!;
            }
            else
            {
                var messageSend = new NotificationDtoWebsocket(notificationReceived.Id,
                    notificationReceived.Message, notificationReceived.DateCreated, notificationReceived.Url, notificationReceived.Status);
                json = JsonSerializer.Serialize(messageSend);
            }

            lock (_clients)
            {
                var disconnectedClients = new List<(StreamWriter stream, string channel)>();
                foreach (var client in _clients.Where(x => x.channel == userId))
                {
                    try
                    {
                        if (!HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            client.strem.Write($"data: {json}\n\n");
                            client.strem.Flush();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        disconnectedClients.Add(client);
                    }
                }

                foreach (var disconnectedClient in disconnectedClients)
                {
                    _clients.Remove(disconnectedClient);
                    Console.WriteLine($"Conexao fechada no {userId}");
                }
            }
        });

        // Mantenha a conexão aberta
        try
        {
            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // A conexão foi cancelada
        }
        finally
        {
            lock (_clients)
            {
                _clients.Remove((writer, userId));
                Console.WriteLine($"Conexao fechada no {userId}");
            }
            await subscriber.UnsubscribeAsync(userId);
            writer.Close();
            await writer.DisposeAsync();
        }
    }
}

public record DeleteEvent(string Action, string MessageId);
