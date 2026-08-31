using NotificationLog.ApiService.Contracts.Triggers;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Triggers.Commands.AddConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.ChangeConfigurationTemplate;
using NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;
using NotificationLog.NotificationService.Application.Triggers.Commands.DisableConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.EnableConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.SetTriggerStatus;
using NotificationLog.NotificationService.Application.Triggers.Commands.UpdateTriggerDescription;
using NotificationLog.NotificationService.Application.Triggers.Dtos;
using NotificationLog.NotificationService.Application.Triggers.Queries.GetTriggerById;
using NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

namespace NotificationLog.ApiService.Endpoints;

public static class TriggerEndpoints
{
    public static IEndpointRouteBuilder MapTriggers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/triggers").WithTags("Triggers");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTrigger")
            .WithSummary("Crea un trigger de notificación")
            .Produces<CreatedTriggerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTriggerById")
            .WithSummary("Obtiene un trigger por su identificador")
            .Produces<TriggerDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListAsync)
            .WithName("ListTriggers")
            .WithSummary("Lista triggers con filtros y paginación")
            .Produces<PagedResult<TriggerSummaryDto>>();

        group.MapPatch("/{id:guid}/status", SetStatusAsync)
            .WithName("SetTriggerStatus")
            .WithSummary("Habilita o deshabilita un trigger")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/description", UpdateDescriptionAsync)
            .WithName("UpdateTriggerDescription")
            .WithSummary("Actualiza la descripción de un trigger")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/configurations", AddConfigurationAsync)
            .WithName("AddConfiguration")
            .WithSummary("Agrega una configuración de notificación a un trigger")
            .Produces<CreatedConfigurationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/configurations/{configId:guid}", DisableConfigurationAsync)
            .WithName("DisableConfiguration")
            .WithSummary("Deshabilita una configuración de notificación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/configurations/{configId:guid}/enable", EnableConfigurationAsync)
            .WithName("EnableConfiguration")
            .WithSummary("Habilita una configuración de notificación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/configurations/{configId:guid}/template", ChangeConfigurationTemplateAsync)
            .WithName("ChangeConfigurationTemplate")
            .WithSummary("Cambia la plantilla de una configuración de notificación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateTriggerRequest body,
        CreateTriggerHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(
            new CreateTriggerCommand(body.EventKey, body.Description), ct);

        return Results.CreatedAtRoute(
            "GetTriggerById",
            new { id },
            new CreatedTriggerResponse(id));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetTriggerByIdHandler handler,
        CancellationToken ct)
    {
        var trigger = await handler.HandleAsync(new GetTriggerByIdQuery(id), ct);
        return Results.Ok(trigger);
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListTriggersQuery query,
        ListTriggersHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> SetStatusAsync(
        Guid id,
        SetTriggerStatusRequest body,
        SetTriggerStatusHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SetTriggerStatusCommand(id, body.IsEnabled), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateDescriptionAsync(
        Guid id,
        UpdateTriggerDescriptionRequest body,
        UpdateTriggerDescriptionHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new UpdateTriggerDescriptionCommand(id, body.Description), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddConfigurationAsync(
        Guid id,
        AddConfigurationRequest body,
        AddConfigurationHandler handler,
        CancellationToken ct)
    {
        var configuration = await handler.HandleAsync(
            new AddConfigurationCommand(id, body.TemplateId, body.Channel), ct);

        return Results.Created(
            $"/api/triggers/{id}/configurations/{configuration.Id}",
            new CreatedConfigurationResponse(configuration.Id));
    }

    private static async Task<IResult> DisableConfigurationAsync(
        Guid id,
        Guid configId,
        DisableConfigurationHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new DisableConfigurationCommand(id, configId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> EnableConfigurationAsync(
        Guid id,
        Guid configId,
        EnableConfigurationHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new EnableConfigurationCommand(id, configId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangeConfigurationTemplateAsync(
        Guid id,
        Guid configId,
        ChangeConfigurationTemplateRequest body,
        ChangeConfigurationTemplateHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(
            new ChangeConfigurationTemplateCommand(id, configId, body.TemplateId), ct);
        return Results.NoContent();
    }
}
