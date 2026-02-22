using Microsoft.EntityFrameworkCore;
using PingNetService.Database.Model;

namespace PingNetService.Database
{
    public class AppDatabaseContext : DbContext
    {
        private const string Path = "./data";
        private static string DbPath => System.IO.Path.Combine(Path, "app.db");

        public static void EnsureDatabaseExists()
        {
            Directory.CreateDirectory(Path);
            var ctx = new AppDatabaseContext();
            ctx.Database.EnsureCreated();
            ctx.Dispose();
        }

        public DbSet<LocationEntity> Locations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use SQLite as the database provider
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }
}
