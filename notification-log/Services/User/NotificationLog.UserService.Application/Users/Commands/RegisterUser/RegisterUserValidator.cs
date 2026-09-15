using FluentValidation;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(PersonName.MaxLength);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(Email.MaxLength);
    }
}
