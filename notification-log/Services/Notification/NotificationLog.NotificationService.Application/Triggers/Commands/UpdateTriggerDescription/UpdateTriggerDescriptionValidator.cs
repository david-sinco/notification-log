using FluentValidation;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.UpdateTriggerDescription;

internal sealed class UpdateTriggerDescriptionValidator : AbstractValidator<UpdateTriggerDescriptionCommand>
{
    public UpdateTriggerDescriptionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(NotificationTrigger.DescriptionMaxLength);
    }
}
