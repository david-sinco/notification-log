using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Visitors.Exceptions;

public sealed class VisitorUnchangedException(Guid visitorId)
    : DomainException($"Los datos del visitante '{visitorId}' no cambiaron.");
