#pragma warning disable CS8618
using ImportBonuse.Postgres.Entity;
using Microsoft.EntityFrameworkCore;
namespace ImportBonus.Postgres;

public class PostgresDbContext : DbContext
{
    public DbSet<Transaction> Transactions  { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Transaction>().HasIndex(x => new { x.PersonId, x.BankId });
        builder.Entity<Transaction>().HasIndex(x => x.TransactionId)
            .IncludeProperties(x=> new { x.BonusSum })
            .IsUnique();
    }
}
