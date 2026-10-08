using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class VisitorFactory
{
    public static Visitor Registered(Guid id)
    {
        var visitor = Visitor.Register(id, "Víctor Rojas Peña", "victor@example.com", "+573109876543");
        visitor.ClearDomainEvents();

        return visitor;
    }
}
