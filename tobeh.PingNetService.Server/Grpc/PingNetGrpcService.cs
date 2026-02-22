using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PingNetService.Database.Model;
using PingNetService.Service;
using tobeh.PingNetService;

namespace PingNetService.Grpc;

public class PingNetGrpcService(
    ILogger<PingNetGrpcService> logger,
    LocationService locationService,
    PingService pingService,
    GeoService geoService
    ) : PingNet.PingNetBase
{
    public override async Task<PingResultMessage> GetPingForLocation(LocationInfoMessage request, ServerCallContext context)
    {
        logger.LogTrace("GetPingForLocation({RegionCode}, {CountryCode})", request.RegionCode, request.CountryCode);

        return await GetPingForNearestLocation(request.RegionCode, request.CountryCode);
    }

    public override async Task<PingResultMessage> GetMaxPingLocation(Empty request, ServerCallContext context)
    {
        logger.LogTrace("GetMaxPingLocation()");

        var location = await locationService.GetMaxPingLocation();
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "No ping locations found"));
        }

        return new PingResultMessage
        {
            Location = new LocationInfoMessage
            {
                RegionCode = location.RegionCode,
                CountryCode = location.CountryCode
            },
            PingMs = location.PingMs,
            LookupMode = LookupMode.Global
        };
    }

    public override async Task<PingResultMessage> GetMinPingLocation(Empty request, ServerCallContext context)
    {
        logger.LogTrace("GetMinPingLocation()");

        var location = await locationService.GetMinPingLocation();
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "No ping locations found"));
        }

        return new PingResultMessage
        {
            Location = new LocationInfoMessage
            {
                RegionCode = location.RegionCode,
                CountryCode = location.CountryCode
            },
            PingMs = location.PingMs,
            LookupMode = LookupMode.Global
        };
    }

    public override async Task<PingResultMessage> AddPingLocation(AddPingLocationMessage request, ServerCallContext context)
    {
        logger.LogTrace("AddPingLocation({IpAddress}, {Name})", request.IpAddress, request.Name);
        
        var pingMs = await pingService.Ping(request.IpAddress);
        var region = geoService.GetLocationForIp(request.IpAddress);
        
        if(region.RegionCode == null || region.CountryCode == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Could not determine region or country for IP address"));
        }
        
        var location = await locationService.AddPingLocation(request.Name, request.IpAddress, region.RegionCode, region.CountryCode, pingMs);

        return new PingResultMessage
        {
            Location = new LocationInfoMessage
            {
                RegionCode = location.RegionCode,
                CountryCode = location.CountryCode
            },
            PingMs = pingMs,
            LookupMode = LookupMode.Region
        };
    }

    public override async Task<PingResultMessage> GetPingForIp(IpAddressMessage request, ServerCallContext context)
    {
        logger.LogTrace("GetPingForIp({IpAddress})", request.IpAddress);

        var region = geoService.GetLocationForIp(request.IpAddress);
        
        return await GetPingForNearestLocation(region.RegionCode, region.CountryCode);
    }

    public override async Task<Empty> RemovePingLocation(LocationInfoMessage request, ServerCallContext context)
    {
        logger.LogTrace("RemovePingLocation({RegionCode}, {CountryCode})", request.RegionCode, request.CountryCode);

        var location = await locationService.GetPingForRegion(request.CountryCode, request.RegionCode);
        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Location not found for region code"));
        }

        await locationService.RemovePingLocation(location.IpAddress);
        return new Empty();
    }

    public override async Task GetAllPingLocations(Empty request, IServerStreamWriter<PingResultMessage> responseStream, ServerCallContext context)
    {
        logger.LogTrace("GetAllPingLocations()");

        var locations = await locationService.GetAllPingLocations();
        foreach (var location in locations)
        {
            await responseStream.WriteAsync(new PingResultMessage
            {
                Location = new LocationInfoMessage
                {
                    RegionCode = location.RegionCode,
                    CountryCode = location.CountryCode
                },
                PingMs = location.PingMs,
                LookupMode = LookupMode.Region
            });
        }
    }

    public override async Task<Empty> RefreshPingLocations(Empty request, ServerCallContext context)
    {
        logger.LogTrace("RefreshPingLocations()");

        var locations = await locationService.GetAllPingLocations();
        foreach (var location in locations)
        {
            var pingMs = await pingService.Ping(location.IpAddress);
            await locationService.UpdatePing(location.IpAddress, pingMs);
        }

        return new Empty();
    }
    
    private async Task<PingResultMessage> GetPingForNearestLocation(string? regionCode, string? countryCode)
    {
        logger.LogTrace("GetPingForLocation({RegionCode}, {CountryCode})", regionCode, countryCode);

        LocationEntity? location = null;
        var mode = LookupMode.Region;

        if (regionCode != null && countryCode != null)
        {
            location = await locationService.GetPingForRegion(countryCode, regionCode);
            mode = LookupMode.Region;
        }

        if (location == null && countryCode != null)
        {
            location = await locationService.GetPingForCountry(countryCode);
            mode = LookupMode.Country;
        }

        if (location == null && countryCode != null)
        {
            location = await locationService.GetPingForContinent(countryCode);
            mode = LookupMode.Continent;
        }

        if (location == null)
        {
            location = await locationService.GetMinPingLocation();
            mode = LookupMode.Global;
        }

        if (location == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "No ping locations found"));
        }

        return new PingResultMessage
        {
            Location = new LocationInfoMessage
            {
                RegionCode = location.RegionCode,
                CountryCode = location.CountryCode
            },
            PingMs = location.PingMs,
            LookupMode = mode
        };
    }
}
