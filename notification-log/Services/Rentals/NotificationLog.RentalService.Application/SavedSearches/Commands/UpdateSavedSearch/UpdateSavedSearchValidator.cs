using FluentValidation;
using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.UpdateSavedSearch;

internal sealed class UpdateSavedSearchValidator : AbstractValidator<UpdateSavedSearchCommand>
{
    public UpdateSavedSearchValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SearchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(SavedSearchList.MaxNameLength);
        RuleFor(x => x.Criteria).NotNull().SetValidator(new SearchCriteriaInputValidator());
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
