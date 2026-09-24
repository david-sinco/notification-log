using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Application.Visitors.Commands.RegisterVisitor;

public sealed record RegisterVisitorCommand(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);