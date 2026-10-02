using NotificationLog.Web.Api.Rentals.Owners;

namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record CompleteVisitorProfileRequest(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);
