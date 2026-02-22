using MaxMind.GeoIP2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PingNetService.Config;

namespace PingNetService.Service;

public record GeoInfo(string IpAddress, string? CountryCode, string? RegionCode);

public class GeoService(ILogger<GeoService> logger, IOptions<GeoConfig> config)
{
    private readonly DatabaseReader _reader = new DatabaseReader(config.Value.CityDbPath);
    
    public GeoInfo GetLocationForIp(string ipAddress)
    {
        logger.LogTrace("GetLocationForIp({IpAddress})", ipAddress);
        
        try
        {
            var city = _reader.City(ipAddress);
            if (city.Country.IsoCode == null)
            {
                logger.LogDebug("No country found for IP address {IpAddress}", ipAddress);
            }
            
            if(city.MostSpecificSubdivision.IsoCode == null)
            {
                logger.LogDebug("No region found for IP address {IpAddress}", ipAddress);
            }
            
            return new GeoInfo(ipAddress, city.Country.IsoCode, city.MostSpecificSubdivision.IsoCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get location for IP address {IpAddress}", ipAddress);
            return new GeoInfo(ipAddress, null, null);
        }
    }
}