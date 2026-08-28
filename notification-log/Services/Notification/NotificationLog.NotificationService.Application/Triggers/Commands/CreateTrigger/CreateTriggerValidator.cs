using FluentValidation;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;

internal sealed class CreateTriggerValidator : AbstractValidator<CreateTriggerCommand>
{
    public CreateTriggerValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(NotificationTrigger.DescriptionMaxLength);
        
        RuleFor(x => x.EventKey)
            .NotEmpty()
            .MaximumLength(EventKey.MaxLength);
    }
}