using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace NotificationRealTime.Services;

public interface INotificationStreamManager
{
    ChannelReader<string> Subscribe(string channel);
    void Unsubscribe(string channel, ChannelReader<string> reader);
    Task BroadcastAsync(string channel, string message);
    Task PublishAsync<T>(string channel, T request, string eventName);
}

public class PublisherDto<T>(string eventName, T data)
{
    public string EventName { get; init; } = eventName;
    public T Data { get; init; } = data;
}

public class NotificationStreamManager : INotificationStreamManager
{
    private readonly ConcurrentDictionary<string, List<Channel<string>>> _channels = new();

    public ChannelReader<string> Subscribe(string channel)
    {
        var channelStream = Channel.CreateUnbounded<string>();
        _channels.AddOrUpdate(
            channel,
            _ => [channelStream],
            (_, list) => { list.Add(channelStream); return list; });

        return channelStream.Reader;
    }

    public void Unsubscribe(string channel, ChannelReader<string> reader)
    {
        if (_channels.TryGetValue(channel, out var list))
        {
            lock (list)
            {
                list.RemoveAll(ch => ch.Reader == reader);
                if (list.Count == 0)
                    _channels.TryRemove(channel, out _);
            }
        }
    }

    public async Task BroadcastAsync(string channel, string message)
    {
        Console.WriteLine($"Received message: {message} on channel: {channel}");
        if (_channels.TryGetValue(channel, out var list))
        {
            foreach (var ch in list)
            {
                await ch.Writer.WriteAsync(message);
            }
        }
    }

    public async Task PublishAsync<T>(string channel, T request, string eventName)
    {
        var json = JsonSerializer.Serialize(new PublisherDto<T>(eventName, request));
        await BroadcastAsync(channel, json);
    }
}