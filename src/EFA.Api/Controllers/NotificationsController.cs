using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EFA.Api.Common;
using EFA.Application.Notifications;
using EFA.Application.Notifications.CreateNotification;
using EFA.Application.Notifications.GetAdminNotificationLog;
using EFA.Application.Notifications.GetNotificationById;
using EFA.Application.Notifications.GetNotifications;
using EFA.Application.Notifications.GetUnreadNotificationsCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class NotificationsController : ControllerBase
    {
        private readonly CreateNotificationHandler _createNotificationHandler;
        private readonly GetNotificationsHandler _getNotificationsHandler;
        private readonly GetNotificationByIdHandler _getNotificationByIdHandler;
        private readonly GetUnreadNotificationsCountHandler _getUnreadNotificationsCountHandler;
        private readonly GetAdminNotificationLogHandler _getAdminNotificationLogHandler;

        public NotificationsController(
            CreateNotificationHandler createNotificationHandler,
            GetNotificationsHandler getNotificationsHandler,
            GetNotificationByIdHandler getNotificationByIdHandler,
            GetUnreadNotificationsCountHandler getUnreadNotificationsCountHandler,
            GetAdminNotificationLogHandler getAdminNotificationLogHandler)
        {
            _createNotificationHandler = createNotificationHandler;
            _getNotificationsHandler = getNotificationsHandler;
            _getNotificationByIdHandler = getNotificationByIdHandler;
            _getUnreadNotificationsCountHandler = getUnreadNotificationsCountHandler;
            _getAdminNotificationLogHandler = getAdminNotificationLogHandler;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,MembershipOfficer")]
        public async Task<IActionResult> CreateNotification(
            [FromBody] CreateNotificationCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createNotificationHandler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to create notification.",
                    result.Errors));
            }

            return Ok(ApiResponse<CreateNotificationResponse>.Success(
                result.Data!,
                "Notification sent successfully."));
        }

        [HttpGet("log")]
        [Authorize(Roles = "Admin,MembershipOfficer")]
        public async Task<IActionResult> GetAdminNotificationLog(
            [FromQuery] string? search,
            [FromQuery] string? targetGroup,
            CancellationToken cancellationToken)
        {
            var result = await _getAdminNotificationLogHandler.HandleAsync(
                new GetAdminNotificationLogQuery
                {
                    Search = search,
                    TargetGroup = targetGroup
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve notification log.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<AdminNotificationLogResponse>>.Success(
                result.Data!,
                "Notification log retrieved successfully."));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _getNotificationsHandler.HandleAsync(
                new GetNotificationsQuery { UserId = userId },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve notifications.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<NotificationListResponse>>.Success(
                result.Data!,
                "Notifications retrieved successfully."));
        }

        [HttpGet("unread-count")]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _getUnreadNotificationsCountHandler.HandleAsync(
                new GetUnreadNotificationsCountQuery { UserId = userId },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve unread notifications count.",
                    result.Errors));
            }

            return Ok(ApiResponse<UnreadNotificationsCountResponse>.Success(
                result.Data!,
                "Unread notifications count retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetNotificationById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _getNotificationByIdHandler.HandleAsync(
                new GetNotificationByIdQuery
                {
                    Id = id,
                    UserId = userId
                },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Notification not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve notification.",
                    result.Errors));
            }

            return Ok(ApiResponse<NotificationDetailsResponse>.Success(
                result.Data!,
                "Notification retrieved successfully."));
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
