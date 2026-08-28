var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume();

var notificationDb = sql.AddDatabase("Default", "NotificationLog");

var apiService = builder.AddProject<Projects.NotificationLog_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(notificationDb)
    .WaitFor(notificationDb);

builder.AddProject<Projects.NotificationLog_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();