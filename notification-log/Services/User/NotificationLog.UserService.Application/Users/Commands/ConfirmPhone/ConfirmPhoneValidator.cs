using FluentValidation;

namespace NotificationLog.UserService.Application.Users.Commands.ConfirmPhone;

internal sealed class ConfirmPhoneValidator : AbstractValidator<ConfirmPhoneCommand>
{
    public ConfirmPhoneValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
    }
}
