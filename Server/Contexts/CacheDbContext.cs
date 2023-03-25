using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelsDLL;

namespace Server.Contexts;

internal class CacheDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

        var connectionString = configuration.GetConnectionString("DBConnection");

        optionsBuilder.UseSqlServer(connectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyValue>().HasKey(e => e.Key);
        base.OnModelCreating(modelBuilder);
    }

    DbSet<KeyValue> KeyValues { get; set; }
}
