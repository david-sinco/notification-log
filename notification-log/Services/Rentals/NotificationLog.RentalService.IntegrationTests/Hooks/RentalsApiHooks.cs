using Reqnroll.BoDi;
using Testcontainers.PostgreSql;

namespace NotificationLog.RentalService.IntegrationTests.Hooks;

[Binding]
public sealed class RentalsApiHooks
{
    private static PostgreSqlContainer _postgres = null!;
    private static RentalsApi _api = null!;

    [BeforeTestRun]
    public static async Task StartApiAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await _postgres.StartAsync();

        _api = new RentalsApi(_postgres.GetConnectionString());
        _api.CreateClient().Dispose();
    }

    [AfterTestRun]
    public static async Task StopApiAsync()
    {
        await _api.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [BeforeScenario(Order = 0)]
    public static async Task ResetAsync(IObjectContainer container)
    {
        await _api.ResetAsync();

        container.RegisterInstanceAs(_api);
    }
}
