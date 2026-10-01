using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

internal sealed class RequestVisitValidator : AbstractValidator<RequestVisitCommand>
{
    public RequestVisitValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Slots).NotEmpty();
        RuleFor(x => x.Slots.Count).LessThanOrEqualTo(VisitPolicy.MaxProposedSlots).When(x => x.Slots is not null);
    }
}
