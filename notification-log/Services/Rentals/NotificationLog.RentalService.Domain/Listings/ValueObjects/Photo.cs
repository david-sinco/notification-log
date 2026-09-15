using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Listings.ValueObjects;

public sealed record Photo
{
    public const int MaxReferenceLength = 500;

    public string Reference { get; }

    private Photo(string reference) => Reference = reference;

    public static Photo Create(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new DomainException("La referencia de la foto es obligatoria.");

        var normalized = reference.Trim();

        if (normalized.Length > MaxReferenceLength)
            throw new DomainException($"La referencia de la foto no puede superar {MaxReferenceLength} caracteres.");

        return new Photo(normalized);
    }

    internal static Photo FromStorage(string reference) => new(reference);

    public override string ToString() => Reference;
}
