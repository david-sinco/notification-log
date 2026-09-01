using FluentValidation;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;

internal sealed class UpdateRecipientProfileValidator : AbstractValidator<UpdateRecipientProfileCommand>
{
    public UpdateRecipientProfileValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Recipient.NameMaxLength);
    }
}
