namespace NotificationLog.NotificationService.Application.Triggers.Commands.SetTriggerStatus;

public sealed record SetTriggerStatusCommand(Guid Id, bool IsEnabled);
