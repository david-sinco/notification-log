using FluentValidation;
using NotificationLog.UserService.Application.Abstractions;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.ConfirmPhone;

public sealed class ConfirmPhoneHandler
{
    private readonly IUserRepository _users;
    private readonly IVerificationCodeStore _codes;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ConfirmPhoneCommand> _validator;

    public ConfirmPhoneHandler(
        IUserRepository users, IVerificationCodeStore codes, IUnitOfWork uow, IValidator<ConfirmPhoneCommand> validator)
        => (_users, _codes, _uow, _validator) = (users, codes, uow, validator);

    public async Task HandleAsync(ConfirmPhoneCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var user = await _users.LoadAsync(cmd.UserId, ct)
            ?? throw new NotFoundException(nameof(User), cmd.UserId);

        var verified = await _codes.ConsumeAsync(user.Id, VerificationPurpose.Phone, cmd.Code, ct)
            ?? throw new AppValidationException("El código no es válido o ha caducado.");

        user.ConfirmPhone(Phone.Create(verified));

        await _users.AppendAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
