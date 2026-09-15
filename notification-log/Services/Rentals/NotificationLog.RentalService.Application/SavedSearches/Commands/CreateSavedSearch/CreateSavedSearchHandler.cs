using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.SavedSearches;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.CreateSavedSearch;

public sealed class CreateSavedSearchHandler
{
    private readonly ISavedSearchListRepository _searches;
    private readonly IIdentityReplica _identity;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateSavedSearchCommand> _validator;

    public CreateSavedSearchHandler(
        ISavedSearchListRepository searches,
        IIdentityReplica identity,
        IUnitOfWork uow,
        IValidator<CreateSavedSearchCommand> validator)
        => (_searches, _identity, _uow, _validator) = (searches, identity, uow, validator);

    public async Task<Guid> HandleAsync(CreateSavedSearchCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        if (cmd.Frequency != AlertFrequency.None && !await _identity.HasAlertsConsentAsync(cmd.UserId, ct))
            throw new AppValidationException("Para recibir alertas necesitas autorizar el envío de alertas.");

        var searches = await _searches.LoadAsync(SavedSearchListId.For(cmd.UserId), ct) ?? SavedSearchList.StartFor(cmd.UserId);
        var searchId = Guid.NewGuid();

        searches.Add(searchId, cmd.Name, cmd.Criteria.ToDomain(), cmd.Frequency);

        await _searches.AppendAsync(searches, ct);
        await _uow.SaveChangesAsync(ct);

        return searchId;
    }
}
