using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Application.Common.Storage;
using Wolverine;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class RentalsApi(string connectionString) : WebApplicationFactory<Program>
{
    public TestClock Clock { get; } = new();
    public RecordingNotificationProducer Notifications { get; } = new();
    public InMemoryPhotoStorage Photos { get; } = new();

    public async Task ResetAsync()
    {
        await Services.GetRequiredService<IDocumentStore>().Advanced.ResetAllData();

        Clock.Reset();
        Notifications.Clear();
        Photos.Clear();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:rentals", connectionString);
        builder.UseSetting("ConnectionStrings:rabbitmq", "amqp://guest:guest@localhost:5672");
        builder.UseSetting("ConnectionStrings:photos", "UseDevelopmentStorage=true;ContainerName=photos");
        builder.UseSetting("Oidc:Issuer", "https://identity.test/");
        builder.UseSetting("Oidc:Audience", "rentals");
        builder.UseSetting("Rentals:RebuildViews", "false");

        builder.ConfigureTestServices(services =>
        {
            services.DisableAllExternalWolverineTransports();

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);

            services.RemoveAll<INotificationProducer>();
            services.AddSingleton<INotificationProducer>(Notifications);

            services.RemoveAll<IPhotoStorage>();
            services.AddSingleton<IPhotoStorage>(Photos);

            services.RemoveAll<IPhotoUrlProvider>();
            services.AddSingleton<IPhotoUrlProvider>(Photos);

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });
        });
    }
}
