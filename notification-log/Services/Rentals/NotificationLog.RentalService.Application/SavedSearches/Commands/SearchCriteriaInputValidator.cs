using FluentValidation;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands;

internal sealed class SearchCriteriaInputValidator : AbstractValidator<SearchCriteriaInput>
{
    public SearchCriteriaInputValidator()
    {
        RuleFor(x => x.Operation).IsInEnum();
        RuleFor(x => x.City).NotEmpty().MaximumLength(SearchCriteria.MaxCityLength);
        RuleFor(x => x.Neighborhoods).NotNull().Must(neighborhoods => neighborhoods.Count <= SearchCriteria.MaxNeighborhoods)
            .WithMessage($"Una búsqueda admite como máximo {SearchCriteria.MaxNeighborhoods} barrios.");
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.MinBedrooms).GreaterThanOrEqualTo(0).When(x => x.MinBedrooms.HasValue);
        RuleFor(x => x.MinStratum).InclusiveBetween(Stratum.Min, Stratum.Max).When(x => x.MinStratum.HasValue);
        RuleFor(x => x.MaxStratum).InclusiveBetween(Stratum.Min, Stratum.Max).When(x => x.MaxStratum.HasValue);
        RuleFor(x => x.MinArea).GreaterThan(0).When(x => x.MinArea.HasValue);
    }
}
