using BonusApi;
using BonusService.Common;
using BonusService.Postgres;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IAsyncDisposable
{
    public static class Q
    {
        public const long Sum2000 = 2000;
        public const long Sum1000 = 1000;
        public const long Sum500 = 500;
        public const string Description1 = "Описанье 1";
        public const string Description2 = "Описанье 2";
        public readonly static Guid UserId1 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        public readonly static Guid UserId2 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66af45");
        public const string TransactionId1 = "3fa85f64-5717-4562-b3fc-2c963f66af11";
        public const string TransactionId2 = "3fa85f64-5717-4562-b3fc-2c963f66af12";
        public const string TransactionId3 = "33385f64-5717-4562-b3fc-2c963f66af12";
        public static string GetRandomTransactionId() => Guid.NewGuid().ToString("N");
        public readonly static Guid PersonId1 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        public readonly static Guid PersonId2 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa7");
        public const int BankIdRub = 1;
        public const  int BankIdKaz = 7;
        public static readonly TimeSpan timezone = new(0, 0, 0);
        public static DateTimeOffset [] DateTimeSequence =
        {
            new (2001, 1, 2, 3, 4, 5, timezone),
            new (2001, 1, 3, 4, 5, 6, timezone),
            new (2001, 2, 3, 5, 3, 1, timezone),
            new (2002, 1, 2, 3, 4, 5, timezone),
            new (2003, 1, 2, 21, 4, 5, timezone),
            new (2004, 1, 2, 3, 4, 6, timezone),
        };
    }

    protected async Task InitTransactionTran1Person1BankRub(long bonusBalance)
    {
        await postgres.Transactions.AddAsync(new Transaction()
        {
            BonusSum = bonusBalance,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            Description = Q.Description1,
            Type = TransactionType.Manual,
            UserId = Q.UserId1
        });
        await postgres.SaveChangesAsync();
    }

    protected readonly BonusClient api;
    protected readonly FakeApplicationFactory<Program> server;
    protected readonly IServiceScope scope;
    protected IServiceScope CreateScope() => server.Services.CreateScope();
    protected readonly PostgresDbContext postgres;

    public BonusTestApi(FakeApplicationFactory<Program> server)
    {
        this.server = server;
        InitDatabases(server).GetAwaiter().GetResult();
        var httpClient = server.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        api = new BonusClient(httpClient);
        A.CallTo(() => this.server.DateTimeService.GetNowUtc()).ReturnsNextFromSequence(Q.DateTimeSequence);
        scope = CreateScope();
        postgres = scope.GetRequiredService<PostgresDbContext>();
    }

    protected async Task InitDatabases(FakeApplicationFactory<Program> server)
    {
        var postgres = InfraHelper.RunPostgresContainer();
        var mongo = InfraHelper.RunMongo(this.server.DbName).ContinueWith((t) =>
        {
            InfraHelper.CreateMongoDatabase(this.server.DbName);
        });
        await Task.WhenAll(postgres, mongo);
    }


    public async ValueTask DisposeAsync()
    {
        var postgresDelete = postgres.Database.EnsureDeletedAsync();

        var hangfire = scope.ServiceProvider.GetRequiredService<HangfireServicesExt.HangfireDbContext>();
        var hangfireDelete = hangfire.Database.EnsureDeletedAsync();

        var mongoDelete = InfraHelper.DropMongoDatabase(server.DbName);
        var tasks = new [] { postgresDelete, hangfireDelete, mongoDelete };
        await Task.WhenAll(tasks);
        scope.Dispose();
    }
}
