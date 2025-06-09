using StackExchange.Redis;
using System.Text.Json;

namespace NotificationRealTime.Services;


public interface IPubSubService
{
    Task PublishAsync<T>(string channel, T request, string eventName);
}

public sealed class PubSubService(IConnectionMultiplexer redis) : IPubSubService
{
    public async Task PublishAsync<T>(string channel, T request, string eventName)
    {
        var subscriber = redis.GetSubscriber();
        var json = JsonSerializer.Serialize(new PublisherDto<T>(eventName, request));
        await subscriber.PublishAsync(channel, json);
    }
}

public class RedisNotificationService(IConnectionMultiplexer redis, INotificationStreamManager streamManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = redis.GetSubscriber();
        await sub.SubscribeAsync("*", async (channel, value) =>
        {
            Console.WriteLine($"Received message: {value} on channel: {channel}");
            await streamManager.BroadcastAsync(channel, value);
        });
    }
}