using FluentValidation;

namespace NotificationLog.RentalService.Application.Visits.Commands.ScheduleVisit;

internal sealed class ScheduleVisitValidator : AbstractValidator<ScheduleVisitCommand>
{
    public ScheduleVisitValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.StartsAt).NotEmpty();
    }
}
