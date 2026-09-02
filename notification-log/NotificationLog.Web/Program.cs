using NotificationLog.Web.Api.Notifications;
using NotificationLog.Web.Api.Recipients;
using NotificationLog.Web.Api.Templates;
using NotificationLog.Web.Api.Triggers;
using NotificationLog.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Clientes tipados hacia NotificationLog.ApiService. La URL usa "https+http://" para preferir
// HTTPS cuando esté disponible; la resuelve el descubrimiento de servicios de Aspire.
builder.Services.AddHttpClient<TriggersApiClient>(client => client.BaseAddress = new("https+http://apiservice"));
builder.Services.AddHttpClient<TemplatesApiClient>(client => client.BaseAddress = new("https+http://apiservice"));
builder.Services.AddHttpClient<RecipientsApiClient>(client => client.BaseAddress = new("https+http://apiservice"));
builder.Services.AddHttpClient<NotificationsApiClient>(client => client.BaseAddress = new("https+http://apiservice"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
