using FluentValidation;
using NotificationLog.RentalService.Application.Visitors.Commands.RegisterVisitor;
using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.Application.VIsitors.Commands.RegisterVisitor;

internal sealed class RegisterVisitorValidator : AbstractValidator<RegisterVisitorCommand>
{
    public RegisterVisitorValidator()
    {
        RuleFor(x => x.FirstNames).NotEmpty();
        RuleFor(x => x.LastNames).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ContactInfo.MaxEmailLength);
        RuleFor(x => x.Phone).NotEmpty();
    }
}
