using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.DeclineVisit;

internal sealed class DeclineVisitValidator : AbstractValidator<DeclineVisitCommand>
{
    public DeclineVisitValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(VisitPolicy.MaxReasonLength);
    }
}
