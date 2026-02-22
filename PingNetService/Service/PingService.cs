using Microsoft.Extensions.Logging;

namespace PingNetService.Service;

public class PingService(ILogger<LocationService> logger)
{
    public async Task<int> Ping(string ipAddress, int repeats = 10)
    { 
        logger.LogTrace("Ping({IpAddress}, {Repeats})", ipAddress, repeats);
        
        var ping = new System.Net.NetworkInformation.Ping();
        var totalTime = 0;
        for (var i = 0; i < repeats; i++)
        {
            var reply = await ping.SendPingAsync(ipAddress);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                totalTime += (int)reply.RoundtripTime;
            }
            else
            {
                throw new Exception($"Ping to {ipAddress} failed with status: {reply.Status}");
            }
        }
        return totalTime / repeats;
    }
}
