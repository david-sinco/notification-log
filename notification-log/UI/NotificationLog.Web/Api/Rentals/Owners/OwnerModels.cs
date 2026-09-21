namespace NotificationLog.Web.Api.Rentals.Owners;

public enum DocumentType
{
    CitizenshipCard = 0,
    ForeignerId = 1,
    Passport = 2
}

public sealed record OwnerDto(
    Guid Id,
    Guid CreatedBy,
    string Type,
    string? FirstNames,
    string? LastNames,
    string? DocumentType,
    string? DocumentNumber,
    string? LegalName,
    string? Nit,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt);

public sealed record RegisterNaturalOwnerRequest(
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone);

public sealed record RegisterCompanyOwnerRequest(string LegalName, string Nit, string Email, string Phone);
