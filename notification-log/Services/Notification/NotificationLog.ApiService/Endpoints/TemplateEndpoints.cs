using NotificationLog.ApiService.Contracts.Templates;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;
using NotificationLog.NotificationService.Application.Templates.Commands.PublishTemplateVersion;
using NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;
using NotificationLog.NotificationService.Application.Templates.Dtos;
using NotificationLog.NotificationService.Application.Templates.Queries.GetTemplateById;
using NotificationLog.NotificationService.Application.Templates.Queries.ListTemplates;

namespace NotificationLog.ApiService.Endpoints;

public static class TemplateEndpoints
{
    public static IEndpointRouteBuilder MapTemplates(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/templates").WithTags("Templates");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTemplate")
            .WithSummary("Crea una plantilla de notificación")
            .Produces<CreatedTemplateResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTemplateById")
            .WithSummary("Obtiene una plantilla por su identificador")
            .Produces<TemplateDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListAsync)
            .WithName("ListTemplates")
            .WithSummary("Lista plantillas con filtros y paginación")
            .Produces<PagedResult<TemplateSummaryDto>>();

        group.MapPost("/{id:guid}/versions", PublishVersionAsync)
            .WithName("PublishTemplateVersion")
            .WithSummary("Publica una nueva versión (asunto y cuerpo) de una plantilla")
            .Produces<PublishedTemplateVersionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/status", SetStatusAsync)
            .WithName("SetTemplateStatus")
            .WithSummary("Habilita o deshabilita una plantilla")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateTemplateRequest body,
        CreateTemplateHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(
            new CreateTemplateCommand(body.Name, body.Channel, body.Subject, body.Body), ct);

        return Results.CreatedAtRoute(
            "GetTemplateById", new { id }, new CreatedTemplateResponse(id));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id, GetTemplateByIdHandler handler, CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetTemplateByIdQuery(id), ct));

    private static async Task<IResult> ListAsync(
        [AsParameters] ListTemplatesQuery query,
        ListTemplatesHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> PublishVersionAsync(
        Guid id,
        PublishTemplateVersionRequest body,
        PublishTemplateVersionHandler handler,
        CancellationToken ct)
    {
        var versionId = await handler.HandleAsync(
            new PublishTemplateVersionCommand(id, body.Subject, body.Body), ct);

        return Results.Created(
            $"/api/templates/{id}/versions/{versionId}",
            new PublishedTemplateVersionResponse(versionId));
    }

    private static async Task<IResult> SetStatusAsync(
        Guid id,
        SetTemplateStatusRequest body,
        SetTemplateStatusHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SetTemplateStatusCommand(id, body.IsEnabled), ct);
        return Results.NoContent();
    }
}