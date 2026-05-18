using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Users.ChangePassword;
using PFP.Application.Features.Users.GetMe;
using PFP.Application.Features.Users.GetProfile;
using PFP.Application.Features.Users.NotificationPrefs.GetNotificationPrefs;
using PFP.Application.Features.Users.NotificationPrefs.UpdateNotificationPrefs;
using PFP.Application.Features.Users.UpdateProfile;
using PFP.Application.Features.Users.UploadAvatar;

namespace PFP.API.Controllers;

/// <summary>User profile, password, avatar, and notification-preference endpoints.</summary>
[ApiController]
[Authorize]
[Route("api/v1/user")]
public sealed class UsersController : ControllerBase
{
    private const long AvatarMaxBytes = UploadAvatarCommandValidator.MaxSizeBytes;

    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the current session's user summary (used by the frontend on bootstrap).</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<GetMeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMeResponse>>> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMeQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetMeResponse> { Data = result });
    }

    /// <summary>Returns the full profile sidecar.</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<GetProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfileQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetProfileResponse> { Data = result });
    }

    /// <summary>Updates editable profile + personalisation fields.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UpdateProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateProfileResponse>>> UpdateProfile(
        [FromBody] UpdateProfileBody body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand(
            body.FullName,
            body.DisplayName,
            body.PhoneNumber,
            body.DateOfBirth,
            body.LanguageCode,
            body.Timezone,
            body.DateFormat,
            body.Theme);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateProfileResponse> { Data = result });
    }

    /// <summary>Changes the local password (revokes every other refresh token).</summary>
    [HttpPut("password")]
    [ProducesResponseType(typeof(ApiResponse<ChangePasswordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword(
        [FromBody] ChangePasswordBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new ChangePasswordCommand(body.CurrentPassword, body.NewPassword), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<ChangePasswordResponse> { Data = result });
    }

    /// <summary>Returns the granular notification matrix.</summary>
    [HttpGet("notification-prefs")]
    [ProducesResponseType(typeof(ApiResponse<GetNotificationPrefsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetNotificationPrefsResponse>>> GetNotificationPrefs(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNotificationPrefsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetNotificationPrefsResponse> { Data = result });
    }

    /// <summary>Bulk-upserts notification-preference rows for the caller.</summary>
    [HttpPut("notification-prefs")]
    [ProducesResponseType(typeof(ApiResponse<UpdateNotificationPrefsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateNotificationPrefsResponse>>> UpdateNotificationPrefs(
        [FromBody] UpdateNotificationPrefsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateNotificationPrefsResponse> { Data = result });
    }

    /// <summary>Uploads a new avatar (JPEG / PNG / WebP, max 5 MB).</summary>
    [HttpPost("avatar")]
    [RequestSizeLimit(AvatarMaxBytes + (1024 * 64))]
    [ProducesResponseType(typeof(ApiResponse<UploadAvatarResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UploadAvatarResponse>>> UploadAvatar(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();
        var command = new UploadAvatarCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UploadAvatarResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="UsersController.UpdateProfile"/>.</summary>
public sealed class UpdateProfileBody
{
    /// <summary>New full name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Optional display-name override.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Optional phone number in E.164 format.</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Optional date of birth.</summary>
    public DateOnly? DateOfBirth { get; init; }

    /// <summary>BCP-47 locale code.</summary>
    public string LanguageCode { get; init; } = "vi";

    /// <summary>IANA timezone identifier.</summary>
    public string Timezone { get; init; } = "Asia/Ho_Chi_Minh";

    /// <summary>Preferred date format token, e.g. <c>dd/MM/yyyy</c>.</summary>
    public string DateFormat { get; init; } = "dd/MM/yyyy";

    /// <summary>UI theme: <c>light</c> | <c>dark</c> | <c>system</c>.</summary>
    public string Theme { get; init; } = "system";
}

/// <summary>JSON body for <see cref="UsersController.ChangePassword"/>.</summary>
public sealed class ChangePasswordBody
{
    /// <summary>Current password (required for confirmation).</summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>New password (min 8, max 128 chars; must differ from the current one).</summary>
    public string NewPassword { get; init; } = string.Empty;
}
