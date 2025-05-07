using Microsoft.AspNetCore.Mvc;

namespace NotificationRealTimeSocket.SseV3;

[Route("api/[controller]")]
[ApiController]
public class SseV3Controller(NotificationStreamManager streamManager) : ControllerBase
{
    [HttpGet("stream/{channel}")]
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
}