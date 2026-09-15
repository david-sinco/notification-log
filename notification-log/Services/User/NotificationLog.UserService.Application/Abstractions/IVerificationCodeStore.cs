namespace NotificationLog.UserService.Application.Abstractions;

public interface IVerificationCodeStore
{
    Task<string> IssueAsync(Guid userId, VerificationPurpose purpose, string target, TimeSpan ttl, CancellationToken ct);

    Task<string?> ConsumeAsync(Guid userId, VerificationPurpose purpose, string code, CancellationToken ct);
}
