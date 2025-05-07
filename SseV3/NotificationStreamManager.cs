using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NotificationRealTimeSocket.SseV3;

public class NotificationStreamManager
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

    public void Unsubscribe(string channel, Channel<string> ch)
    {
        if (_channels.TryGetValue(channel, out var list))
        {
            list.Remove(ch);
            if (list.Count == 0)
                _channels.TryRemove(channel, out _);
        }
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
        if (_channels.TryGetValue(channel, out var list))
        {
            foreach (var ch in list)
            {
                await ch.Writer.WriteAsync(message);
            }
        }
    }
}