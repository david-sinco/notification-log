using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

internal sealed class RequestVisitValidator : AbstractValidator<RequestVisitCommand>
{
    public RequestVisitValidator()
    {
        RuleFor(x => x.VisitorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.SlotStarts).NotEmpty().Must(slots => slots.Count <= VisitPolicy.MaxSlots)
            .WithMessage($"Hay que proponer entre 1 y {VisitPolicy.MaxSlots} franjas.");
    }
}
