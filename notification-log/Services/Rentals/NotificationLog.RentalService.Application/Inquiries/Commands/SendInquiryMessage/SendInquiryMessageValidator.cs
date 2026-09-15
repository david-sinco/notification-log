using FluentValidation;
using NotificationLog.RentalService.Domain.Inquiries.ValueObjects;

namespace NotificationLog.RentalService.Application.Inquiries.Commands.SendInquiryMessage;

internal sealed class SendInquiryMessageValidator : AbstractValidator<SendInquiryMessageCommand>
{
    public SendInquiryMessageValidator()
    {
        RuleFor(x => x.SeekerId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(InquiryMessage.MaxLength);
    }
}
