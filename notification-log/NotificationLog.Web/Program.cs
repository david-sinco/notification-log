using NotificationLog.Web.Api.Notifications;
using NotificationLog.Web.Api.Recipients;
using NotificationLog.Web.Api.Templates;
using NotificationLog.Web.Api.Triggers;
using NotificationLog.Web.Api.Rentals;
using NotificationLog.Web.Api.Rentals.Identity;
using NotificationLog.Web.Api.Rentals.Inquiries;
using NotificationLog.Web.Api.Rentals.Listings;
using NotificationLog.Web.Api.Rentals.Offers;
using NotificationLog.Web.Api.Rentals.Users;
using NotificationLog.Web.Api.Rentals.Visits;
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

// Clientes tipados hacia NotificationLog.ApiService. La URL usa "https+http://" para preferir
// HTTPS cuando esté disponible; la resuelve el descubrimiento de servicios de Aspire.
builder.Services.AddHttpClient<TriggersApiClient>(client => client.BaseAddress = new("https+http://notification"));
builder.Services.AddHttpClient<TemplatesApiClient>(client => client.BaseAddress = new("https+http://notification"));
builder.Services.AddHttpClient<RecipientsApiClient>(client => client.BaseAddress = new("https+http://notification"));
builder.Services.AddHttpClient<NotificationsApiClient>(client => client.BaseAddress = new("https+http://notification"));

builder.Services.AddHttpClient<ListingsApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<ModerationApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<OffersApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<VisitsApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<InquiriesApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<UserCollectionsApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
builder.Services.AddHttpClient<DevIdentityApiClient>(client => client.BaseAddress = new(RentalsApi.BaseAddress));
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
