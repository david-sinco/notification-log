using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Owners.Events;

public sealed record CompanyOwnerRegistered(
    Guid OwnerId,
    Guid CreatedBy,
    string LegalName,
    string Nit,
    int NitCheckDigit,
    string Email,
    string Phone
) : DomainEvent;
