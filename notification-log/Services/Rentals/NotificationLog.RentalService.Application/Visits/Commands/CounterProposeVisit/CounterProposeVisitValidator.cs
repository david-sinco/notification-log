using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;

internal sealed class CounterProposeVisitValidator : AbstractValidator<CounterProposeVisitCommand>
{
    public CounterProposeVisitValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Slots).NotEmpty();
        RuleFor(x => x.Slots.Count).LessThanOrEqualTo(VisitPolicy.MaxProposedSlots).When(x => x.Slots is not null);
    }
}
