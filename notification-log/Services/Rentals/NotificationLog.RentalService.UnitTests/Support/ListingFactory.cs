using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class ListingFactory
{
    public static readonly Guid Owner = Guid.NewGuid();
    public static readonly Guid Moderator = Guid.NewGuid();
    public static readonly DateTimeOffset ExpiresAt = Clock.Now.AddMonths(ListingPolicy.ValidityMonths);

    public static Listing Draft(Operation operation = Operation.Rent)
        => Clean(Listing.Draft(Guid.NewGuid(), Owner, Owner, operation));

    public static Listing Complete(int photos = ListingPolicy.MinPhotos)
    {
        var listing = Draft();
        listing.UpdateDetails(SampleDetails(), SampleLocation(), SampleDescription());
        listing.ChangePrice(Money.Create(1_800_000), Clock.Now);

        for (var i = 0; i < photos; i++)
            listing.AddPhoto(NewPhoto());

        return Clean(listing);
    }

    public static Listing InStatus(ListingStatus status)
    {
        var listing = Complete();

        if (status == ListingStatus.Draft)
            return listing;

        listing.SubmitForReview();

        if (status == ListingStatus.InReview)
            return Clean(listing);

        listing.Approve(Moderator, Clock.Now);

        switch (status)
        {
            case ListingStatus.Paused:
                listing.Pause();
                break;
            case ListingStatus.Expired:
                listing.Expire(ExpiresAt);
                break;
            case ListingStatus.Closed:
                listing.Close(Money.Create(1_750_000), new DateOnly(2026, 10, 4), Clock.Now);
                break;
            case ListingStatus.Withdrawn:
                listing.Withdraw("Decidí no arrendar");
                break;
            case ListingStatus.Suspended:
                listing.Suspend("Denuncia por datos falsos");
                break;
        }

        return Clean(listing);
    }

    public static PropertyDetails SampleDetails()
        => PropertyDetails.Create(PropertyType.Apartment, 68m, 2, 2, 1, Stratum.Create(4), 5, true, Money.Create(320_000));

    public static Location SampleLocation(string neighborhood = "Chapinero")
        => Location.Create("Bogotá", neighborhood, "Calle 60 # 9-45");

    public static ListingDescription SampleDescription()
        => ListingDescription.Create(new string('a', ListingPolicy.MinDescriptionLength));

    public static Photo NewPhoto()
    {
        using var content = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

        return Photo.FromStreamAsync(content).GetAwaiter().GetResult();
    }

    private static Listing Clean(Listing listing)
    {
        listing.ClearDomainEvents();

        return listing;
    }
}
