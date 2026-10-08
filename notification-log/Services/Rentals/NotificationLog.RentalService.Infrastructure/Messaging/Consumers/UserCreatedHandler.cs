using Domain.Shared.Authorization;
using Microsoft.Extensions.Logging;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Visitors.Commands.UpsertVisitor;
using NotificationLog.RentalService.Domain.Visitors.Exceptions;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public sealed class UserCreatedHandler(UpsertVisitorHandler handler, ILogger<UserCreatedHandler> logger)
{
    private readonly UpsertVisitorHandler _handler = handler;
    private readonly ILogger<UserCreatedHandler> _logger = logger;

    public async Task Handle(UserCreated message, CancellationToken ct)
    {
        if (!Enum.TryParse<UserRole>(message.Role, out var role))
            return;

        try
        {
            await _handler.HandleAsync(
                new UpsertVisitorCommand(Guid.Parse(message.UserId), role, message.Name, message.Email, message.Phone),
                ct);
        }
        catch (VisitorUnchangedException ex)
        {
            _logger.LogInformation("{Message} Se ignora el mensaje {EventId}.", ex.Message, message.EventId);
        }
    }
}
