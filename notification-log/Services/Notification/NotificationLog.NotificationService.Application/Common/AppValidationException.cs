namespace NotificationLog.NotificationService.Application.Common;

public sealed class AppValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public AppValidationException(string message)
        : base(message)
        => Errors = new Dictionary<string, string[]>();

    public AppValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Se encontraron uno o más errores de validación.")
        => Errors = errors;
}