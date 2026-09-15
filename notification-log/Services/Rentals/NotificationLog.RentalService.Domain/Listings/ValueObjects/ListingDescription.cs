using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Listings.ValueObjects;

public sealed record ListingDescription
{
    public string Value { get; }

    private ListingDescription(string value) => Value = value;

    public static ListingDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("La descripción es obligatoria.");

        var normalized = value.Trim();

        if (normalized.Length < ListingPolicy.MinDescriptionLength)
            throw new DomainException(
                $"La descripción debe tener al menos {ListingPolicy.MinDescriptionLength} caracteres.");

        if (normalized.Length > ListingPolicy.MaxDescriptionLength)
            throw new DomainException(
                $"La descripción no puede superar {ListingPolicy.MaxDescriptionLength} caracteres.");

        return new ListingDescription(normalized);
    }

    internal static ListingDescription FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
