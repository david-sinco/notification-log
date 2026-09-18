using NotificationLog.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var databases = AddGroup("databases").WithIconName("Database");
var messaging = AddGroup("messaging").WithIconName("ChatMultiple");
var apis = AddGroup("apis").WithIconName("PlugConnected");
var ui = AddGroup("ui").WithIconName("Desktop");

var cache = builder.AddRedis("cache")
    .WithParentRelationship(databases);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithParentRelationship(databases);

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithParentRelationship(databases);

var identityDb = postgres.AddDatabase("identity-db", "identity");
var rentalsDb = postgres.AddDatabase("rentals");

AddSqlClient();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithParentRelationship(messaging);

var buggregator = builder.AddContainer("buggregator", "ghcr.io/buggregator/server")
    .WithEnvironment("CLIENT_SUPPORTED_EVENTS", "smtp,sms")
    .WithHttpEndpoint(port: 8000, targetPort: 8000)
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp", scheme: "tcp")
    .WithParentRelationship(messaging);

var webClientSecret = builder.AddParameter(
    "web-client-secret",
    new GenerateParameterDefault { MinLength = 32, Special = false },
    secret: true,
    persist: true);

var oidcConfig = builder.Configuration.GetSection("Oidc");
var webClientConfig = builder.Configuration.GetSection("WebClient");

var notification = AddNotification();
var identity = AddIdentity();
var rental = AddRental();
AddWeb();
AddDocs();

builder.Build().Run();

void AddSqlClient() =>
    builder.AddContainer("sql-client", "dbgate/dbgate")
        .WithHttpEndpoint(port: 3000, targetPort: 3000)
        .WithEnvironment("CONNECTIONS", "sql,postgres")
        .WithEnvironment("LABEL_sql", "SQL Server (Notification)")
        .WithEnvironment("SERVER_sql", sql.Resource.Name)
        .WithEnvironment("PORT_sql", "1433")
        .WithEnvironment("USER_sql", "sa")
        .WithEnvironment("PASSWORD_sql", sql.Resource.PasswordParameter)
        .WithEnvironment("ENGINE_sql", "mssql@dbgate-plugin-mssql")
        .WithEnvironment("LABEL_postgres", "PostgreSQL (Identity, Rentals)")
        .WithEnvironment("SERVER_postgres", postgres.Resource.Name)
        .WithEnvironment("PORT_postgres", "5432")
        .WithEnvironment("USER_postgres", "postgres")
        .WithEnvironment("PASSWORD_postgres", postgres.Resource.PasswordParameter)
        .WithEnvironment("ENGINE_postgres", "postgres@dbgate-plugin-postgres")
        .WaitFor(sql)
        .WaitFor(postgres)
        .WithDevUrls()
        .WithParentRelationship(databases);

IResourceBuilder<ProjectResource> AddNotification() =>
    builder.AddProject<Projects.NotificationLog_ApiService>("notification")
        .WithHttpHealthCheck("/health")
        .WithReference(notificationDb)
        .WaitFor(notificationDb)
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq)
        .WaitFor(buggregator)
        .WithDevUrls()
        .WithParentRelationship(apis);

IResourceBuilder<ProjectResource> AddIdentity() =>
    builder.AddProject<Projects.NotificationLog_IdentityService_Api>("identity")
        .WithHttpHealthCheck("/health")
        .WithReference(identityDb)
        .WaitFor(identityDb)
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq)
        .WithEnvironment("Oidc__Issuer", oidcConfig["Issuer"])
        .WithEnvironment("Oidc__Audiences__identity", oidcConfig["Audiences:Identity"])
        .WithEnvironment("Oidc__Audiences__rentals", oidcConfig["Audiences:Rentals"])
        .WithEnvironment("Oidc__Audiences__notifications", oidcConfig["Audiences:Notifications"])
        .WithEnvironment("Seed__Clients__0__ClientId", webClientConfig["ClientId"])
        .WithEnvironment("Seed__Clients__0__ClientSecret", webClientSecret)
        .WithEnvironment("Seed__Clients__0__Scopes__0", "identity")
        .WithEnvironment("Seed__Clients__0__RedirectUris__0", webClientConfig["RedirectUris:0"])
        .WithEnvironment("Seed__Clients__0__RedirectUris__1", webClientConfig["RedirectUris:1"])
        .WithEnvironment("Seed__Clients__0__PostLogoutRedirectUris__0", webClientConfig["PostLogoutRedirectUris:0"])
        .WithEnvironment("Seed__Clients__0__PostLogoutRedirectUris__1", webClientConfig["PostLogoutRedirectUris:1"])
        .WithDevUrls()
        .WithParentRelationship(apis);

IResourceBuilder<ProjectResource> AddRental() =>
    builder.AddProject<Projects.NotificationLog_RentalService_Api>("rental")
        .WithHttpHealthCheck("/health")
        .WithReference(rentalsDb)
        .WaitFor(rentalsDb)
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq)
        .WithDevUrls()
        .WithParentRelationship(apis);

void AddWeb() =>
    builder.AddProject<Projects.NotificationLog_Web>("web", launchProfileName: "https")
        .WithExternalHttpEndpoints()
        .WithHttpHealthCheck("/health")
        .WithReference(cache)
        .WaitFor(cache)
        .WithReference(notification)
        .WithReference(rental)
        .WithReference(identity)
        .WaitFor(identity)
        .WithEnvironment("Identity__Authority", oidcConfig["Issuer"])
        .WithEnvironment("Identity__ClientId", webClientConfig["ClientId"])
        .WithEnvironment("Identity__ClientSecret", webClientSecret)
        .WithDevUrls()
        .WithParentRelationship(ui);

void AddDocs() =>
    builder.AddViteApp("docs", "../UI/docs")
        .WithNpm()
        .WithExternalHttpEndpoints()
        .WithDevUrls()
        .WithParentRelationship(ui);

IResourceBuilder<ResourceGroup> AddGroup(string name) =>
    builder.AddResource(new ResourceGroup(name))
        .WithInitialState(new CustomResourceSnapshot
        {
            ResourceType = "Group",
            State = KnownResourceStates.Running,
            Properties = []
        })
        .ExcludeFromManifest();
