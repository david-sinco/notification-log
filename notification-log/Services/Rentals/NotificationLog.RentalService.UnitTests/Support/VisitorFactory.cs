using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class VisitorFactory
{
    public static Visitor Pending(Guid id)
    {
        var visitor = Visitor.Register(id, "Víctor", "victor@example.com", "3109876543");
        visitor.ClearDomainEvents();

        return visitor;
    }

    public static Visitor Registered(Guid id)
    {
        var visitor = Pending(id);

        visitor.CompleteProfile(
            PersonName.Create("Víctor", "Rojas Peña"),
            IdentityDocument.Create(DocumentType.CitizenshipCard, "1020304"),
            ContactInfo.Create("victor@example.com", "3109876543"));

        visitor.ClearDomainEvents();

        return visitor;
    }
}
