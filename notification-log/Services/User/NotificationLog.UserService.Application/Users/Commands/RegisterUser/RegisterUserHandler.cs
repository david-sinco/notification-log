using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(IUserRepository users, IUnitOfWork uow, IValidator<RegisterUserCommand> validator)
        => (_users, _uow, _validator) = (users, uow, validator);

    public async Task<Guid> HandleAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var name = PersonName.Create(cmd.Name);
        var email = Email.Create(cmd.Email);
        var phone = string.IsNullOrWhiteSpace(cmd.Phone) ? null : Phone.Create(cmd.Phone);

        var id = Guid.NewGuid();

        if (!await _users.TryReserveEmailAsync(email, id, ct))
            throw new AppValidationException($"El correo '{email}' ya está registrado.");

        var user = User.Register(id, name, email, phone);

        await _users.AppendAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return user.Id;
    }
}
