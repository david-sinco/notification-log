using NotificationLog.Web.Api.Identity;
using NotificationLog.Web.Api.Notifications;
using NotificationLog.Web.Api.Rentals;
using NotificationLog.Web.Authentication;
using NotificationLog.Web.Components;
using NotificationLog.Web.Components.Rentals;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddIdentityAuthentication(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddNotificationApi();
builder.Services.AddIdentityApi();
builder.Services.AddRentalsApi();

builder.Services.AddScoped<RentalsActor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapAuthentication();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
