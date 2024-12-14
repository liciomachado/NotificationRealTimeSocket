using Microsoft.AspNetCore.Mvc;

namespace RealTimeDriverTracking.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController : ControllerBase
{
    [HttpGet("notifications/{channel}")]
    public async Task Get([FromServices] WebSocketHandler webSocketHandler, string channel)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsync("Expected a WebSocket request");
            return;
        }

        await webSocketHandler.HandleWebSocketRequest(HttpContext, channel);
    }
}