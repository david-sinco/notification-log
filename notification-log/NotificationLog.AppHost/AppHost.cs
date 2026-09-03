var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

// Management plugin: UI en el puerto expuesto por Aspire (link visible en el dashboard) para
// publicar mensajes de prueba a mano mientras se prueba el ejemplo de Recipients sync.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var apiService = builder.AddProject<Projects.NotificationLog_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.NotificationLog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();