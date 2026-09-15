using FluentValidation;

namespace NotificationLog.UserService.Application.Users.Commands.ConfirmEmail;

internal sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
    }
}
