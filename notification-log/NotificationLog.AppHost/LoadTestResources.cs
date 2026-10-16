namespace NotificationLog.AppHost;

public static class LoadTestResources
{
    private const string K6Image = "grafana/k6";
    private const string K6Tag = "1.3.0";
    private const int DashboardPort = 5665;

    private static readonly (string Name, string Profile)[] Runs =
    [
        ("loadtest-smoke", "smoke"),
        ("loadtest-100", "carga-100"),
        ("loadtest-1000", "carga-1000")
    ];

    public static void AddLoadTests(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ResourceGroup> group,
        IResourceBuilder<ProjectResource> identity,
        IResourceBuilder<ProjectResource> rental,
        IResourceBuilder<ProjectResource> notification,
        IResourceBuilder<ContainerResource> mailbox)
    {
        foreach (var (name, profile) in Runs)
        {
            var report = $"/results/{profile}";

            builder.AddContainer(name, K6Image, K6Tag)
                .WithBindMount("../LoadTests/k6", "/scripts", isReadOnly: true)
                .WithBindMount("../LoadTests/results", "/results")
                .WithArgs("run", "/scripts/main.js")
                .WithEnvironment("PROFILE", profile)
                .WithEnvironment("SUMMARY_PATH", $"{report}-summary.json")
                .WithEnvironment("K6_WEB_DASHBOARD", "true")
                .WithEnvironment("K6_WEB_DASHBOARD_HOST", "0.0.0.0")
                .WithEnvironment("K6_WEB_DASHBOARD_EXPORT", $"{report}-report.html")
                .WithEnvironment("IDENTITY_URL", identity.GetEndpoint("https"))
                .WithEnvironment("RENTAL_URL", rental.GetEndpoint("https"))
                .WithEnvironment("NOTIFICATION_URL", notification.GetEndpoint("https"))
                .WithEnvironment("MAILBOX_URL", mailbox.GetEndpoint("http"))
                .WithEnvironment("INSECURE_TLS", "true")
                .WithHttpEndpoint(targetPort: DashboardPort, name: "dashboard")
                .WaitFor(identity)
                .WaitFor(rental)
                .WaitFor(notification)
                .WaitFor(mailbox)
                .WithExplicitStart()
                .ExcludeFromManifest()
                .WithParentRelationship(group);
        }
    }
}
