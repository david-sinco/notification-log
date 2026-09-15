using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

public sealed class UpdateRecipientHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateRecipientCommand> _validator;

    public UpdateRecipientHandler(
        IRecipientRepository recipients, IUnitOfWork uow, IValidator<UpdateRecipientCommand> validator)
        => (_recipients, _uow, _validator) = (recipients, uow, validator);

    public async Task HandleAsync(UpdateRecipientCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        // cmd es un snapshot completo, no un delta: no hay "no vino en el mensaje", cada campo se
        // aplica siempre tal cual llegó. Vacío no significa "no tocar" — significa que el dato ya
        // no está confirmado, y se refleja como tal. Name es la única excepción real: el dominio no
        // admite un Recipient sin nombre, así que un Name vacío no es un valor a aplicar, es un
        // mensaje inválido — se rechaza en el validador (NotEmpty), no acá.
        recipient.ChangeName(cmd.Name!);

        // ChangeLocalization tiene su propio criterio (mantener el valor con default si no viene
        // uno nuevo) porque Locale/TimeZone no son "datos que se confirman o se borran" como
        // email/phone — siempre tienen un valor válido, nunca están "vacíos" en el sentido de
        // "eliminados".
        recipient.ChangeLocalization(cmd.Locale, cmd.TimeZone);

        recipient.ChangeEmail(cmd.Email);
        recipient.ChangePhone(cmd.Phone);
        recipient.ReplaceAttributes(cmd.Attributes ?? new Dictionary<string, string?>());

        if (cmd.IsActive)
            recipient.Activate();
        else
            recipient.Deactivate();

        await _uow.SaveChangesAsync(ct);
    }
}
