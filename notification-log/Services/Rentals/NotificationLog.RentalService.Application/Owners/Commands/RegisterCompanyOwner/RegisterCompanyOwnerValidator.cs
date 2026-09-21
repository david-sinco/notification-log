using FluentValidation;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;

internal sealed class RegisterCompanyOwnerValidator : AbstractValidator<RegisterCompanyOwnerCommand>
{
    public RegisterCompanyOwnerValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty();
        RuleFor(x => x.Nit).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ContactInfo.MaxEmailLength);
        RuleFor(x => x.Phone).NotEmpty();
    }
}
