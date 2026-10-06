namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class TestClock : TimeProvider
{
    private DateTimeOffset? _now;

    public override DateTimeOffset GetUtcNow() => _now ?? base.GetUtcNow();

    public void Set(DateTimeOffset now) => _now = now.ToUniversalTime();

    public void Advance(TimeSpan time) => _now = GetUtcNow() + time;

    public void Reset() => _now = null;
}
