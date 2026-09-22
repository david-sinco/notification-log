using API.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using NotificationLog.ApiService.Endpoints;
using NotificationLog.NotificationService.Application;
using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Infrastructure;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapPost("/api/dev/recipients", async (
        CreateRecipientCommand cmd,
        CreateRecipientHandler handler,
        CancellationToken ct) =>
    {
        await handler.HandleAsync(cmd, ct);
        return Results.Created($"/api/recipients/{cmd.RecipientId}", new { id = cmd.RecipientId });
    })
    .WithTags("Dev")
    .WithSummary("Solo desarrollo: crea un destinatario manualmente");
}

app.MapDefaultEndpoints();

// Endpoints
app.MapTriggers();
app.MapTemplates();
app.MapRecipients();
app.MapNotifications();

app.Run();