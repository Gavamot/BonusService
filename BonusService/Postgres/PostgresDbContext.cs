#pragma warning disable CS8618
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
    public DbSet<Program> Programs { get; set; }
    public DbSet<ProgramLevel> ProgramLevels  { get; set; }
    public DbSet<Transaction> Transactions  { get; set; }
    public DbSet<BalanceRegister> BalanceRegister  { get; set; }

    public PostgresDbContext(){ }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options): base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Program>().HasMany(x=>x.ProgramLevels);

        builder.Entity<ProgramLevel>().HasOne(x=>x.Program);

        builder.Entity<Transaction>().HasOne(x => x.Program);

        builder.Entity<BalanceRegister>().HasIndex(x => new { x.PersonId, x.BankId }).IsUnique();
    }
}
