using Microsoft.AspNetCore.Mvc;
using NotificationRealTime.Services;

namespace NotificationRealTime.SseV3;

[Route("api/[controller]")]
[ApiController]
public class SseV3Controller(INotificationStreamManager streamManager) : ControllerBase
{
    [HttpGet("stream/{channel}")]
    public async Task GetStream(string channel, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("MachineName", Environment.MachineName);

        var reader = streamManager.Subscribe(channel);
        try
        {
            await foreach (var message in reader.ReadAllAsync(cancellationToken))
            {
                var item = System.Text.Json.JsonSerializer.Deserialize<PublisherDto<object>>(message)!;
                await Response.WriteAsync($"event: {item.EventName}\n", cancellationToken: cancellationToken);
                await Response.WriteAsync($"data: {item.Data}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            streamManager.Unsubscribe(channel, reader);
        }
    }
}