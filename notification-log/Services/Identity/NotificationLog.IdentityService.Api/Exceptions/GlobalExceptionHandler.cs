using Application.Shared.Common;
using Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NotificationLog.IdentityService.Api.Exceptions;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var problem = exception switch
        {
            NotFoundException e => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = e.Message
            },

            AppValidationException e => BuildValidationProblem(e),

            ForbiddenException e => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acceso denegado",
                Detail = e.Message
            },

            DomainException e => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Regla de negocio violada",
                Detail = e.Message
            },

            _ => null
        };

        if (problem is null)
        {
            _logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);
            return false;
        }

        problem.Instance = context.Request.Path;

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }

    private static ProblemDetails BuildValidationProblem(AppValidationException exception)
    {
        if (exception.Errors.Count == 0)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Solicitud inválida",
                Detail = exception.Message
            };
        }

        return new ValidationProblemDetails(
            exception.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Errores de validación"
        };
    }
}
