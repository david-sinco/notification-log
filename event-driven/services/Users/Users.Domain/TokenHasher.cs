using System.Security.Cryptography;
using System.Text;

namespace Users.Domain;

/// <summary>
/// Hashes verification tokens. Only the hash is ever put on a domain event; the plaintext is
/// generated and returned by Users.Application, never persisted (SPEC.md §4, "Token handling").
/// </summary>
public static class TokenHasher
{
    public static string Hash(string plaintextToken)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintextToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
