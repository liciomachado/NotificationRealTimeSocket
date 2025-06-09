using Microsoft.AspNetCore.Mvc;
using NotificationRealTimeSocket.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace NotificationRealTimeSocket.SseV3;

[Route("api/[controller]")]
[ApiController]
public class RastreamentoEntregaController(INotificationStreamManager streamManager, IConnectionMultiplexer redis) : ControllerBase
{
    [HttpGet("{channel}")]
    public async Task GetStream(string channel, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");

        var reader = streamManager.Subscribe(channel);
        try
        {
            await foreach (var message in reader.ReadAllAsync(cancellationToken))
            {
                await Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            streamManager.Unsubscribe(channel, reader);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LocalizationRequest request)
    {
        var subscriber = redis.GetSubscriber();
        var message = JsonSerializer.Serialize(request);
        await subscriber.PublishAsync(request.IdEntrega, message);
        return Ok();
    }
}

public record LocalizationRequest(string IdEntrega, double Latitude, double Longitude);