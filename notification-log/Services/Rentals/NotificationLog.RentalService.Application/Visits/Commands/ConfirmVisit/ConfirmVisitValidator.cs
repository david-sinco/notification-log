using FluentValidation;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.ConfirmVisit;

internal sealed class ConfirmVisitValidator : AbstractValidator<ConfirmVisitCommand>
{
    public ConfirmVisitValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
    }
}
