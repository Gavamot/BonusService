using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusApi;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FakeItEasy;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Transaction = BonusService.Common.Postgres.Entity.Transaction;
using TransactionType = BonusService.Common.Postgres.Entity.TransactionType;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IAsyncLifetime
{
    private string ToJson(object a)
    {
        var opt = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        return JsonSerializer.Serialize(a, opt);
    }
    protected void AssertEqualJsonUnSensitiveToRegister(object expected, object actual)
    {
        var shodBe = ToJson(actual).ToLower();
        var itIs = ToJson(expected).ToLower();
        itIs.Should().Be(shodBe);
    }
    public static class Q
    {
        public static void AddPostgresSeed(IServiceProvider serviceProvider)
        {
            // Времянка пока юонусные программы захардкоженны
            using var scope1 = serviceProvider.CreateScope();
            var postgres = scope1.ServiceProvider.GetRequiredService<PostgresDbContext>();
            var bp = postgres.BonusPrograms.FirstOrDefault(x => x.Id == 1);
            if (bp == null)
            {
                bp = BonusProgramSeed.Get();
                bp.Id = 0;
                postgres.BonusPrograms.Add(bp);
                postgres.SaveChanges();
            }
        }
        public static Transaction CreateTransaction(string personId, int bankId = Q.BankIdRub, long sum = Q.Sum1000) => new ()
        {
            Description = Q.Description1,
            Type = TransactionType.Auto,
            TransactionId = GetRandomTransactionId(),
            PersonId = personId,
            BankId = bankId,
            BonusSum = sum,
        };

        public static readonly DateTimeOffset IntervalMoth1Start = new (2023, 11, 1, 0, 0, 0, new TimeSpan(0));
        public static readonly DateTimeOffset IntervalMoth1End = IntervalMoth1Start.AddMonths(1);
        public readonly static DateTimeExt TimeExtMoth1 = new (IntervalMoth1Start, IntervalMoth1End);

        public readonly static Guid EzsId1 = Guid.Parse("3fa85f64-5717-aaaa-b3fc-2c222f66afa6");
        public readonly static Guid EzsId2 = Guid.Parse("3fa85f61-5717-aaaa-b3fc-2c222f66afa6");

        public readonly static int OwnerId1 = 2;
        public readonly static int OwnerId2 = 3;
        public readonly static int OwnerId3 = 4;

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
        public readonly static string PersonId1 = "5fa85f64-5717-4562-b3fc-2c963f66afa6";
        public readonly static string PersonId2 = "6fa85f64-5717-4562-b3fc-2c963f66afa7";

        public const int BonusProgramId1 = 11;
        public const int BonusProgramId2 = 22;
        public const int BankIdRub = 1;
        public const  int BankIdKaz = 7;
        public static readonly int [] AllBanks = new [] { BankIdRub, BankIdKaz };
        public static readonly TimeSpan timezone = new(0, 0, 0);

        public static readonly DateTimeOffset [] DateTimeSequence =
        {
            IntervalMoth1Start.AddHours(1),
            IntervalMoth1Start.AddDays(5),
            IntervalMoth1Start.AddDays(5).AddHours(4),
            IntervalMoth1Start.AddMonths(1).AddDays(2).AddHours(4),
            IntervalMoth1Start.AddYears(1).AddDays(2).AddHours(4),
        };

        public const string ClientLogin = "8909113342";
        public const string ClientLogin2 = "8909163142";
        public static MongoSession CreateSession(DateTimeOffset date, long payment = Q.SumLevel2) => CreateSession(date.UtcDateTime, payment);
        public static MongoSession CreateSession(DateTime date , long payment = Q.SumLevel2) => new ()
        {
            //_id = ObjectId.GenerateNewId(),
            operation = new MongoOperation()
            {
                calculatedPayment = payment
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
                clientNodeId = Q.PersonId1
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
    protected T GetService<T>() where T : notnull => scope.GetRequiredService<T>();
    protected IServiceScope CreateScope() => server.Services.CreateScope();
    protected readonly PostgresDbContext postgres;
    protected readonly MongoDbContext mongo;
    protected IBackgroundJobClientV2 jobClient;
    protected readonly HangfireDbContext hangfireDb;

    public BonusTestApi(FakeApplicationFactory<Program> server)
    {
        this.server = server;
        //InitDatabases(server);
        A.CallTo(() => this.server.DateTimeService.GetNowUtc()).
            ReturnsNextFromSequence(Q.DateTimeSequence);

        scope = CreateScope();
        postgres = scope.GetRequiredService<PostgresDbContext>();
        postgres.Database.EnsureDeleted();
        postgres.Database.Migrate();
        BonusProgramSeed.AddPostgresSeed(server.Services);

        //InfraHelper.CreateMongoDatabase(this.server.DbName).GetAwaiter().GetResult();
        mongo = scope.GetRequiredService<MongoDbContext>();
        mongo.Database.DropCollection(MongoDbContext.SessionCollection);
        jobClient = scope.GetRequiredService<IBackgroundJobClientV2>();

        hangfireDb = scope.GetRequiredService<HangfireDbContext>();
        hangfireDb.Database.Migrate();


    }

    protected void InitDatabases(FakeApplicationFactory<Program> server)
    {
        InfraHelper.RunPostgresContainer();
        InfraHelper.RunMongo(this.server.DbName);
    }

    public async Task InitializeAsync()
    {

    }
    async Task IAsyncLifetime.DisposeAsync()
    {
        await postgres.Database.EnsureDeletedAsync();
        var hangfire = scope.ServiceProvider.GetRequiredService<HangfireDbContext>();
        await hangfire.Database.EnsureDeletedAsync();
        await mongo.Database.DropCollectionAsync(MongoDbContext.SessionCollection);
        InfraHelper.DropMongoDatabase(server.DbName);
        scope.Dispose();
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
