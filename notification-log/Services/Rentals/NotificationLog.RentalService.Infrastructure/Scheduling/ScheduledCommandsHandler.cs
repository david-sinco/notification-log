using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireReservation;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Application.Offers.Commands.ExpireOffer;
using NotificationLog.RentalService.Application.Visits.Commands.AutoCompleteVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ExpireVisitRequest;
using NotificationLog.RentalService.Application.Visits.Commands.SendVisitReminder;

namespace NotificationLog.RentalService.Infrastructure.Scheduling;

public static class ScheduledCommandsHandler
{
    public static Task Handle(WarnListingExpiryCommand command, WarnListingExpiryHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(ExpireListingCommand command, ExpireListingHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(ExpireReservationCommand command, ExpireReservationHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(ExpireOfferCommand command, ExpireOfferHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(ExpireVisitRequestCommand command, ExpireVisitRequestHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(SendVisitReminderCommand command, SendVisitReminderHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);

    public static Task Handle(AutoCompleteVisitCommand command, AutoCompleteVisitHandler handler, CancellationToken ct)
        => handler.HandleAsync(command, ct);
}
