using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;

internal sealed class CancelVisitValidator : AbstractValidator<CancelVisitCommand>
{
    public CancelVisitValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(VisitPolicy.MaxReasonLength);
    }
}
