using Microsoft.AspNetCore.Mvc;
using NotificationRealTimeSocket.Repositories;
using NotificationRealTimeSocket.Services;

namespace NotificationRealTimeSocket.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationController(INotificationStreamManager pubSubService, INotificationsRepository notificationsRepository) : ControllerBase
{

    [HttpPost("publish/{channel}")]
    public async Task<IActionResult> Publish(string channel, [FromBody] NotificationDtoRequest request)
    {
        Response.Headers.Append("MachineName", Environment.MachineName);

        var newNotification = await notificationsRepository.AddMessage(channel, request.Message, request.Url);
        await pubSubService.PublishAsync(channel, newNotification, "new-notification");
        return Ok(newNotification);
    }

    [HttpDelete("delete/{userId}/{messageId}")]
    public async Task<IActionResult> Delete(string userId, string messageId)
    {
        await notificationsRepository.DeleteMessage(userId, messageId);

        var deleteNotification = new DeleteEvent("delete", messageId);
        await pubSubService.PublishAsync(userId, deleteNotification, "delete-notification");
        return NoContent();
    }

    [HttpPut("finalize/{userId}/{messageId}")]
    public async Task<IActionResult> Finalize(string userId, string messageId)
    {
        var notification = await notificationsRepository.GetAsync(userId, messageId);
        if (notification is null) return NotFound();

        notification.Status = "Finalized";
        await notificationsRepository.Update(userId, notification);

        var finalizedNotification = new ChangeNotificationEvent("update", notification.Id, notification.Message,
            notification.DateCreated, notification.Url, notification.Status);

        await pubSubService.PublishAsync(userId, finalizedNotification, "update-notification");
        return NoContent();
    }
}

public record NotificationDtoRequest(string Message, string? Url);
public record DeleteEvent(string Action, string MessageId);
public record ChangeNotificationEvent(string Action, string Id, string Message, DateTime Date, string? Url, string Status);
