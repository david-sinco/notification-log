using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.CancelReservation;

internal sealed class CancelReservationValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(ListingPolicy.MaxReasonLength);
    }
}
