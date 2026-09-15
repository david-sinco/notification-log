var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

// DbGate: cliente SQL en el navegador para inspeccionar la base sin instalar nada — apunta al
// mismo contenedor de SQL Server, con el usuario/password que ya genera Aspire para "sql". Sin
// paquete de hosting de Aspire dedicado, se agrega como contenedor genérico.
builder.AddContainer("sql-client", "dbgate/dbgate")
    .WithHttpEndpoint(port: 3000, targetPort: 3000)
    .WithEnvironment("CONNECTIONS", "sql")
    .WithEnvironment("LABEL_sql", "NotificationLog SQL Server")
    .WithEnvironment("SERVER_sql", sql.Resource.Name)
    .WithEnvironment("PORT_sql", "1433")
    .WithEnvironment("USER_sql", "sa")
    .WithEnvironment("PASSWORD_sql", sql.Resource.PasswordParameter)
    .WithEnvironment("ENGINE_sql", "mssql@dbgate-plugin-mssql")
    .WaitFor(sql);

// Management plugin: UI en el puerto expuesto por Aspire (link visible en el dashboard) para
// publicar mensajes de prueba a mano mientras se prueba el ejemplo de Recipients sync.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

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
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp", scheme: "tcp");

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
    .WaitFor(buggregator);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var rentalsDb = postgres.AddDatabase("rentals");

var rentalsApi = builder.AddProject<Projects.NotificationLog_RentalService_Api>("notificationlog-rentalservice-api")
    .WithHttpHealthCheck("/health")
    .WithReference(rentalsDb)
    .WaitFor(rentalsDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.NotificationLog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WithReference(rentalsApi);

builder.Build().Run();