using ImportBonuse.Postgres.Entity;
using Microsoft.EntityFrameworkCore;
namespace ImportBonuse.Postgres;

public class PostgresDbContext : DbContext
{
    private readonly string _conStr;
    public PostgresDbContext(string conStr)
    {
        _conStr = conStr;

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_conStr);
    }

    public DbSet<Transaction> Transactions  { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Transaction>().HasIndex(x => new { x.PersonId, x.BankId });
        builder.Entity<Transaction>().HasIndex(x => x.TransactionId)
            .IncludeProperties(x=> new { x.BonusSum })
            .IsUnique();
    }
}
