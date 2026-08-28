using Marten.Events.Projections;
using Users.Application.ReadModels;
using Users.Domain;

namespace Users.Infrastructure.Projections;

/// <summary>
/// Multi-stream aggregation across every user stream into shared day buckets — this is what
/// forces Marten's async daemon to walk the whole event table single-threaded (SPEC.md §4).
/// Registered async; see ServiceCollectionExtensions.
/// </summary>
public sealed class VerificationFunnelProjection : MultiStreamProjection<VerificationFunnelBucket, string>
{
    public VerificationFunnelProjection()
    {
        Identity<UserRegistered>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<EmailChangeRequested>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<EmailVerified>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<EmailVerificationFailed>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<PhoneChangeRequested>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<PhoneVerified>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<PhoneVerificationFailed>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
        Identity<UserDeactivated>(e => VerificationFunnelBucket.DayKey(e.OccurredAt));
    }
}
