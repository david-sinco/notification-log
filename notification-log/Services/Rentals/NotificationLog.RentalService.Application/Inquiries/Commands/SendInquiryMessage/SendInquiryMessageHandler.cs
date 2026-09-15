using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Inquiries;
using NotificationLog.RentalService.Domain.Inquiries.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Inquiries.Commands.SendInquiryMessage;

public sealed class SendInquiryMessageHandler
{
    private readonly IListingRepository _listings;
    private readonly IInquiryRepository _inquiries;
    private readonly ListingAccess _access;
    private readonly ISoftRuleChecks _rules;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SendInquiryMessageCommand> _validator;
    private readonly TimeProvider _time;

    public SendInquiryMessageHandler(
        IListingRepository listings,
        IInquiryRepository inquiries,
        ListingAccess access,
        ISoftRuleChecks rules,
        IUnitOfWork uow,
        IValidator<SendInquiryMessageCommand> validator,
        TimeProvider time)
        => (_listings, _inquiries, _access, _rules, _uow, _validator, _time) = (listings, inquiries, access, rules, uow, validator, time);

    public async Task<Guid> HandleAsync(SendInquiryMessageCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (listing.Status != ListingStatus.Published)
            throw new AppValidationException("La publicación no está disponible para consultas.");

        if (await _access.CanManageAsync(listing, cmd.SeekerId, ct))
            throw new AppValidationException("No puedes consultar tu propia publicación.");

        await _access.RequireVerifiedUserAsync(cmd.SeekerId, requireDocument: false, ct);

        var message = InquiryMessage.Create(cmd.Message);
        var inquiry = await _inquiries.LoadAsync(InquiryId.For(listing.Id, cmd.SeekerId), ct);

        if (inquiry is null)
        {
            var since = _time.GetUtcNow().AddDays(-1);

            if (await _rules.CountInquiriesOpenedSinceAsync(cmd.SeekerId, since, ct) >= Inquiry.MaxNewInquiriesPerDay)
                throw new AppValidationException(
                    $"Puedes abrir como máximo {Inquiry.MaxNewInquiriesPerDay} consultas nuevas al día.");

            inquiry = Inquiry.Open(listing.Id, cmd.SeekerId, message);
        }
        else
        {
            inquiry.Post(cmd.SeekerId, message);
        }

        await _inquiries.AppendAsync(inquiry, ct);
        await _uow.SaveChangesAsync(ct);

        return inquiry.Id;
    }
}
