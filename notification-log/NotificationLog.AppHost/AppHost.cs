using NotificationLog.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var databases = AddGroup("databases").WithIconName("Database");
var messaging = AddGroup("messaging").WithIconName("ChatMultiple");
var apis = AddGroup("apis").WithIconName("PlugConnected");
var ui = AddGroup("ui").WithIconName("Desktop");

var cache = builder.AddRedis("cache")
    .WithParentRelationship(databases);

// SQL Server: base de Notification.
var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithParentRelationship(databases);

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

// PostgreSQL: una base por servicio (Identity y Rentals).
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithParentRelationship(databases);

var identityDb = postgres.AddDatabase("identity");
var rentalsDb = postgres.AddDatabase("rentals");

// DbGate: cliente SQL en el navegador para inspeccionar las bases sin instalar nada — una conexión
// por servidor, con el usuario/password que ya genera Aspire para cada uno. Sin paquete de hosting
// de Aspire dedicado, se agrega como contenedor genérico.
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
    .WithParentRelationship(databases);

// Management plugin: UI en el puerto expuesto por Aspire (link visible en el dashboard) para
// publicar mensajes de prueba a mano mientras se prueba el ejemplo de Recipients sync.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithParentRelationship(messaging);

// Buggregator: un único sink de dev para todo lo que sale del servicio — SMTP (IEmailNotificationSender)
// y el SMS Gateway multi-proveedor, ambos por el mismo puerto 8000 (ISmsNotificationSender).
// CLIENT_SUPPORTED_EVENTS limita los módulos activos a
// smtp+sms — el resto (Sentry, Ray, VarDumper, Monolog, Inspector, XHProf, HTTP dumps) no aplica
// acá, y un módulo deshabilitado ni siquiera abre su puerto TCP
// (https://docs.buggregator.dev/config/server.html), por eso no se declaran endpoints para
// var-dump ni monolog. Sin paquete de hosting de Aspire dedicado, se agrega como contenedor
// genérico con los puertos default de la imagen (https://github.com/buggregator/server).
var buggregator = builder.AddContainer("buggregator", "ghcr.io/buggregator/server")
    .WithEnvironment("CLIENT_SUPPORTED_EVENTS", "smtp,sms")
    .WithHttpEndpoint(port: 8000, targetPort: 8000)
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp", scheme: "tcp")
    .WithParentRelationship(messaging);

var apiService = builder.AddProject<Projects.NotificationLog_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    // ContainerResource no implementa IResourceWithServiceDiscovery, así que WithReference(buggregator)
    // a secas no compila — hay que referenciar el endpoint puntual. Esto inyecta
    // "services__buggregator__http__0" con la dirección real (localhost:<puerto que Docker le
    // asignó esta corrida>) — no un nombre de dominio, apiservice sigue siendo un proceso nativo en
    // esta misma máquina, no otro contenedor. HttpSmsNotificationSender lee esa variable directo
    // (ver NotificationSendingExtensions) en vez de tener el puerto fijo a mano, que es lo que se
    // desincronizaba cada vez que Aspire reasignaba el puerto publicado del contenedor.
    .WaitFor(buggregator)
    .WithParentRelationship(apis);

var identityApi = builder.AddProject<Projects.NotificationLog_IdentityService_Api>("identityservice")
    .WithHttpHealthCheck("/health")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WaitFor(buggregator)
    .WithParentRelationship(apis);

var rentalsApi = builder.AddProject<Projects.NotificationLog_RentalService_Api>("rentalservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rentalsDb)
    .WaitFor(rentalsDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithParentRelationship(apis);

var webClientSecret = builder.AddParameter(
    "web-client-secret",
    new GenerateParameterDefault { MinLength = 32, Special = false },
    secret: true,
    persist: true);

var web = builder.AddProject<Projects.NotificationLog_Web>("webfrontend", launchProfileName: "https")
    .WithEndpoint("https", endpoint => endpoint.IsProxied = false)
    .WithEndpoint("http", endpoint => endpoint.IsProxied = false)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WithReference(rentalsApi)
    .WithReference(identityApi)
    .WaitFor(identityApi)
    .WithEnvironment("Identity__Authority", identityApi.GetEndpoint("https"))
    .WithEnvironment("Identity__ClientId", "web")
    .WithEnvironment("Identity__ClientSecret", webClientSecret)
    .WithParentRelationship(ui);

var webHttps = web.GetEndpoint("https");

identityApi
    .WithEnvironment("Seed__Clients__0__ClientId", "web")
    .WithEnvironment("Seed__Clients__0__ClientSecret", webClientSecret)
    .WithEnvironment("Seed__Clients__0__DisplayName", "NotificationLog Web")
    .WithEnvironment("Seed__Clients__0__RedirectUris__0", ReferenceExpression.Create($"{webHttps}/signin-oidc"))
    .WithEnvironment("Seed__Clients__0__PostLogoutRedirectUris__0", ReferenceExpression.Create($"{webHttps}/signout-callback-oidc"))
    .WithEnvironment("Seed__Clients__0__Scopes__0", "identity");

builder.Build().Run();

IResourceBuilder<ResourceGroup> AddGroup(string name) =>
    builder.AddResource(new ResourceGroup(name))
        .WithInitialState(new CustomResourceSnapshot
        {
            ResourceType = "Group",
            State = KnownResourceStates.Running,
            Properties = []
        })
        .ExcludeFromManifest();