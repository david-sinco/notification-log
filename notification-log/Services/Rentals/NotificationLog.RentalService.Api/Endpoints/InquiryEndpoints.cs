using NotificationLog.RentalService.Api.Contracts.Inquiries;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Inquiries.Dtos;
using NotificationLog.RentalService.Application.Inquiries.Queries.GetInquiryById;
using NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;
using NotificationLog.RentalService.Application.Inquiries.Commands.ReplyToInquiry;
using NotificationLog.RentalService.Application.Inquiries.Commands.SendInquiryMessage;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class InquiryEndpoints
{
    public static IEndpointRouteBuilder MapInquiries(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inquiries").WithTags("Inquiries");

        group.MapPost("/", SendMessageAsync)
            .WithName("SendInquiryMessage")
            .WithSummary("Envía un mensaje al publicador; abre la consulta si todavía no existe")
            .Produces<SentInquiryMessageResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .WithName("ListInquiries")
            .WithSummary("Lista consultas con filtros y paginación")
            .Produces<PagedResult<InquirySummaryDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetInquiryById")
            .WithSummary("Obtiene una consulta con sus mensajes")
            .Produces<InquiryDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/replies", ReplyAsync)
            .WithName("ReplyToInquiry")
            .WithSummary("Responde la consulta como publicador o asesor")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> SendMessageAsync(
        SendInquiryMessageRequest body,
        SendInquiryMessageHandler handler,
        CancellationToken ct)
    {
        var inquiryId = await handler.HandleAsync(
            new SendInquiryMessageCommand(body.SeekerId, body.ListingId, body.Message), ct);

        return Results.Ok(new SentInquiryMessageResponse(inquiryId));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListInquiriesQuery query,
        ListInquiriesHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetInquiryByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetInquiryByIdQuery(id), ct));

    private static async Task<IResult> ReplyAsync(
        Guid id,
        ReplyToInquiryRequest body,
        ReplyToInquiryHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReplyToInquiryCommand(body.ActorId, id, body.Message), ct);
        return Results.NoContent();
    }
}
