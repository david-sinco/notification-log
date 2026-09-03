using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;

public sealed class CreateRecipientHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateRecipientCommand> _validator;

    public CreateRecipientHandler(IRecipientRepository repo, IUnitOfWork uow, IValidator<CreateRecipientCommand> validator)
        => (_recipients, _uow, _validator) = (repo, uow, validator);

    public async Task HandleAsync(CreateRecipientCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        if (await _recipients.GetByIdAsync(cmd.RecipientId, ct) is not null)
            throw new AppValidationException($"El destinatario '{cmd.RecipientId}' ya existe.");

        var recipient = Recipient.Create(cmd.RecipientId, cmd.Name, cmd.Email, cmd.Phone);

        // Recipient.Create no toma locale/timeZone/attributes/isActive — se aplican con los mismos
        // métodos de dominio que usa UpdateRecipientHandler. ChangeLocalization en particular deja
        // el default del agregado (es-CO / America/Bogota) si viene vacío, en vez de perder el dato
        // si sí vino con un valor real.
        recipient.ChangeLocalization(cmd.Locale, cmd.TimeZone);
        recipient.ReplaceAttributes(cmd.Attributes ?? new Dictionary<string, string?>());

        if (cmd.IsActive)
            recipient.Activate();
        else
            recipient.Deactivate();

        await _recipients.AddAsync(recipient, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
