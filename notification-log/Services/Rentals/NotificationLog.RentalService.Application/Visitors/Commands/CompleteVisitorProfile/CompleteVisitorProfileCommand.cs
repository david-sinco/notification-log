using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;

public sealed record CompleteVisitorProfileCommand(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);
