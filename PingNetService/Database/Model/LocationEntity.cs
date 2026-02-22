using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PingNetService.Database.Model;

[PrimaryKey(nameof(IpAddress))]
[Index(nameof(Name), nameof(RegionCode), IsUnique = true)]
public class LocationEntity
{
    public string IpAddress { get; set; }
    public int PingMs { get; set; }
    public string Name { get; set; }
    public string RegionCode { get; set; }
    public string CountryCode { get; set; }
    public string ContinentName { get; set; }
}
