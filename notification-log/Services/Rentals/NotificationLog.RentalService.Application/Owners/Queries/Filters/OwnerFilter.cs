using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Application.Owners.Queries.Filters;

public sealed record OwnerFilter(Guid? CreatedBy = null, string? Search = null, OwnerType? Type = null);
