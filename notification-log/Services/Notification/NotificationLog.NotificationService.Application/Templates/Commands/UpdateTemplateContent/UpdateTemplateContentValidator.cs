using FluentValidation;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.UpdateTemplateContent;

internal sealed class UpdateTemplateContentValidator: AbstractValidator<UpdateTemplateContentCommand>
{
    public UpdateTemplateContentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(NotificationTemplate.BodyMaxLength);
    }
}