using API.Shared.Exceptions;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Api.Authorization;
using NotificationLog.RentalService.Api.Endpoints;
using NotificationLog.RentalService.Api.OpenApi;
using NotificationLog.RentalService.Application;
using NotificationLog.RentalService.Infrastructure;
using Scalar.AspNetCore;
using API.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenIdDictAuthorization(builder.Configuration, [OidcScope.Rentals] );
//builder.Services.AddRentalsAuthorization(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>());

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference((options, context) => options
        .AddPreferredSecuritySchemes(OAuthSecuritySchemeTransformer.SchemeName)
        .AddAuthorizationCodeFlow(OAuthSecuritySchemeTransformer.SchemeName, flow => flow
            .WithClientId(builder.Configuration["Scalar:ClientId"])
            .WithPkce(Pkce.Sha256)
            .WithRedirectUri($"{context.Request.Scheme}://{context.Request.Host}/scalar/")
            .WithSelectedScopes(["openid", "roles", OidcScope.Rentals.ToScopeName()])));
    app.MapDevIdentity();
}

app.MapDefaultEndpoints();

app.MapListings();
app.MapModeration();
app.MapVisits();
app.MapOwners();

app.Run();
