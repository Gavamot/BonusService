#pragma warning disable CS8618
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Postgres;

public static class PostgresDbContextExt
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetConnectionString("Postgres") ?? throw new Exception("ConnectionString:Postgres does not exist");
        return services.AddDbContext<PostgresDbContext>(opt=> opt.UseNpgsql(conStr));
    }

    public static void ApplyPostgresMigrations(this IApplicationBuilder app)
    {
        var isNswagGenerator = Environment.GetEnvironmentVariable("NswagGen");
        if (string.IsNullOrWhiteSpace(isNswagGenerator) == false) return;
        using var scope = app.ApplicationServices.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        ctx.Database.EnsureCreated();
        ctx.Database.Migrate();
    }
}

// Обновить .net tools for ef
// dotnet tool update --global dotnet-ef
public class PostgresDbContext : DbContext
{
    //public DbSet<BonusProgram> BonusPrograms { get; set; }
    //public DbSet<BonusProgramLevel> BonusProgramLevels  { get; set; }
    public DbSet<Transaction> Transactions  { get; set; }
    public DbSet<BalanceRegister> BalanceRegister  { get; set; }

    public PostgresDbContext(){ }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options): base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.Entity<BonusProgram>().HasMany(x=>x.ProgramLevels);

        //builder.Entity<BonusProgramLevel>().HasOne(x=>x.BonusProgram);

        //builder.Entity<Transaction>().HasOne(x => x.Program);
        builder.Entity<Transaction>().HasIndex(x => x.TransactionId).IsUnique();
        builder.Entity<BalanceRegister>().HasIndex(x => new { x.PersonId, x.BankId, x.Date }).IsUnique();
    }
}
