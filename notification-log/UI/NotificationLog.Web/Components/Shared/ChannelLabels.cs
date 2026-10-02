namespace NotificationLog.Web.Components.Shared;

public static class ChannelLabels
{
    public static string Name(string channel) => channel == "Sms" ? "SMS" : channel;

    public static string Variant(string channel) => channel switch
    {
        "Email" => "info",
        "Sms" => "success",
        "Push" => "warning",
        _ => "neutral"
    };
}
