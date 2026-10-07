namespace NotificationLog.RentalService.Api.Contracts.Owners;

public sealed record RegisterOwnerRequest(string Name, string Email, string Phone);
