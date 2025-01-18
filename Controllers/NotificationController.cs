using Microsoft.AspNetCore.Mvc;
using NotificationRealTimeSocket.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace NotificationRealTimeSocket.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationController(IConnectionMultiplexer redis, INotificationsMongoRepository notificationsMongoRepository) : ControllerBase
{

    [HttpPost("publish/{userId}")]
    public async Task<IActionResult> Publish(string userId, [FromBody] NotificationDtoRequest request)
    {
        var subscriber = redis.GetSubscriber();
        var newNotification = await notificationsMongoRepository.AddMessage(userId, request.Message, request.Url);
        var message = JsonSerializer.Serialize(newNotification);
        await subscriber.PublishAsync(userId, message);
        return Ok(newNotification);
    }

    [HttpDelete("delete/{userId}/{messageId}")]
    public async Task<IActionResult> Delete(string userId, string messageId)
    {
        await notificationsMongoRepository.DeleteMessage(userId, messageId);
        var subscriber = redis.GetSubscriber();
        var deleteNotification = JsonSerializer.Serialize(new { Action = "delete", MessageId = messageId });
        await subscriber.PublishAsync(userId, deleteNotification);
        return NoContent();
    }

    [HttpPut("finalize/{userId}/{messageId}")]
    public async Task<IActionResult> Finalize(string userId, string messageId)
    {
        var notification = await notificationsMongoRepository.GetAsync(userId, messageId);
        if (notification is null) return NotFound();

        notification.Status = "Finalized";
        await notificationsMongoRepository.Update(userId, notification);

        var subscriber = redis.GetSubscriber();
        var deleteNotification = JsonSerializer.Serialize(new ChangeNotificationDtoWebsocket("update", notification.Id, notification.Message, notification.DateCreated, notification.Url, notification.Status));
        await subscriber.PublishAsync(userId, deleteNotification);
        return NoContent();
    }
}

public record NotificationDtoRequest(string Message, string? Url);