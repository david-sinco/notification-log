using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Shared;

internal static class Guard
{
    public static void RequireId(Guid id, string message)
    {
        if (id == Guid.Empty)
            throw new DomainException(message);
    }

    public static string RequireReason(string reason, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Hay que indicar el motivo.");

        var normalized = reason.Trim();

        if (normalized.Length > maxLength)
            throw new DomainException($"El motivo no puede superar {maxLength} caracteres.");

        return normalized;
    }
}
