using FluentValidation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;

internal sealed class CompleteVisitorProfileValidator : AbstractValidator<CompleteVisitorProfileCommand>
{
    public CompleteVisitorProfileValidator()
    {
        RuleFor(x => x.FirstNames).NotEmpty();
        RuleFor(x => x.LastNames).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ContactInfo.MaxEmailLength);
        RuleFor(x => x.Phone).NotEmpty();
    }
}
