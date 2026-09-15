using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileHandler
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateProfileCommand> _validator;

    public UpdateProfileHandler(IUserRepository users, IUnitOfWork uow, IValidator<UpdateProfileCommand> validator)
        => (_users, _uow, _validator) = (users, uow, validator);

    public async Task HandleAsync(UpdateProfileCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var user = await _users.LoadAsync(cmd.UserId, ct)
            ?? throw new NotFoundException(nameof(User), cmd.UserId);

        var name = PersonName.Create(cmd.Name);
        var email = Email.Create(cmd.Email);
        var phone = string.IsNullOrWhiteSpace(cmd.Phone) ? null : Phone.Create(cmd.Phone);

        if (email != user.Email)
        {
            if (!await _users.TryReserveEmailAsync(email, user.Id, ct))
                throw new AppValidationException($"El correo '{email}' ya está registrado.");

            await _users.ReleaseEmailAsync(user.Email, user.Id, ct);
        }

        user.ChangeName(name);
        user.ChangeEmail(email);

        if (phone is not null)
            user.ChangePhone(phone);

        await _users.AppendAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
