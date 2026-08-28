namespace Users.Api.Contracts;

public sealed record RegisterUserRequest(string Name);

public sealed record RequestEmailChangeRequest(string Email);

public sealed record VerifyEmailRequest(string Token);

public sealed record RequestPhoneChangeRequest(string Phone);

public sealed record VerifyPhoneRequest(string Token);

public sealed record ChangePreferencesRequest(
    bool Email, bool Sms, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);
