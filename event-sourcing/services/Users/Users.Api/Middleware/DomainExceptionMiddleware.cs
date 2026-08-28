using Microsoft.AspNetCore.Http;
using Users.Domain;

namespace Users.Api.Middleware;

/// <summary>
/// Maps the domain exception vocabulary to HTTP status codes (SPEC.md §4):
/// EmailAlreadyInUseException/ConcurrencyConflictException -> 409, UserNotFoundException -> 404,
/// any other DomainException -> 422.
/// </summary>
public sealed class DomainExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UserNotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (EmailAlreadyInUseException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ConcurrencyConflictException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DomainException ex)
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
