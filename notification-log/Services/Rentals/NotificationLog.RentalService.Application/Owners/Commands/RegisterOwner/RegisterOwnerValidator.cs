using FluentValidation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterOwner;

internal sealed class RegisterOwnerValidator : AbstractValidator<RegisterOwnerCommand>
{
    public RegisterOwnerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(OwnerName.MaxLength);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ContactInfo.MaxEmailLength);
        RuleFor(x => x.Phone).NotEmpty();
    }
}
