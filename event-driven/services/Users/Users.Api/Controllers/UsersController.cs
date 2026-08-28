using Microsoft.AspNetCore.Mvc;
using Users.Api.Contracts;
using Users.Application;
using Users.Application.ReadModels;
using Users.Domain;
using Users.Infrastructure.Materializer;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    UserCommandService commands, UserMaterializer users, VerificationFunnelMaterializer funnel) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request, CancellationToken ct)
    {
        var user = await commands.RegisterUserAsync(request.Name, ct);
        var response = UserResponse.From(user);
        return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, response);
    }

    [HttpPost("{id:guid}/email")]
    public async Task<ActionResult<VerificationTokenResponse>> RequestEmailChange(
        Guid id, RequestEmailChangeRequest request, CancellationToken ct)
    {
        var result = await commands.RequestEmailChangeAsync(id, request.Email, ct);
        return Ok(VerificationTokenResponse.From(result));
    }

    [HttpPost("{id:guid}/email/verify")]
    public async Task<ActionResult<UserResponse>> VerifyEmail(Guid id, VerifyEmailRequest request, CancellationToken ct)
    {
        var user = await commands.VerifyEmailAsync(id, request.Token, ct);
        return Ok(UserResponse.From(user));
    }

    [HttpPost("{id:guid}/phone")]
    public async Task<ActionResult<VerificationTokenResponse>> RequestPhoneChange(
        Guid id, RequestPhoneChangeRequest request, CancellationToken ct)
    {
        var result = await commands.RequestPhoneChangeAsync(id, request.Phone, ct);
        return Ok(VerificationTokenResponse.From(result));
    }

    [HttpPost("{id:guid}/phone/verify")]
    public async Task<ActionResult<UserResponse>> VerifyPhone(Guid id, VerifyPhoneRequest request, CancellationToken ct)
    {
        var user = await commands.VerifyPhoneAsync(id, request.Token, ct);
        return Ok(UserResponse.From(user));
    }

    [HttpPut("{id:guid}/preferences")]
    public async Task<ActionResult<UserResponse>> ChangePreferences(
        Guid id, ChangePreferencesRequest request, CancellationToken ct)
    {
        var preferences = new NotificationPreferences(
            request.Email, request.Sms, request.QuietHoursStart, request.QuietHoursEnd);
        var user = await commands.ChangePreferencesAsync(id, preferences, ct);
        return Ok(UserResponse.From(user));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserResponse>> Deactivate(Guid id, CancellationToken ct)
    {
        var user = await commands.DeactivateUserAsync(id, ct);
        return Ok(UserResponse.From(user));
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<UserResponse>> Reactivate(Guid id, CancellationToken ct)
    {
        var user = await commands.ReactivateUserAsync(id, ct);
        return Ok(UserResponse.From(user));
    }

    /// <summary>
    /// Reads straight from the materializer's in-memory fold — no separate UserProfile read-model
    /// type, unlike event-sourcing/'s controller. There, an inline projection exists because the
    /// event stream and a snapshot document are two separately-stored artifacts kept in sync; here
    /// there is only one place state lives, so that distinction collapses.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetProfile(Guid id, CancellationToken ct)
    {
        await users.WaitUntilReadyAsync(ct);
        var user = users.TryGet(id);
        return user is null ? NotFound() : Ok(UserResponse.From(user));
    }

    /// <summary>Raw log for this user, for inspecting it by hand — a lab-only endpoint. Kept
    /// alongside the fold rather than re-scanning the topic per request.</summary>
    [HttpGet("{id:guid}/events")]
    public async Task<ActionResult> GetEvents(Guid id, CancellationToken ct)
    {
        await users.WaitUntilReadyAsync(ct);
        var history = users.TryGetHistory(id);
        if (history is null)
            return NotFound();

        var payload = history.Select(e => new { type = e.GetType().Name, data = e });
        return Ok(payload);
    }

    [HttpGet("funnel")]
    public async Task<ActionResult<IReadOnlyList<VerificationFunnelBucket>>> GetFunnel(CancellationToken ct)
    {
        await funnel.WaitUntilReadyAsync(ct);
        return Ok(funnel.GetAll());
    }
}
