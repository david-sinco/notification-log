using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;

namespace NotificationLog.RentalService.Infrastructure.Scheduling;

public static class ScheduledCommandsHandler
{
    public static Task Handle(WarnListingExpiryCommand command, WarnListingExpiryHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(ExpireListingCommand command, ExpireListingHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);
}
