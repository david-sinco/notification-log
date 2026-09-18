using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Owners;

public sealed record RegisterNaturalOwnerRequest(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber);
