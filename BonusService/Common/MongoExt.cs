using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
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
        services.Configure<MongoConfig>(configuration.GetSection(nameof(MongoConfig)));

        services.AddSingleton<MongoDbContext>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoConfig>>().Value;
            return new MongoDbContext(settings.ConnectionString, settings.Database);
        });
        //var config = configuration.GetRequiredSection(nameof(MongoConfig)).Get<MongoConfig>() ?? throw new AggregateException();
        //services.AddMongoDB<MongoDbContext>(config.ConnectionString, config.Database);
        return services;
    }
}


public class MongoDbContext //: DbContext
{
    public IMongoDatabase Database { get; init; }
    public const string SessionCollection = "cp_sessions";
    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<MongoSession> Sessions => Database.GetCollection<MongoSession>(SessionCollection);

    /*public DbSet<MongoSession> Sessions { get; set; }
    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MongoSession>().ToCollection("cp_sessions");

    }*/
}

public class MongoSession
{
    public ObjectId _id { get; set; }
    public int? status { get; set; }
    public DateTime chargeEndTime { get; set; }

    public MongoUser? user { get; set; }

    public MongoOperation? operation { get; set; }

    public MongoTariff? tariff { get; set; }
}

public class MongoUser
{
    public Guid? clientNodeId { get; set; }

    public string? clientLogin { get; set; }

    public int? chargingClientType { get; set; }
}

public class MongoOperation
{
    public long? calculatedPayment { get; set; }
}

public class MongoTariff
{
    public int? BankId { get; set; }
}
