using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Visitors;

public sealed record CompleteVisitorProfileRequest(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);
