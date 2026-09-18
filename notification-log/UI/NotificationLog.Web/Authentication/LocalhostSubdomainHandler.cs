using System.Net.Sockets;

namespace NotificationLog.Web.Authentication;

public static class LocalhostSubdomainHandler
{
    public static SocketsHttpHandler Create() => new()
    {
        ConnectCallback = async (context, ct) =>
        {
            var host = context.DnsEndPoint.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
                ? "localhost"
                : context.DnsEndPoint.Host;

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

            try
            {
                await socket.ConnectAsync(host, context.DnsEndPoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };
}
