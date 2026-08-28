using System.Text.Json.Serialization;
using Notifications.Api.HealthChecks;
using Notifications.Api.Middleware;
using Notifications.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddNotificationsInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<MaterializerReadinessHealthCheck>("materializer-readiness", tags: ["ready"]);
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<DomainExceptionMiddleware>();

app.MapControllers();

app.Run();
