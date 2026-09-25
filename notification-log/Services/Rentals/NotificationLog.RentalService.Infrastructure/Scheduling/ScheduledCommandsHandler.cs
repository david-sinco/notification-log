using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;

namespace NotificationLog.RentalService.Infrastructure.Scheduling;

public static class ScheduledCommandsHandler
{

    public static Task Handle(ExpireListingCommand command, ExpireListingHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);
}
