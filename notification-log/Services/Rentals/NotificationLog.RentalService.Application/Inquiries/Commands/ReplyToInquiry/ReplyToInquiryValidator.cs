using FluentValidation;
using NotificationLog.RentalService.Domain.Inquiries.ValueObjects;

namespace NotificationLog.RentalService.Application.Inquiries.Commands.ReplyToInquiry;

internal sealed class ReplyToInquiryValidator : AbstractValidator<ReplyToInquiryCommand>
{
    public ReplyToInquiryValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.InquiryId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(InquiryMessage.MaxLength);
    }
}
