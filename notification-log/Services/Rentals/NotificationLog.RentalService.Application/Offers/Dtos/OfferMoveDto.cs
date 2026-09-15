namespace NotificationLog.RentalService.Application.Offers.Dtos;

public sealed record OfferMoveDto(string Kind, string? By, long? Amount, string? Reason, DateTimeOffset At);
