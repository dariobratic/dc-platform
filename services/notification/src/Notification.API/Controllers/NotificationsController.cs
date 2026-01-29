using Microsoft.AspNetCore.Mvc;
using Notification.API.Models;
using Notification.API.Services;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("email")]
    public async Task<ActionResult<NotificationResponse>> SendEmail([FromBody] EmailNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.To))
            return BadRequest(new { error = "To address is required" });

        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(new { error = "Subject is required" });

        if (string.IsNullOrWhiteSpace(request.Body) && string.IsNullOrWhiteSpace(request.TemplateId))
            return BadRequest(new { error = "Body or TemplateId is required" });

        var response = await _notificationService.SendEmailAsync(request);

        return Ok(response);
    }

    [HttpPost("push")]
    public async Task<ActionResult<NotificationResponse>> SendPush([FromBody] PushNotificationRequest request)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest(new { error = "Valid UserId is required" });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required" });

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message is required" });

        var response = await _notificationService.SendPushAsync(request);

        return Ok(response);
    }

    [HttpGet("health")]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse(
            ServiceName: "Notification",
            Status: "Healthy",
            Timestamp: DateTime.UtcNow
        ));
    }
}
