using Aspire.Hosting.ApplicationModel;

// The single switch point for the whole lab (SPEC.md §9): flip this and re-run, nothing else
// changes. All transports start regardless of which one is selected, so switching never requires
// an infrastructure change — only this constant.
//   "RabbitMq"       — classic queues: ack deletes the message, no replay, no consumer groups.
//   "RabbitMqStream" — RabbitMQ Streams: an actual append-only log on the same broker, over a
//                      different protocol/port. Replay-from-offset and durable per-reference
//                      checkpoints, same broker as above, structurally different guarantees.
//   "Kafka"          — partitioned log, compacted by physical offset per key, manual commit.
const string transportKind = "RabbitMq"; // "RabbitMq" | "RabbitMqStream" | "Kafka"

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var usersDb = postgres.AddDatabase("usersdb");
var notificationsDb = postgres.AddDatabase("notificationsdb");

// The Streams plugin ships in the image but is off by default — enabled by replacing
// /etc/rabbitmq/enabled_plugins wholesale, so rabbitmq_management has to be listed here too or
// WithManagementPlugin's image would lose it. Port 5552 is the Streams protocol's fixed default;
// it's a separate listener on this same container, not an alternative to 5672.
var rabbitEnabledPlugins = Path.Combine(builder.AppHostDirectory, "rabbitmq", "enabled_plugins");

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume()
    .WithBindMount(rabbitEnabledPlugins, "/etc/rabbitmq/enabled_plugins", isReadOnly: true)
    .WithEndpoint(targetPort: 5552, name: "stream");

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI()
    .WithDataVolume();

// Catches real SMTP traffic for the Email channel so a "sent" notification can actually be
// opened and read, not just trusted on the fake sender's word (SPEC.md §8's fake sender still
// decides pass/fail/retry — this only proves delivery for the sends it lets through).
var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(targetPort: 8025, port: 8025, name: "http") // fixed so the web UI is always at :8025
    .WithEndpoint(targetPort: 1025, scheme: "smtp", name: "smtp");

// Passed as the resource builder itself, not a hand-built "{name.connectionString}" string — the
// latter is never resolved by Aspire and the service would start with a literal placeholder
// (SPEC.md §9). RabbitMqStream is still the "rabbitmq" resource — same broker, same AMQP
// connection string for host/credentials, just read over the separate stream endpoint below.
IResourceBuilder<IResourceWithConnectionString> messaging = transportKind == "Kafka" ? kafka : rabbit;
var streamPort = rabbit.GetEndpoint("stream").Property(EndpointProperty.Port);

var usersApi = builder.AddProject<Projects.Users_Api>("users-api")
    .WithReference(usersDb)
    .WaitFor(usersDb)
    .WithEnvironment("Messaging__Kind", transportKind)
    .WithEnvironment("Messaging__ConnectionString", messaging)
    .WithEnvironment("Messaging__StreamPort", streamPort)
    .WaitFor(messaging)
    .WithHttpHealthCheck("/health");

var notificationsApi = builder.AddProject<Projects.Notifications_Api>("notifications-api")
    .WithReference(notificationsDb)
    .WaitFor(notificationsDb)
    .WithEnvironment("Messaging__Kind", transportKind)
    .WithEnvironment("Messaging__ConnectionString", messaging)
    .WithEnvironment("Messaging__StreamPort", streamPort)
    .WaitFor(messaging)
    .WithEnvironment("Smtp__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
    .WithEnvironment("Smtp__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port))
    .WaitFor(mailpit)
    .WithHttpHealthCheck("/health");

// A hand-testing console, not part of the measured system — see Web/Web.csproj. WithReference
// here wires Aspire service discovery so it can resolve "http://users-api"/"http://notifications-api"
// without hard-coded ports.
builder.AddProject<Projects.Web>("web")
    .WithReference(usersApi)
    .WithReference(notificationsApi)
    .WaitFor(usersApi)
    .WaitFor(notificationsApi);

builder.Build().Run();
