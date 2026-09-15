using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Inquiries.ValueObjects;

public static class InquiryId
{
    private static readonly Guid Namespace = new("6f0d2c1e-8b4a-4e7f-9a53-2c7e1d4b9f08");

    public static Guid For(Guid listingId, Guid seekerId)
        => NameBasedGuid.Create(Namespace, $"{listingId:N}{seekerId:N}");
}
