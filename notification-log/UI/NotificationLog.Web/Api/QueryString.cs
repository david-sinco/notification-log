namespace NotificationLog.Web.Api;

internal static class QueryString
{
    public static string Build(params (string Key, string? Value)[] parameters)
    {
        var parts = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");

        var joined = string.Join("&", parts);
        return joined.Length == 0 ? "" : $"?{joined}";
    }
}
