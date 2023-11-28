using System.Net.Http.Headers;
using BonusApi;
using BonusService.Common;
using BonusService.Postgres;
using FakeItEasy;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IAsyncDisposable
{
    public static class Q
    {
        public static Transaction CreateTransaction(Guid personId, int bankId = Q.BankIdRub, long sum = Q.Sum1000) => new ()
        {
            Description = Q.Description1,
            Type = TransactionType.Auto,
            TransactionId = GetRandomTransactionId(),
            PersonId = personId,
            BankId = bankId,
            BonusSum = sum,
        };

        public readonly static DateTimeInterval IntervalMoth1 = new (new DateTimeOffset(2000, 1, 1, 1, 1, 1, new TimeSpan(0)), new DateTimeOffset(2000, 2, 1, 1, 1, 1, new TimeSpan(0)));
        public readonly static Guid EzsId1 = Guid.Parse("3fa85f64-5717-aaaa-b3fc-2c222f66afa6");
        public readonly static Guid EzsId2 = Guid.Parse("3fa85f61-5717-aaaa-b3fc-2c222f66afa6");

        public readonly static int OwnerId1 = 1;
        public readonly static int OwnerId2 = 2;

        public const long SumLevel1 = 1_000_00;
        public const long SumLevel2 = 3_000_00;
        public const long Sum2000 = 20_00;
        public const long Sum1000 = 10_00;
        public const long Sum500 = 5_00;
        public const string Description1 = "Описанье 1";
        public const string Description2 = "Описанье 2";
        public readonly static string UserName = "Admin";
        public const string TransactionId1 = "3fa85f64-5717-4562-b3fc-2c963f66af11";
        public const string TransactionId2 = "3fa85f64-5717-4562-b3fc-2c963fa6af12";
        public const string TransactionId3 = "33385f64-5717-4562-b3fc-2c963f66af12";
        public static string GetRandomTransactionId() => Guid.NewGuid().ToString("N");
        public readonly static Guid PersonId1 = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa6");
        public readonly static string PersonId1String = "5fa85f64-5717-4562-b3fc-2c963f66afa6";
        public readonly static Guid PersonId2 = Guid.Parse("6fa85f64-5717-4562-b3fc-2c963f66afa7");
        public readonly static string PersonId2String = "6fa85f64-5717-4562-b3fc-2c963f66afa7";


        public const int BankIdRub = 1;
        public const  int BankIdKaz = 7;
        public static readonly int [] AllBanks = new [] { BankIdRub, BankIdKaz };
        public static readonly TimeSpan timezone = new(0, 0, 0);

        public static readonly DateTimeOffset [] DateTimeSequence =
        {
            new (2001, 1, 2, 3, 4, 5, timezone),
            new (2001, 1, 3, 4, 5, 6, timezone),
            new (2001, 2, 3, 5, 3, 1, timezone),
            new (2002, 1, 2, 3, 4, 5, timezone),
            new (2003, 1, 2, 21, 4, 5, timezone),
            new (2004, 1, 2, 3, 4, 6, timezone),
        };

        public const string ClientLogin = "8909113342";
        public const string ClientLogin2 = "8909163142";
        public static MongoSession CreateSession(DateTime date) => new MongoSession()
        {
            //_id = ObjectId.GenerateNewId(),
            operation = new MongoOperation()
            {
                calculatedPayment = Q.SumLevel2
            },
            chargeEndTime = date,
            status = MongoSessionStatus.Paid,
            tariff = new MongoTariff()
            {
                BankId = Q.BankIdRub
            },
            user = new MongoUser()
            {
                clientLogin = ClientLogin,
                chargingClientType = MongoChargingClientType.IndividualEntity,
                clientNodeId = Q.PersonId1String
            }
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
            UserName = Q.UserName
        });
        await postgres.SaveChangesAsync();
    }

    protected BonusClient api
    {
        get
        {
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiQWRtaW4iLCJSb2xlcyI6IkFkbWluIiwibmJmIjoxNzAwNjUyODQzLCJleHAiOjE3OTA2NTI4NDMsImlhdCI6MTcwMDY1Mjg0MywiaXNzIjoiUGxhdGZvcm1XZWJBcGkifQ.C3zc6s9FH7emLZVpRyaulc_aw2QD4gNzaUNTLXVnj_FDhSQzDxijr7aWYrT3XT2gPziHYUFh8uBtIAY_nfQ3Mw");
            return new BonusClient(client);
        }
    }
    protected readonly FakeApplicationFactory<Program> server;
    protected readonly IServiceScope scope;
    protected T GetService<T>() => scope.GetRequiredService<T>();
    protected IServiceScope CreateScope() => server.Services.CreateScope();
    protected readonly PostgresDbContext postgres;
    protected readonly MongoDbContext mongo;
    protected IBackgroundJobClientV2 jobClient;
    protected readonly HangfireDbContext hangfireDb;

    public BonusTestApi(FakeApplicationFactory<Program> server)
    {
        this.server = server;
        //InitDatabases(server).GetAwaiter().GetResult();
        A.CallTo(() => this.server.DateTimeService.GetNowUtc()).
            ReturnsNextFromSequence(Q.DateTimeSequence);
        A.CallTo(() => this.server.DateTimeService.GetCurrentMonth())
            .ReturnsNextFromSequence(Q.IntervalMoth1);

        scope = CreateScope();
        postgres = scope.GetRequiredService<PostgresDbContext>();
        postgres.Database.EnsureDeleted();
        postgres.Database.Migrate();
        Program.AddPostgresSeed(server.Services);

        //InfraHelper.CreateMongoDatabase(this.server.DbName).GetAwaiter().GetResult();
        mongo = scope.GetRequiredService<MongoDbContext>();
        mongo.Database.DropCollection(MongoDbContext.SessionCollection);
        jobClient = scope.GetRequiredService<IBackgroundJobClientV2>();
    }

    protected async Task InitDatabases(FakeApplicationFactory<Program> server)
    {
        var postgres = InfraHelper.RunPostgresContainer();
        var mongo = InfraHelper.RunMongo(this.server.DbName);
        await Task.WhenAll(postgres, mongo);
    }

    public async ValueTask DisposeAsync()
    {
        /*var postgresDelete = postgres.Database.EnsureDeletedAsync();

        var hangfire = scope.ServiceProvider.GetRequiredService<HangfireServicesExt.HangfireDbContext>();
        var hangfireDelete = hangfire.Database.EnsureDeletedAsync();

      //mongo.Database.DropCollection(MongoDbContext.SessionCollection);
        var mongoDelete = InfraHelper.DropMongoDatabase(server.DbName);
        var tasks = new [] { postgresDelete, hangfireDelete, mongoDelete };
        await Task.WhenAll(tasks);
        scope.Dispose();*/
    }
}
