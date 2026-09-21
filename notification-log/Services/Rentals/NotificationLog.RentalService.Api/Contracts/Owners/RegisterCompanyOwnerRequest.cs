namespace NotificationLog.RentalService.Api.Contracts.Owners;

public sealed record RegisterCompanyOwnerRequest(string LegalName, string Nit, string Email, string Phone);
