using Marten;
using Microsoft.AspNetCore.Mvc;
using Users.Api.Contracts;
using Users.Application;
using Users.Application.ReadModels;
using Users.Domain;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(UserCommandService commands, IQuerySession session) : ControllerBase
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfile>> GetProfile(Guid id, CancellationToken ct)
    {
        var profile = await session.LoadAsync<UserProfile>(id, ct);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Raw stream, for inspecting the log by hand (SPEC.md §4) — a lab-only endpoint.</summary>
    [HttpGet("{id:guid}/events")]
    public async Task<ActionResult> GetEvents(Guid id, CancellationToken ct)
    {
        var events = await session.Events.FetchStreamAsync(id, token: ct);
        if (events.Count == 0)
            return NotFound();

        var payload = events.Select(e => new
        {
            type = e.EventTypeName,
            version = e.Version,
            occurredAt = e.Timestamp,
            data = e.Data,
        });

        return Ok(payload);
    }

    [HttpGet("funnel")]
    public async Task<ActionResult<IReadOnlyList<VerificationFunnelBucket>>> GetFunnel(CancellationToken ct)
    {
        var buckets = await session.Query<VerificationFunnelBucket>()
            .OrderBy(b => b.Id)
            .ToListAsync(ct);

        return Ok(buckets);
    }
}
