using FluentValidation;
using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.CreateSavedSearch;

internal sealed class CreateSavedSearchValidator : AbstractValidator<CreateSavedSearchCommand>
{
    public CreateSavedSearchValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(SavedSearchList.MaxNameLength);
        RuleFor(x => x.Criteria).NotNull().SetValidator(new SearchCriteriaInputValidator());
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
