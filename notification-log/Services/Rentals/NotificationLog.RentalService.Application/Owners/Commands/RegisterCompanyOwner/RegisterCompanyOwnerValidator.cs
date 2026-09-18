using FluentValidation;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;

internal sealed class RegisterCompanyOwnerValidator : AbstractValidator<RegisterCompanyOwnerCommand>
{
    public RegisterCompanyOwnerValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty();
        RuleFor(x => x.Nit).NotEmpty();
    }
}
