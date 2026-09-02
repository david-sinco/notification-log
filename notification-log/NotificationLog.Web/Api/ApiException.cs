using System.Net;

namespace NotificationLog.Web.Api;

public sealed class ApiException(
    HttpStatusCode statusCode,
    string? title,
    string? detail,
    IReadOnlyDictionary<string, string[]>? fieldErrors = null)
    : Exception(detail ?? title ?? "Ocurrió un error al comunicarse con el servicio.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? Title { get; } = title;
    public IReadOnlyDictionary<string, string[]>? FieldErrors { get; } = fieldErrors;
}
