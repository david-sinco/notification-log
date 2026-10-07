using System.Security.Claims;

namespace Domain.Shared.Authorization;

public static class ClaimsPrincipalExtensions
{
    private const string SubjectClaim = "sub";
    private const string EmailClaim = "email";
    private const string PhoneClaim = "phone_number";

    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirst(SubjectClaim)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("El token no tiene un usuario válido.");

    public static string? GetEmail(this ClaimsPrincipal principal) => principal.FindFirst(EmailClaim)?.Value;

    public static string? GetPhone(this ClaimsPrincipal principal) => principal.FindFirst(PhoneClaim)?.Value;

    public static bool IsAdministrador(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Administrador));

    public static bool IsModerador(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Moderador));

    public static bool IsPropietario(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Propietario));
}
