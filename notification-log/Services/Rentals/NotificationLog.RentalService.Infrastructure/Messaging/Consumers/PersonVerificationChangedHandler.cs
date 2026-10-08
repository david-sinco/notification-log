using Microsoft.Extensions.Logging;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Visitors.Commands.UpsertVisitor;
using NotificationLog.RentalService.Domain.Visitors.Exceptions;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public sealed class PersonVerificationChangedHandler(
    UpsertVisitorHandler handler,
    ILogger<PersonVerificationChangedHandler> logger)
{
    private readonly UpsertVisitorHandler _handler = handler;
    private readonly ILogger<PersonVerificationChangedHandler> _logger = logger;

    public async Task Handle(PersonVerificationChanged message, CancellationToken ct)
    {
        try
        {
            await _handler.HandleAsync(
                new UpsertVisitorCommand(Guid.Parse(message.UserId), null, string.Empty, message.Email, message.Phone),
                ct);
        }
        catch (VisitorUnchangedException ex)
        {
            _logger.LogInformation("{Message} Se ignora el mensaje {EventId}.", ex.Message, message.EventId);
        }
    }
}
