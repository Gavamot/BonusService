#pragma warning disable CS8618
using BonusService.Common.Postgres.Entity;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Common.Postgres;

// dotnet ef migrations add InitialCreate --context PostgresDbContext
public static class PostgresExt
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetConnectionString("Postgres") ?? throw new Exception("ConnectionString:Postgres does not exist");
        return services.AddDbContext<PostgresDbContext>(
            opt=> opt.UseNpgsql(conStr)
            );
    }

    public static void ApplyPostgresMigrations(this IApplicationBuilder app)
    {
        if(Program.IsNswagBuild()) return;
        using var scope = app.ApplicationServices.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        ctx.Database.Migrate();
    }
}

// Обновить .net tools for ef
// dotnet tool update --global dotnet-ef
public class PostgresDbContext : DbContext
{
    public DbSet<Transaction> Transactions  { get; set; }
    public DbSet<TransactionHistory> TransactionHistory  { get; set; }
    public DbSet<OwnerMaxBonusPay> OwnerMaxBonusPays { get; set; }

    public DbSet<BonusProgram?> BonusPrograms { get; set; }
    public DbSet<BonusProgramLevel> BonusProgramsLevels { get; set; }
    public DbSet<BonusProgramHistory> BonusProgramHistory { get; set; }

    public IQueryable<BonusProgram> GetBonusPrograms()
    {
        return BonusPrograms.Where(x => x.IsDeleted == false)
            .Include(x=> x.ProgramLevels)
            .AsNoTracking();
    }

    public BonusProgram? GetBonusProgramById(int id, CancellationToken ct = default)
    {
        return this.BonusPrograms
            .Include(x => x.ProgramLevels)
            .AsNoTracking()
            .FirstOrDefault(x => x.IsDeleted == false);
    }


    public PostgresDbContext(){ }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options): base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<BonusProgram>().HasMany(x=>x.ProgramLevels);
        builder.Entity<BonusProgram>().HasMany(x=>x.BonusProgramHistory);

        builder.Entity<BonusProgramLevel>().HasOne(x=>x.BonusProgram);
        builder.Entity<BonusProgramHistory>().HasOne(x => x.BonusProgram);

        builder.Entity<Transaction>().HasIndex(x => new { x.PersonId, x.BankId });
        builder.Entity<Transaction>().HasIndex(x => x.TransactionId)
            .IncludeProperties(x=> new { x.BonusSum })
            .IsUnique();

        builder.Entity<OwnerMaxBonusPay>().HasIndex(x => x.OwnerId).IsUnique();
    }
}
