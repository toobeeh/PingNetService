using Microsoft.Extensions.Logging;

namespace PingNetService.Service;

public class PingService(ILogger<LocationService> logger)
{
    public async Task<int> Ping(string ipAddress)
    {
        var ping = new System.Net.NetworkInformation.Ping();
        var reply = await ping.SendPingAsync(ipAddress);
        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            return (int)reply.RoundtripTime;
        }

        throw new Exception($"Ping to {ipAddress} failed with status: {reply.Status}");
    }
}
