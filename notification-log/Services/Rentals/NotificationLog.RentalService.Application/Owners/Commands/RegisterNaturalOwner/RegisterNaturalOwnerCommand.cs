using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;

public sealed record RegisterNaturalOwnerCommand(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber);
