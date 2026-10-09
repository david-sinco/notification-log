using API.Shared.Exceptions;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Api.Endpoints;
using NotificationLog.RentalService.Application;
using NotificationLog.RentalService.Infrastructure;
using API.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddAzureBlobContainerClient("photos");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenIdDictAuthorization(builder.Configuration, [OidcScope.Rentals] );

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOAuthOpenApi([OidcScope.Rentals]);

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapOAuthScalar();
app.MapRootToScalar();

app.MapDefaultEndpoints();

app.MapCatalog();
app.MapDashboard();
app.MapListings();
app.MapModeration();
app.MapOwners();
app.MapVisitors();
app.MapVisits();

app.Run();
