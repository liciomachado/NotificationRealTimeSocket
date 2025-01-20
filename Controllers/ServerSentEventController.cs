using Microsoft.AspNetCore.Mvc;
using NotificationRealTimeSocket.Domain;
using NotificationRealTimeSocket.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NotificationRealTimeSocket.Controllers;

[ApiController]
[Route("[controller]")]
public class ServerSentEventController(
    IConnectionMultiplexer redis,
    INotificationsRepository notificationsRepository)
    : ControllerBase
{
    private static readonly List<(StreamWriter stream, string channel)> Clients = [];

    [HttpGet("stream/{channel}")]
    public async Task GetNotificationsStream(string channel, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        var subscriber = redis.GetSubscriber();
        var writer = new StreamWriter(Response.Body);

        lock (Clients)
        {
            Clients.Add((writer, channel));
        }

        // Envie notificações salvas ao novo consumidor
        var notifications = await notificationsRepository.GetNotificationsByUser(channel);

        foreach (var notification in notifications)
        {
            var messageDto = new NotificationDtoWebsocket(notification.Id, notification.Message, notification.DateCreated, notification.Url, notification.Status);
            var json = JsonSerializer.Serialize(messageDto);
            if (HttpContext.RequestAborted.IsCancellationRequested) continue;
            await writer.WriteAsync($"data: {json}\n\n");
            await writer.FlushAsync(cancellationToken);
        }

        await subscriber.SubscribeAsync(channel, Handler);

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
            lock (Clients)
            {
                Clients.Remove((writer, channel));
                Console.WriteLine($"Conexao fechada no {channel}");
            }
            await subscriber.UnsubscribeAsync(channel);
            writer.Close();
            await writer.DisposeAsync();
        }
    }

    // Inscreva-se no canal Redis
    private void Handler(RedisChannel channel, RedisValue message)
    {
        var notificationReceived = JsonSerializer.Deserialize<Notification>(message!)!;
        var jsonObject = JsonNode.Parse(message)?.AsObject()!;
        var action = jsonObject["Action"]?.ToString();

        string json;
        if (!string.IsNullOrEmpty(action) && action == "delete" || action == "update")
            json = message!;
        else
        {
            var messageSend = new NotificationDtoWebsocket(notificationReceived.Id, notificationReceived.Message, notificationReceived.DateCreated, notificationReceived.Url, notificationReceived.Status);
            json = JsonSerializer.Serialize(messageSend);
        }

        SendToActiveClients(json, channel.ToString());
    }

    private void SendToActiveClients(string data, string channel)
    {
        lock (Clients)
        {
            var disconnectedClients = new List<(StreamWriter stream, string channel)>();
            foreach (var client in Clients.Where(x => x.channel == channel))
            {
                try
                {
                    if (!HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        client.stream.WriteAsync($"data: {data}\n\n");
                        client.stream.FlushAsync();
                    }
                }
                catch (ObjectDisposedException)
                {
                    disconnectedClients.Add(client);
                }
            }

            foreach (var disconnectedClient in disconnectedClients)
            {
                Clients.Remove(disconnectedClient);
                Console.WriteLine($"Conexao fechada no {channel}");
            }
        }
    }
}

public record DeleteEvent(string Action, string MessageId);
