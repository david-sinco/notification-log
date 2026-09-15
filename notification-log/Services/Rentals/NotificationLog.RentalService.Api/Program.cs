using NotificationLog.RentalService.Api.Endpoints;
using NotificationLog.RentalService.Api.Exceptions;
using NotificationLog.RentalService.Application;
using NotificationLog.RentalService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapDevIdentity();
}

app.MapDefaultEndpoints();

app.MapListings();
app.MapModeration();
app.MapOffers();
app.MapVisits();
app.MapInquiries();
app.MapFavorites();
app.MapSavedSearches();

app.Run();
