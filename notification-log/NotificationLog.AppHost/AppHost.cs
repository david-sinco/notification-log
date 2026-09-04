var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

// Management plugin: UI en el puerto expuesto por Aspire (link visible en el dashboard) para
// publicar mensajes de prueba a mano mientras se prueba el ejemplo de Recipients sync.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Mailpit: atrapa el SMTP saliente y lo muestra en su propia UI — para cuando IEmailNotificationSender
// tenga implementación real en Infrastructure, en vez de mandar correos de verdad en dev.
var mailpit = builder.AddMailPit("mailpit");

// SMSPit: el mismo rol que Mailpit pero para SMS — simula localmente las APIs HTTP de proveedores
// reales (Twilio, Vonage, etc.) para ISmsNotificationSender. No tiene paquete de hosting de Aspire
// dedicado (a diferencia de Mailpit), así que se agrega como contenedor genérico.
var smspit = builder.AddContainer("smspit", "ntechservices/smspitt")
    .WithHttpEndpoint(port: 2875, targetPort: 2875, name: "ui")
    .WithHttpEndpoint(port: 2876, targetPort: 2876, name: "provider-api")
    .WithHttpEndpoint(port: 2877, targetPort: 2877, name: "test-api");

var apiService = builder.AddProject<Projects.NotificationLog_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(notificationDb)
    .WaitFor(notificationDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(mailpit)
    .WaitFor(mailpit)
    .WaitFor(smspit);

builder.AddProject<Projects.NotificationLog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService);

builder.Build().Run();