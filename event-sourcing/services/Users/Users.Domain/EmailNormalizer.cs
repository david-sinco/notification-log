namespace Users.Domain;

/// <summary>
/// Shared by the aggregate (invariant 3: no re-requesting an already-verified email) and by
/// Users.Infrastructure's email-reservation document, whose id *is* the normalized email.
/// Both sides must agree on normalization or the uniqueness check silently misses collisions.
/// </summary>
public static class EmailNormalizer
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
