using System.Security.Claims;

namespace Domain.Shared.Authorization;

public static class ClaimsPrincipalExtensions
{
    private const string SubjectClaim = "sub";

    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirst(SubjectClaim)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("El token no tiene un usuario válido.");

    public static bool IsAdministrador(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Administrador));

    public static bool IsAsesor(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Asesor));

    public static bool IsPropietario(this ClaimsPrincipal principal) => principal.IsInRole(nameof(UserRole.Propietario));
}
