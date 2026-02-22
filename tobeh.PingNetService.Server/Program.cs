using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PingNetService.Config;
using PingNetService.Database;
using PingNetService.Grpc;
using PingNetService.Service;

namespace PingNetService;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        AppDatabaseContext.EnsureDatabaseExists();

        var builder = WebApplication.CreateBuilder(args);

        var root = (IConfigurationRoot)builder.Configuration;

        // configure kestrel
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Setup a HTTP/2 endpoint without TLS.
            options.ListenAnyIP(builder.Configuration.GetRequiredSection("Grpc").GetValue<int>("HostPort"), o => o.Protocols = HttpProtocols.Http2);
        });

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddDbContext<AppDatabaseContext>();
        builder.Services.AddHttpClient();
        builder.Services.AddLogging();
        builder.Services.AddScoped<PingService>();
        builder.Services.AddScoped<LocationService>();
        builder.Services.AddSingleton<GeoService>();
        
        builder.Services.Configure<GeoConfig>(builder.Configuration.GetRequiredSection("Geo"));

        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.MapGrpcService<PingNetGrpcService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}
