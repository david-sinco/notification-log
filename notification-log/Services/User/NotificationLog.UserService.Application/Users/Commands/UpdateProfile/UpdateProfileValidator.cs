using FluentValidation;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.UpdateProfile;

internal sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(PersonName.MaxLength);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(Email.MaxLength);
    }
}
