using FluentValidation;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

internal sealed class UpdateRecipientValidator : AbstractValidator<UpdateRecipientCommand>
{
    public UpdateRecipientValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();

        RuleFor(x => x.Email)
            .MaximumLength(Recipient.EmailMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(Recipient.PhoneMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
