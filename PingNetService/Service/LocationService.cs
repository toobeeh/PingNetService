using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nager.Country;
using PingNetService.Database;
using PingNetService.Database.Model;

namespace PingNetService.Service;

public class LocationService(
    ILogger<LocationService> logger,
    AppDatabaseContext db
    )
{
    public async Task<LocationEntity> AddPingLocation(string name, string ipAddress, string regionCode, string countryCode, int pingMs)
    {
        logger.LogTrace("AddPingLocation({Name}, {IpAddress}, {RegionCode}, {CountryCode})", name, ipAddress, regionCode, countryCode);

        var countryProvider = new CountryProvider();
        var country = countryProvider.GetCountry(countryCode);
        var continent = country.Region.ToString();

        var location = new LocationEntity
        {
            Name = name,
            IpAddress = ipAddress,
            RegionCode = regionCode,
            CountryCode = countryCode,
            ContinentName = continent,
            PingMs = pingMs
        };

        db.Locations.Add(location);
        await db.SaveChangesAsync();
        return location;
    }

    public async Task RemovePingLocation(string ipAddress)
    {
        logger.LogTrace("RemovePingLocation({IpAddress})", ipAddress);

        var location = await db.Locations.FindAsync(ipAddress);
        if (location != null)
        {
            db.Locations.Remove(location);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<LocationEntity>> GetAllPingLocations()
    {
        logger.LogTrace("GetAllPingLocations()");

        return await db.Locations.ToListAsync();
    }

    public async Task UpdatePing(string ipAddress, int pingMs)
    {
        logger.LogTrace("UpdatePing({IpAddress}, {PingMs})", ipAddress, pingMs);

        var location = await db.Locations.FindAsync(ipAddress);
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found"));
        }

        location.PingMs = pingMs;
        await db.SaveChangesAsync();
    }

    public async Task<LocationEntity?> GetPingForRegion(string countryCode, string regionCode)
    {
        logger.LogTrace("GetPingForRegion({CountryCode}, {RegionCode})", countryCode, regionCode);

        return await db.Locations.Where(l => l.CountryCode == countryCode && l.RegionCode == regionCode).OrderBy(l => l.PingMs).FirstOrDefaultAsync();
    }

    public async Task<LocationEntity?> GetPingForCountry(string countryCode)
    {
        logger.LogTrace("GetPingForCountry({CountryCode})", countryCode);

        return await db.Locations.Where(l => l.CountryCode == countryCode).OrderBy(l => l.PingMs).FirstOrDefaultAsync();
    }

    public async Task<LocationEntity?> GetPingForContinent(string countryCode)
    {
        logger.LogTrace("GetPingForContinent({CountryCode})", countryCode);

        var countryProvider = new CountryProvider();
        var country = countryProvider.GetCountry(countryCode);
        var continent = country.Region.ToString();

        return await db.Locations.Where(l => l.ContinentName == continent).OrderBy(l => l.PingMs).FirstOrDefaultAsync();
    }

    public async Task<LocationEntity?> GetMinPingLocation()
    {
        logger.LogTrace("GetMinPingLocation()");

        return await db.Locations.OrderBy(l => l.PingMs).FirstOrDefaultAsync();
    }

    public async Task<LocationEntity?> GetMaxPingLocation()
    {
        logger.LogTrace("GetMaxPingLocation()");

        return await db.Locations.OrderByDescending(l => l.PingMs).FirstOrDefaultAsync();
    }
}
