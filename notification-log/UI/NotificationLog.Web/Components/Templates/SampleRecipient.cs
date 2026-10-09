namespace NotificationLog.Web.Components.Templates;

public sealed record SampleRecipient(string Key, string Label, IReadOnlyDictionary<string, string> Values)
{
    public static IReadOnlyList<SampleRecipient> All { get; } =
    [
        new("camila", "Camila Ramírez", new Dictionary<string, string>
        {
            ["recipient.name"] = "Camila Ramírez",
            ["recipient.email"] = "camila@correo.co",
            ["recipient.phone"] = "+57 300 555 0142",
            ["recipient.locale"] = "es-CO",
            ["recipient.timeZone"] = "America/Bogota"
        }),
        new("andres", "Andrés Gómez · sin correo", new Dictionary<string, string>
        {
            ["recipient.name"] = "Andrés Gómez",
            ["recipient.email"] = "",
            ["recipient.phone"] = "+57 311 204 8890",
            ["recipient.locale"] = "es-CO",
            ["recipient.timeZone"] = "America/Bogota"
        })
    ];

    public string Name => Values["recipient.name"];

    public string Email => Values["recipient.email"];

    public string Phone => Values["recipient.phone"];
}
