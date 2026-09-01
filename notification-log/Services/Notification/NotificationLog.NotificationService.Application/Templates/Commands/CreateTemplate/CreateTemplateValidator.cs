using FluentValidation;
using NotificationLog.NotificationService.Domain.Shared;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;

internal sealed class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    private static readonly NotificationChannel[] ChannelsWithSubject =
        [NotificationChannel.Email, NotificationChannel.Push];

    public CreateTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(TemplateName.MaxLength);
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(TemplateVersion.BodyMaxLength);

        When(x => ChannelsWithSubject.Contains(x.Channel), () =>
        {
            RuleFor(x => x.Subject)
                .NotEmpty()
                .MaximumLength(TemplateVersion.SubjectMaxLength)
                .WithMessage("Las plantillas de este canal requieren asunto.");
        })
        .Otherwise(() =>
        {
            RuleFor(x => x.Subject)
                .Empty()
                .WithMessage("Las plantillas de este canal no admiten asunto.");
        });
    }
}
