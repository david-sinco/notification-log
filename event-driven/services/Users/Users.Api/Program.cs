using System.Text.Json.Serialization;
using Users.Api.HealthChecks;
using Users.Api.Middleware;
using Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddUsersInfrastructure(builder.Configuration);
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
