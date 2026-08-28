using Aspire.Hosting.ApplicationModel;

// No transport switch here, unlike event-sourcing/AppHost: this solution only ever uses Kafka,
// because the whole point is a log that supports full replay-from-offset — the one property
// classic RabbitMQ structurally lacks and the property this entire architecture depends on.
// No Postgres, either: nothing in this solution has a private database (event-driven/README.md).

var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI()
    .WithDataVolume();

// Catches real SMTP traffic for the Email channel so a "sent" notification can actually be
// opened and read, not just trusted on the fake sender's word — same as event-sourcing/AppHost.
var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(targetPort: 8025, port: 8026, name: "http") // :8026, not event-sourcing/'s :8025 — both solutions can run at once
    .WithEndpoint(targetPort: 1025, scheme: "smtp", name: "smtp");

var usersApi = builder.AddProject<Projects.Users_Api>("users-api")
    .WithEnvironment("Messaging__BootstrapServers", kafka)
    .WaitFor(kafka)
    .WithHttpHealthCheck("/health");

var notificationsApi = builder.AddProject<Projects.Notifications_Api>("notifications-api")
    .WithEnvironment("Messaging__BootstrapServers", kafka)
    .WaitFor(kafka)
    .WithEnvironment("Smtp__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
    .WithEnvironment("Smtp__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port))
    .WaitFor(mailpit)
    .WithHttpHealthCheck("/health");

// A hand-testing console, not part of the measured system — see Web/Web.csproj. WithReference
// wires Aspire service discovery so it can resolve "http://users-api"/"http://notifications-api"
// without hard-coded ports, same as event-sourcing/AppHost's Web.
builder.AddProject<Projects.Web>("web")
    .WithReference(usersApi)
    .WithReference(notificationsApi)
    .WaitFor(usersApi)
    .WaitFor(notificationsApi);

builder.Build().Run();
