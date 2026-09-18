using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Api.Data;
using NotificationLog.IdentityService.Api.Messaging;
using NotificationLog.IdentityService.Api.Seeding;
using NotificationLog.IdentityService.Api.Users;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAccounts(builder.Configuration);
builder.Services.AddConnect(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);

builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection(IdentitySeedOptions.SectionName));
builder.Services.AddHostedService<IdentitySeeder>();

builder.Services.AddRazorPages();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapDefaultEndpoints();

app.MapConnect();
app.MapUsers();
app.MapRazorPages().WithStaticAssets();

app.Run();
