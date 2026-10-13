using Microsoft.Extensions.Configuration;

namespace NotificationLog.AppHost;

public static class LoadTestResources
{
    public const string EnabledKey = "LoadTests:Enabled";

    private const string K6Image = "grafana/k6";
    private const string K6Tag = "1.3.0";
    private const int DashboardPort = 5665;

    private static readonly (string Name, string Profile)[] LocalRuns =
    [
        ("loadtest-smoke", "smoke"),
        ("loadtest-100", "carga-100"),
        ("loadtest-1000", "carga-1000")
    ];

    private static readonly (string Suffix, string Profile)[] EnvironmentRuns =
    [
        ("100", "carga-100"),
        ("1000", "carga-1000")
    ];

    public static bool LoadTestsEnabled(this IDistributedApplicationBuilder builder) =>
        builder.Configuration.GetValue(EnabledKey, false);

    public static void AddLoadTests(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ResourceGroup> group,
        IResourceBuilder<ProjectResource> identity,
        IResourceBuilder<ProjectResource> rental,
        IResourceBuilder<ProjectResource> notification)
    {
        foreach (var (name, profile) in LocalRuns)
        {
            builder.AddK6(name, profile, "local", group)
                .WithEnvironment("IDENTITY_URL", identity.GetEndpoint("https"))
                .WithEnvironment("RENTAL_URL", rental.GetEndpoint("https"))
                .WithEnvironment("NOTIFICATION_URL", notification.GetEndpoint("https"))
                .WithEnvironment("INSECURE_TLS", "true")
                .WaitFor(identity)
                .WaitFor(rental)
                .WaitFor(notification);
        }

        foreach (var environment in builder.Configuration.GetSection("LoadTests:Environments").GetChildren())
        {
            foreach (var (suffix, profile) in EnvironmentRuns)
            {
                builder.AddK6($"loadtest-{environment.Key}-{suffix}", profile, environment.Key, group)
                    .WithEnvironment("IDENTITY_URL", environment["IdentityUrl"])
                    .WithEnvironment("RENTAL_URL", environment["RentalUrl"])
                    .WithEnvironment("NOTIFICATION_URL", environment["NotificationUrl"])
                    .WithEnvironment("INSECURE_TLS", environment["InsecureTls"] ?? "false")
                    .WithEnvironment("LOADTEST_PASSWORD", environment["Password"] ?? "LoadTest12345");
            }
        }
    }

    private static IResourceBuilder<ContainerResource> AddK6(
        this IDistributedApplicationBuilder builder,
        string name,
        string profile,
        string target,
        IResourceBuilder<ResourceGroup> group)
    {
        var report = $"/results/{target}-{profile}";

        return builder.AddContainer(name, K6Image, K6Tag)
            .WithBindMount("../LoadTests/k6", "/scripts", isReadOnly: true)
            .WithBindMount("../LoadTests/results", "/results")
            .WithArgs("run", "/scripts/main.js")
            .WithEnvironment("PROFILE", profile)
            .WithEnvironment("TARGET", target)
            .WithEnvironment("SUMMARY_PATH", $"{report}-summary.json")
            .WithEnvironment("K6_WEB_DASHBOARD", "true")
            .WithEnvironment("K6_WEB_DASHBOARD_HOST", "0.0.0.0")
            .WithEnvironment("K6_WEB_DASHBOARD_EXPORT", $"{report}-report.html")
            .WithHttpEndpoint(targetPort: DashboardPort, name: "dashboard")
            .WithExplicitStart()
            .ExcludeFromManifest()
            .WithParentRelationship(group);
    }
}
