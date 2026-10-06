using Domain.Shared.Common;

namespace NotificationLog.RentalService.UnitTests.Support;

public abstract class DomainScenario
{
    protected const string Accepted = "se acepta";

    protected Exception? Rejection { get; private set; }

    protected DateTimeOffset Now { get; private set; } = Clock.Now;

    protected void ItIs(string moment) => Now = Clock.At(moment);

    protected void Try(Action action)
    {
        Rejection = null;

        try
        {
            action();
        }
        catch (Exception rejection) when (IsRejection(rejection))
        {
            Rejection = rejection;
        }
    }

    protected async Task TryAsync(Func<Task> action)
    {
        Rejection = null;

        try
        {
            await action();
        }
        catch (Exception rejection) when (IsRejection(rejection))
        {
            Rejection = rejection;
        }
    }

    protected virtual bool IsRejection(Exception exception) => exception is DomainException or ForbiddenException;

    protected void ResultIs(string result)
    {
        if (result == Accepted)
            Assert.IsNull(Rejection, Rejection?.Message);
        else
            IsRejectedWith(result);
    }

    protected void IsAccepted(bool accepted) => Assert.AreEqual(accepted, Rejection is null, Rejection?.Message);

    protected void IsRejectedWith(string message) => Assert.AreEqual(message, Rejection?.Message);

    protected static void HasNoEffect(AggregateRoot aggregate) => Assert.IsEmpty(aggregate.DomainEvents);
}
