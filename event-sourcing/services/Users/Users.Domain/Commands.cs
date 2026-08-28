namespace Users.Domain;

// Inputs to User's Decide* methods. Token plaintext never appears here: Users.Application
// generates the token, hashes it with TokenHasher, and passes only the hash and expiry — the
// aggregate is deterministic given (command, now) and never invents secrets of its own.

public sealed record RegisterUser(Guid UserId, string Name);

public sealed record RequestEmailChange(Guid UserId, string Email, string TokenHash, DateTimeOffset ExpiresAt);

public sealed record VerifyEmail(Guid UserId, string Token);

public sealed record RequestPhoneChange(Guid UserId, string Phone, string TokenHash, DateTimeOffset ExpiresAt);

public sealed record VerifyPhone(Guid UserId, string Token);

public sealed record ChangePreferences(Guid UserId, NotificationPreferences Preferences);

public sealed record DeactivateUser(Guid UserId);

public sealed record ReactivateUser(Guid UserId);
