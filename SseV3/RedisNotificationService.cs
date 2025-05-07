using StackExchange.Redis;

namespace NotificationRealTimeSocket.SseV3;

public class RedisNotificationService(IConnectionMultiplexer redis, NotificationStreamManager streamManager) : BackgroundService
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