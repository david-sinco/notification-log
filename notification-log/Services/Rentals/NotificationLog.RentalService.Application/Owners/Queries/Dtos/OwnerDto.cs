namespace NotificationLog.RentalService.Application.Owners.Queries.Dtos;

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
