using Microsoft.AspNetCore.Mvc;
using NotificationRealTime.Repositories;
using NotificationRealTime.Services;

namespace NotificationRealTime.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationController(INotificationStreamManager pubSubService, INotificationsRepository notificationsRepository) : ControllerBase
{
    [HttpPost("publish/{channel}")]
    public async Task<IActionResult> Publish(string channel, [FromBody] NotificationDtoRequest request)
    {
        var newNotification = await notificationsRepository.AddMessage(channel, request.Message, request.Url);
        await pubSubService.PublishAsync(channel, newNotification, "new-notification");
        return Ok(newNotification);
    }

    [HttpPost("dashboard/{channel}")]
    public async Task<IActionResult> Dashboard(string channel)
    {
        // Gera um valor double aleatório entre 1 (inclusive) e 1000 (exclusivo)
        var random = new Random();
        var valorAleatorio = random.NextDouble() * (1000 - 1) + 1;
        var evento = new EventDto(DateTime.Now, valorAleatorio);
        await pubSubService.PublishAsync(channel, evento, "new-item-dashboard");
        return Ok(evento);
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

public record EventDto(DateTime Date, double Value);
public record NotificationDtoRequest(string Message, string? Url);
public record DeleteEvent(string Action, string MessageId);
public record ChangeNotificationEvent(string Action, string Id, string Message, DateTime Date, string? Url, string Status);
