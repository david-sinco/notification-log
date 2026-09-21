using NotificationLog.IdentityService.Application.Abstractions;
using OpenIddict.Abstractions;

namespace NotificationLog.IdentityService.Infrastructure.Security;

internal sealed class OpenIddictSessionRevoker : ISessionRevoker
{
    private readonly IOpenIddictAuthorizationManager _authorizations;
    private readonly IOpenIddictTokenManager _tokens;

    public OpenIddictSessionRevoker(IOpenIddictAuthorizationManager authorizations, IOpenIddictTokenManager tokens)
        => (_authorizations, _tokens) = (authorizations, tokens);

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct)
    {
        var subject = userId.ToString();

        foreach (var authorization in await _authorizations.FindBySubjectAsync(subject, ct).ToListAsync(ct))
            await _authorizations.TryRevokeAsync(authorization, ct);

        foreach (var token in await _tokens.FindBySubjectAsync(subject, ct).ToListAsync(ct))
            await _tokens.TryRevokeAsync(token, ct);
    }
}
