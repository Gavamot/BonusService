using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
namespace BonusService.Common;

public class MongoConfig
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    [Required]
    public string QueriesFolder { get; set; } = string.Empty;
    [Required]
    public string Database { get; set; } = string.Empty;
}

public static class MongoExt
{
    public static IServiceCollection AddMongoService(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetRequiredSection(nameof(MongoConfig)).Get<MongoConfig>() ?? throw new AggregateException();
        services.AddMongoDB<MongoDbContext>(config.ConnectionString, config.Database);
        return services;
    }
}


public class MongoDbContext : DbContext
{
    public DbSet<MongoSession> Sessions { get; init; }

    public static MongoDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<MongoDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);

    public MongoDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MongoSession>().ToCollection("cp_sessions");

    }
}

public class MongoSession
{
    public ObjectId _id { get; set; }
    public int? status { get; set; }
    public DateTime? chargeEndTime { get; set; }

    public MongoUser? user { get; set; }

    public MongoOperation? operation { get; set; }

    public MongoTariff? tariff { get; set; }
}

public class MongoUser
{
    public string? clientNodeId { get; set; }

    public string? clientLogin { get; set; }

    public int? chargingClientType { get; set; }
}

public class MongoOperation
{
    public double? calculatedPayment { get; set; }
}

public class MongoTariff
{
    public int? BankId { get; set; }
    public int? CurrencyId { get; set; }
}
