using Users.Domain;

namespace Users.Application.ReadModels;

/// <summary>
/// Identical to event-sourcing/'s VerificationFunnelBucket — copied near-verbatim on purpose. It
/// was already a zero-Marten, self-aggregating Apply-per-event class; only what drives it changes,
/// from Marten's single-threaded async daemon to a Kafka consumer group
/// (Users.Infrastructure.Materializer.VerificationFunnelMaterializer). Same destination shape
/// either way, so a rebuild-time comparison between the two architectures measures the driver, not
/// the storage engine (the same discipline event-sourcing/SPEC.md §11 calls out).
/// </summary>
public sealed class VerificationFunnelBucket
{
    public string Id { get; set; } = string.Empty;
    public int Registrations { get; set; }
    public int EmailChangesRequested { get; set; }
    public int EmailsVerified { get; set; }
    public int PhoneChangesRequested { get; set; }
    public int PhonesVerified { get; set; }
    public int Deactivations { get; set; }

    public int TokenMismatchFailures { get; set; }
    public int TokenExpiredFailures { get; set; }
    public int TooManyAttemptsFailures { get; set; }

    /// <summary>Registrations that went on to verify an email, as a fraction of registrations.</summary>
    public double ConversionRate => Registrations == 0 ? 0 : (double)EmailsVerified / Registrations;

    public void Apply(UserRegistered e)
    {
        Id = DayKey(e.OccurredAt);
        Registrations++;
    }

    public void Apply(EmailChangeRequested e) => EmailChangesRequested++;

    public void Apply(EmailVerified e) => EmailsVerified++;

    public void Apply(EmailVerificationFailed e) => ApplyFailure(e.Reason);

    public void Apply(PhoneChangeRequested e) => PhoneChangesRequested++;

    public void Apply(PhoneVerified e) => PhonesVerified++;

    public void Apply(PhoneVerificationFailed e) => ApplyFailure(e.Reason);

    public void Apply(UserDeactivated e) => Deactivations++;

    private void ApplyFailure(VerificationFailureReason reason)
    {
        switch (reason)
        {
            case VerificationFailureReason.TokenMismatch: TokenMismatchFailures++; break;
            case VerificationFailureReason.TokenExpired: TokenExpiredFailures++; break;
            case VerificationFailureReason.TooManyAttempts: TooManyAttemptsFailures++; break;
        }
    }

    public static string DayKey(DateTimeOffset occurredAt) => occurredAt.UtcDateTime.ToString("yyyy-MM-dd");
}
