using FluentValidation;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;

internal sealed class RegisterNaturalOwnerValidator : AbstractValidator<RegisterNaturalOwnerCommand>
{
    public RegisterNaturalOwnerValidator()
    {
        RuleFor(x => x.FirstNames).NotEmpty();
        RuleFor(x => x.LastNames).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ContactInfo.MaxEmailLength);
        RuleFor(x => x.Phone).NotEmpty();
    }
}
