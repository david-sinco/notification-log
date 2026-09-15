using Domain.Shared.Common;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

namespace NotificationLog.RentalService.Domain.SavedSearches;

public sealed class SavedSearch : Entity
{
    internal SavedSearch(Guid id, string name, SearchCriteria criteria, AlertFrequency frequency) : base(id)
    {
        Name = name;
        Criteria = criteria;
        Frequency = frequency;
    }

    public string Name { get; private set; }
    public SearchCriteria Criteria { get; private set; }
    public AlertFrequency Frequency { get; private set; }

    internal void Change(string name, SearchCriteria criteria, AlertFrequency frequency)
    {
        Name = name;
        Criteria = criteria;
        Frequency = frequency;
    }
}
