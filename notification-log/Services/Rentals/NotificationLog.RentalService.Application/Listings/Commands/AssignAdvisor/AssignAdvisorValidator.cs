using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.AssignAdvisor;

internal sealed class AssignAdvisorValidator : AbstractValidator<AssignAdvisorCommand>
{
    public AssignAdvisorValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.AdvisorId).NotEqual(Guid.Empty).When(x => x.AdvisorId.HasValue);
    }
}
