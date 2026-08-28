using Users.Domain;

namespace Users.Application.ReadModels;

/// <summary>
/// Multi-stream, async projection keyed by day ("yyyy-MM-dd") — the expensive one. Marten's
/// async daemon walks every user's stream single-threaded to build these buckets, which is
/// exactly the workload this lab compares against a partition-parallel Kafka consumer group
/// doing the same aggregation (SPEC.md §4). Self-aggregating via public Apply(event) methods,
/// same convention as User/UserProfile — zero Marten reference needed here; the Marten-specific
/// slicing (which events land in which day's bucket) lives in Users.Infrastructure's
/// VerificationFunnelProjection.
/// </summary>
public sealed class VerificationFunnelBucket
{
    // Public, not private, setters: Marten's async daemon persists this document and reads it
    // back JSON-deserialized on every incremental update, and a private setter silently
    // deserializes to the property's default instead of failing loudly.
    public string Id { get; set; } = string.Empty;
    public int Registrations { get; set; }
    public int EmailChangesRequested { get; set; }
    public int EmailsVerified { get; set; }
    public int PhoneChangesRequested { get; set; }
    public int PhonesVerified { get; set; }
    public int Deactivations { get; set; }

    // Combined across both channels: the spec lists a single "failures split by reason" bucket
    // in the funnel, not one per channel.
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
