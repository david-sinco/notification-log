using System.Text.Json.Serialization;
using API.Shared.Extensions;
using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Api.Endpoints;
using API.Shared.Exceptions;
using NotificationLog.IdentityService.Application;
using NotificationLog.IdentityService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddConnect(builder.Configuration);
builder.Services.AddScoped<RegistrationRoleResolver>();
builder.Services.AddScopePolicies([OidcScope.Identity]);

builder.Services.AddRazorPages();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOAuthOpenApi([OidcScope.Identity]);

var app = builder.Build();

await app.Services.MigrateIdentityDatabaseAsync();

app.UseExceptionHandler();

app.MapOAuthScalar();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapDefaultEndpoints();

app.MapConnect();
app.MapUsers();
app.MapRazorPages().WithStaticAssets();

app.Run();
