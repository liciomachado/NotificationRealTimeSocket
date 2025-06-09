using StackExchange.Redis;
using System.Text.Json;

const string connectionString = "localhost:6379";
var conn = ConnectionMultiplexer.Connect(connectionString);

const string key = "channel-id";
Console.WriteLine($"Iniciando consumer...");
Console.WriteLine("    1 - Stack     | 2 - FIFO     | 3 - PubSub = ");
var option = Console.ReadLine();
if (option == "1")
    await StackQueue(conn, key);
else if (option == "2")
    await FifoQueue(conn, key);
else
    await PubSubTopic(conn, key);

Console.WriteLine($"Press any key to exit.");
return;

async Task StackQueue(IConnectionMultiplexer connectionMultiplexer, string channel)
{
    Console.WriteLine("Selecionado Queue...");

    var database = connectionMultiplexer.GetDatabase();
    while (true)
    {
        var item = await database.ListLeftPopAsync(channel);  // Stack / Pilha
        if (item.HasValue)
        {
            var json = JsonSerializer.Deserialize<Item>(item!);
            Console.WriteLine($"Pushing item: {json.Id} - {json.Time} from channel: {channel}");
        }

        await Task.Delay(100);
    }
}

async Task FifoQueue(IConnectionMultiplexer connectionMultiplexer, string channel)
{
    Console.WriteLine("Selecionado FIFO...");

    var database = connectionMultiplexer.GetDatabase();
    while (true)
    {
        var item = await database.ListRightPopAsync(key);  // FIFO (First In, First Out)
        if (item.HasValue)
        {
            var json = JsonSerializer.Deserialize<Item>(item!)!;
            Console.WriteLine($"Pushing item: {json.Id} - {json.Time} from channel: {channel}");
        }

        await Task.Delay(100);
    }
}


async Task PubSubTopic(IConnectionMultiplexer connectionMultiplexer, string channel)
{
    Console.WriteLine("Selecionado PubSub...");
    Console.WriteLine("Inscrito no canal. Aguardando mensagens...");

    var sub = connectionMultiplexer.GetSubscriber();
    await sub.SubscribeAsync(channel, (ch, value) =>
    {
        var item = JsonSerializer.Deserialize<Item>(value!);
        Console.WriteLine($"[PubSub] Item recebido: Id = {item?.Id}, Time = {item?.Time:O}");
    });

    await Task.Delay(Timeout.Infinite);
}


public record Item(int Id, DateTime Time);