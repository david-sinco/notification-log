using FluentValidation;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

internal sealed class UpdateRecipientValidator : AbstractValidator<UpdateRecipientCommand>
{
    public UpdateRecipientValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Recipient.NameMaxLength)
            .When(x => x.Name is not null);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(Recipient.EmailMaxLength)
            .When(x => x.IsEmailVerified);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(Recipient.PhoneMaxLength)
            .When(x => x.IsPhoneVerified);

        RuleFor(x => x.Locale)
            .MaximumLength(Recipient.LocaleMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Locale));

        RuleFor(x => x.TimeZone)
            .MaximumLength(Recipient.TimeZoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}
