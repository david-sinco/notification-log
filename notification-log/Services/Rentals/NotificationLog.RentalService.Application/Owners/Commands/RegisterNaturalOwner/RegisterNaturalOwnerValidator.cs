using FluentValidation;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;

internal sealed class RegisterNaturalOwnerValidator : AbstractValidator<RegisterNaturalOwnerCommand>
{
    public RegisterNaturalOwnerValidator()
    {
        RuleFor(x => x.FirstNames).NotEmpty();
        RuleFor(x => x.LastNames).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).NotEmpty();
    }
}
