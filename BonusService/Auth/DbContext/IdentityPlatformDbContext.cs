

using Microsoft.EntityFrameworkCore;
using PlatformWebApi.Models.Models.Identity.Entity;

namespace BonusService.Auth.DbContext;

public class IdentityPlatformDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly IConfiguration _configuration;

    public IdentityPlatformDbContext(IConfiguration configuration)
        => _configuration = configuration;
    
    public DbSet<IdentitySystem> IdentitySystem { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration["IdentitySettings:ConnectionString"]);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<IdentitySystem>()
            .HasKey(x => x.Name);
    }
}
