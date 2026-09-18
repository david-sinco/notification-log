namespace NotificationLog.RentalService.Application.Owners.Queries.ListOwners;

public sealed record ListOwnersQuery(
    Guid? CreatedBy = null,
    int Page = 1,
    int PageSize = 20);
