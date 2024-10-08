// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to notifications management. 
// ***********************************************************************

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Services;

namespace omnicart_api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Send a notification
    [HttpPost("send")]
    [Authorize(Roles = "admin,vendor,csr,customer")] // only logged-in users
    public async Task<ActionResult> SendNotification([FromBody] NotificationRequest notification)
    {
        Console.WriteLine(notification.Roles);
        await _notificationService.CreateNotificationAsync(notification);
        return Ok(new AppResponse<NotificationRequest> { Success = true, Message = "Notification sent successfully.", Data = notification });
    }

    //retrieves notifications for a specific user based on their roles.
    [HttpGet]
    [Authorize(Roles = "admin,vendor,csr,customer")] // only logged-in users
    public async Task<ActionResult<AppResponse<List<Notification>>>> GetUserNotificationsByRole([FromQuery] string roles)
    {
        var notifications = await _notificationService.GetNotificationsForUserAsync(roles);

        var response = new AppResponse<List<Notification>> { Success = true, Message = "Notification retrieves successfully.", Data = notifications };
        return Ok(response);
    }

    // Get all notifications for a user
    [HttpGet("my")]
    [Authorize(Roles = "admin,vendor,csr,customer")] // only logged-in users
    public async Task<ActionResult<List<Notification>>> GetUserNotifications()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return UnprocessableEntity(new AppResponse<Product>
            {
                Success = false,
                Message = "Please login first.",
                Error = "Unprocessable Entity",
                ErrorCode = 422,
                ErrorData = UnprocessableEntity(ModelState)
            });
        }

        var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
        var response = new AppResponse<List<Notification>> { Success = true, Message = "Notification retrieves successfully.", Data = notifications };
        return Ok(response);
    }


    // Mark notification as read
    [HttpPost("mark-as-read/{notificationId}")]
    [Authorize(Roles = "admin,vendor,csr,customer")] // only logged-in users
    public async Task<ActionResult<AppResponse<Notification>>> MarkAsRead(string notificationId)
    {
        await _notificationService.MarkNotificationAsReadAsync(notificationId);
        return Ok(new AppResponse<Notification> { Success = true, Message = "Notification marked as read." });
    }
}