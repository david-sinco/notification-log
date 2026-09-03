using FluentValidation;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

internal sealed class UpdateRecipientValidator : AbstractValidator<UpdateRecipientCommand>
{
    public UpdateRecipientValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();

        // A diferencia de Email/Phone/Attributes, un Name vacío no es un valor válido a aplicar
        // (el dominio no admite un Recipient sin nombre) — es NotEmpty siempre, no condicional.
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Recipient.NameMaxLength);

        RuleFor(x => x.Email)
            .MaximumLength(Recipient.EmailMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(Recipient.PhoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Locale)
            .MaximumLength(Recipient.LocaleMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Locale));

        RuleFor(x => x.TimeZone)
            .MaximumLength(Recipient.TimeZoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}
