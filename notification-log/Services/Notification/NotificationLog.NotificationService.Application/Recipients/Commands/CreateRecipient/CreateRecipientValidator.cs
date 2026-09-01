using FluentValidation;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;

internal sealed class CreateRecipientValidator : AbstractValidator<CreateRecipientCommand>
{
    public CreateRecipientValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Recipient.NameMaxLength);
    }
}