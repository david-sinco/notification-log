using FluentValidation;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.PublishTemplateVersion;

internal sealed class PublishTemplateVersionValidator
    : AbstractValidator<PublishTemplateVersionCommand>
{
    public PublishTemplateVersionValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(TemplateVersion.BodyMaxLength);
    }
}
