using Web.Components;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// "http://users-api" / "http://notifications-api" are logical service names resolved by Aspire
// service discovery (wired up in ServiceDefaults) from the WithReference calls in AppHost —
// not literal hostnames. Same resource names as event-sourcing/AppHost, so this console needs no
// per-solution configuration beyond which AppHost started it.
builder.Services.AddHttpClient<UsersApiClient>(client => client.BaseAddress = new Uri("http://users-api"));
builder.Services.AddHttpClient<NotificationsApiClient>(client => client.BaseAddress = new Uri("http://notifications-api"));

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
