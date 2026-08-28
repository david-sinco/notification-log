using Microsoft.AspNetCore.Http;
using Notifications.Domain;

namespace Notifications.Api.Middleware;

/// <summary>
/// UnknownUserException -> 404, DuplicateNotificationException -> 409, any other
/// NotificationDomainException -> 422.
/// </summary>
public sealed class DomainExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnknownUserException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (DuplicateNotificationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (NotificationDomainException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { title = "Command rejected", status = statusCode, detail });
    }
}
