using System.Text.Json;

namespace NotificationLog.Web.Api;

internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
