namespace Users.Domain;

/// <summary>
/// Event-sourced aggregate. Two groups of members, kept strictly apart (SPEC.md §4):
///
///   Decide*(command, now) — validates invariants, returns events, never mutates state.
///   Apply(event)          — mutates state from an event; Marten calls these on rehydration.
///
/// Because both a fresh command and a stream replay end up mutating state through the same
/// Apply methods, a projection rebuild cannot diverge from live behaviour. This type has zero
/// package references (not even Marten) so it stays testable as plain C# and so Marten can use
/// its reflection-based "self-aggregating" convention without any base class.
///
/// Setters are public, not private, even though nothing outside Apply should ever use them:
/// Marten's inline snapshot for this type round-trips through JSON on every read
/// (FetchLatest/LoadAsync deserialize the stored document rather than replaying events every
/// time), and a private setter silently deserializes to the property's default value instead of
/// failing loudly. Mutate only via Decide/Apply by convention, not by the compiler.
/// </summary>
public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? VerifiedEmail { get; set; }
    public string? VerifiedPhone { get; set; }
    public PendingContact? PendingEmail { get; set; }
    public PendingContact? PendingPhone { get; set; }
    public NotificationPreferences Preferences { get; set; } = NotificationPreferences.Default;
    public bool IsActive { get; set; }
    public int Version { get; set; }

    /// <summary>
    /// Marten's self-aggregating convention requires an explicit Create method for the event
    /// that starts a stream — a bare constructor + Apply(UserRegistered) is silently never
    /// invoked. Kept trivial on purpose: it delegates to the same Apply path everything else
    /// uses, so there's still exactly one place that knows how to interpret UserRegistered.
    /// </summary>
    public static User Create(UserRegistered e)
    {
        var user = new User();
        user.Apply(e);
        return user;
    }

    // ---- Decide -----------------------------------------------------------------------------

    public static UserRegistered DecideRegisterUser(RegisterUser command, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new DomainException("Name must not be empty.");

        return new UserRegistered(command.UserId, command.Name, now);
    }

    public IReadOnlyList<object> DecideRequestEmailChange(RequestEmailChange command, DateTimeOffset now)
    {
        EnsureActive();

        // An empty email would otherwise flow all the way to the reservation document as an
        // empty document id, which Marten rejects — surface it as a 422 here instead of a 500
        // three steps downstream, at verification time.
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new DomainException("Email must not be empty.");

        var normalized = EmailNormalizer.Normalize(command.Email);
        if (string.Equals(normalized, VerifiedEmail, StringComparison.Ordinal))
            throw new DomainException("This email is already verified for this user.");

        return [new EmailChangeRequested(Id, normalized, command.TokenHash, command.ExpiresAt, now)];
    }

    public IReadOnlyList<object> DecideVerifyEmail(VerifyEmail command, DateTimeOffset now)
    {
        EnsureActive();

        var pending = PendingEmail ?? throw new DomainException("No pending email verification to confirm.");

        if (TryDecideVerification(pending, command.Token, now, out var reason, out var attempts))
            return [new EmailVerified(Id, pending.Value, now)];

        return [new EmailVerificationFailed(Id, reason, attempts, now)];
    }

    public IReadOnlyList<object> DecideRequestPhoneChange(RequestPhoneChange command, DateTimeOffset now)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(command.Phone))
            throw new DomainException("Phone must not be empty.");

        if (string.Equals(command.Phone, VerifiedPhone, StringComparison.Ordinal))
            throw new DomainException("This phone number is already verified for this user.");

        return [new PhoneChangeRequested(Id, command.Phone, command.TokenHash, command.ExpiresAt, now)];
    }

    public IReadOnlyList<object> DecideVerifyPhone(VerifyPhone command, DateTimeOffset now)
    {
        EnsureActive();

        var pending = PendingPhone ?? throw new DomainException("No pending phone verification to confirm.");

        if (TryDecideVerification(pending, command.Token, now, out var reason, out var attempts))
            return [new PhoneVerified(Id, pending.Value, now)];

        return [new PhoneVerificationFailed(Id, reason, attempts, now)];
    }

    public IReadOnlyList<object> DecideChangePreferences(ChangePreferences command, DateTimeOffset now)
    {
        EnsureActive();

        var prefs = command.Preferences;

        if (prefs.SmsEnabled && VerifiedPhone is null)
            throw new DomainException("Cannot enable SMS before a phone number is verified.");

        if (prefs.QuietHoursStart.HasValue != prefs.QuietHoursEnd.HasValue)
            throw new DomainException("Quiet hours require both a start and an end, or neither.");

        return [new PreferencesChanged(Id, prefs, now)];
    }

    public IReadOnlyList<object> DecideDeactivateUser(DeactivateUser command, DateTimeOffset now)
    {
        EnsureActive();
        return [new UserDeactivated(Id, now)];
    }

    public IReadOnlyList<object> DecideReactivateUser(ReactivateUser command, DateTimeOffset now)
    {
        if (IsActive)
            throw new DomainException("User is already active.");

        return [new UserReactivated(Id, now)];
    }

    /// <summary>
    /// Shared expiry/hash check for both contact channels. Returns true on success. On failure,
    /// increments the attempt count and, at 3 failures, reports TooManyAttempts so Apply can
    /// invalidate the pending contact and force the flow to restart (invariants 1 and 2).
    /// </summary>
    private static bool TryDecideVerification(
        PendingContact pending, string token, DateTimeOffset now,
        out VerificationFailureReason reason, out int attempts)
    {
        var isExpired = now > pending.ExpiresAt;
        var isMismatch = !isExpired && TokenHasher.Hash(token) != pending.TokenHash;

        if (!isExpired && !isMismatch)
        {
            reason = default;
            attempts = default;
            return true;
        }

        attempts = pending.FailedAttempts + 1;
        reason = attempts >= 3
            ? VerificationFailureReason.TooManyAttempts
            : isExpired ? VerificationFailureReason.TokenExpired : VerificationFailureReason.TokenMismatch;
        return false;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("User is deactivated and cannot accept this command.");
    }

    // ---- Apply --------------------------------------------------------------------------------

    /// <summary>
    /// Dispatches to the typed Apply overload below. Marten's own reflection-based aggregation
    /// never calls this — it's here so callers holding events as `object` (Users.Application's
    /// command handlers, replaying a list returned by a Decide* method) can drive the exact same
    /// Apply path Marten uses, per the guarantee described on this type.
    /// </summary>
    public void Apply(object @event)
    {
        switch (@event)
        {
            case UserRegistered e: Apply(e); break;
            case EmailChangeRequested e: Apply(e); break;
            case EmailVerified e: Apply(e); break;
            case EmailVerificationFailed e: Apply(e); break;
            case PhoneChangeRequested e: Apply(e); break;
            case PhoneVerified e: Apply(e); break;
            case PhoneVerificationFailed e: Apply(e); break;
            case PreferencesChanged e: Apply(e); break;
            case UserDeactivated e: Apply(e); break;
            case UserReactivated e: Apply(e); break;
            default: throw new ArgumentOutOfRangeException(nameof(@event), @event.GetType(), "Unknown User event type.");
        }
    }

    public void Apply(UserRegistered e)
    {
        Id = e.UserId;
        Name = e.Name;
        Preferences = NotificationPreferences.Default;
        IsActive = true;
        Version++;
    }

    public void Apply(EmailChangeRequested e)
    {
        PendingEmail = new PendingContact(e.Email, e.TokenHash, e.ExpiresAt, FailedAttempts: 0);
        Version++;
    }

    public void Apply(EmailVerified e)
    {
        VerifiedEmail = e.Email;
        PendingEmail = null;
        Version++;
    }

    public void Apply(EmailVerificationFailed e)
    {
        PendingEmail = e.Reason == VerificationFailureReason.TooManyAttempts
            ? null
            : PendingEmail is { } p ? p with { FailedAttempts = e.FailedAttempts } : null;
        Version++;
    }

    public void Apply(PhoneChangeRequested e)
    {
        PendingPhone = new PendingContact(e.Phone, e.TokenHash, e.ExpiresAt, FailedAttempts: 0);
        Version++;
    }

    public void Apply(PhoneVerified e)
    {
        VerifiedPhone = e.Phone;
        PendingPhone = null;
        Version++;
    }

    public void Apply(PhoneVerificationFailed e)
    {
        PendingPhone = e.Reason == VerificationFailureReason.TooManyAttempts
            ? null
            : PendingPhone is { } p ? p with { FailedAttempts = e.FailedAttempts } : null;
        Version++;
    }

    public void Apply(PreferencesChanged e)
    {
        Preferences = e.Preferences;
        Version++;
    }

    public void Apply(UserDeactivated e)
    {
        IsActive = false;
        Version++;
    }

    public void Apply(UserReactivated e)
    {
        IsActive = true;
        Version++;
    }
}
