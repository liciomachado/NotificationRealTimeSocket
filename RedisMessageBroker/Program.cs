using StackExchange.Redis;
using System.Text.Json;

const string connectionString = "localhost:6379";
var conn = ConnectionMultiplexer.Connect(connectionString);
var database = conn.GetDatabase();
var sub = conn.GetSubscriber();

const string key = "channel-id";
Console.WriteLine($"Iniciando publisher...");
Console.WriteLine("1 - FIFO/Stack | 2 - PubSub = ");
var option = Console.ReadLine();
if (option == "1")
    await FifoPublisher(database, key);
else
    await PubSubPublisher(sub, key);

Console.WriteLine($"Items pushed to channel: {key}. Press any key to exit.");
return;

async Task FifoPublisher(IDatabase database1, string channel)
{
    for (var i = 0; i < 100; i++)
    {
        Console.ReadLine();
        var item = new Item(i, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(item);
        Console.WriteLine($"Pushing item: {json} to channel: {channel}");
        await database1.ListLeftPushAsync(channel, json);
    }
}

async Task PubSubPublisher(ISubscriber subscriber, string channel)
{
    for (var i = 0; i < 100; i++)
    {
        Console.ReadLine();
        var item = new Item(i, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(item);
        Console.WriteLine($"Pushing item: {json} to channel: {channel}");
        await subscriber.PublishAsync(channel, json);
    }
}

public record Item(int Id, DateTime Time);