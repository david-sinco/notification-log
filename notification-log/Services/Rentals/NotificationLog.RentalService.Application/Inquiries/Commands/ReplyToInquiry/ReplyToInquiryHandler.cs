using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Inquiries;
using NotificationLog.RentalService.Domain.Inquiries.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Inquiries.Commands.ReplyToInquiry;

public sealed class ReplyToInquiryHandler
{
    private readonly IInquiryRepository _inquiries;
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ReplyToInquiryCommand> _validator;

    public ReplyToInquiryHandler(
        IInquiryRepository inquiries,
        IListingRepository listings,
        ListingAccess access,
        IUnitOfWork uow,
        IValidator<ReplyToInquiryCommand> validator)
        => (_inquiries, _listings, _access, _uow, _validator) = (inquiries, listings, access, uow, validator);

    public async Task HandleAsync(ReplyToInquiryCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var inquiry = await _inquiries.GetAsync(cmd.InquiryId, ct);
        var listing = await _listings.GetAsync(inquiry.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        inquiry.Post(cmd.ActorId, InquiryMessage.Create(cmd.Message));

        await _inquiries.AppendAsync(inquiry, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
