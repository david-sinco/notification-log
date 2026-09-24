using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Visitors;

public sealed record RegisterVisitorRequest(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);
