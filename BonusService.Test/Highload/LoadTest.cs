using System.Diagnostics.CodeAnalysis;
using AutoBogus;
using BonusService.BonusPrograms;
using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using BonusService.Test.Common;
using FluentAssertions;
using Hangfire.Server;
using BonusProgram = BonusService.Common.Postgres.Entity.BonusProgram;
namespace BonusService.Test;

public sealed class MongoSessionFaker : AutoFaker<MongoSession>
{
    public static DateTime from = BonusTestApi.Q.TimeExtMoth1.from.UtcDateTime - TimeSpan.FromDays(100);
    public static DateTime to = BonusTestApi.Q.TimeExtMoth1.from.UtcDateTime + TimeSpan.FromDays(100);
    public static readonly string [] Users = Enumerable.Range(0, 50_000).Select(x => Guid.NewGuid().ToString("B")).ToArray();
    public static readonly long [] sums = new long [] { 1_00, 100_00, 500_00, 300_00, 700_00, 990_00, 4500_00 };
    public MongoSessionFaker()
    {
        RuleFor(x => x.status, x => x.PickRandom(MongoSessionStatus.GetAll()));
        RuleFor(fake => fake.chargeEndTime, fake => fake.Date.Between(from, to));
        RuleFor(x => x.tariff, x => new MongoTariff() { BankId = x.PickRandom(BonusTestApi.Q.AllBanks) });
        RuleFor(x => x.operation, x => new MongoOperation()
        {
            calculatedPayment = x.PickRandom(sums),
        });
        RuleFor(fake => fake.user, x => new MongoUser()
        {
            clientNodeId = x.PickRandom(Users),
            chargingClientType = x.PickRandom(MongoChargingClientType.GetAll())
        });
    }
}

[SuppressMessage("Usage", "xUnit1031:Do not use blocking task operations in test method")]
public class LoadTest : BonusTestApi
{
    private BonusProgram bonusProgram;
    public LoadTest(FakeApplicationFactory<Program> server) : base(server)
    {
        bonusProgram = postgres.GetBonusProgramById(Q.BonusProgramId1).GetAwaiter().GetResult()!;
    }



    [Fact(Skip = "Long execution не работает надо починить")]
    public void AddManySessions()
    {
        var faker = new MongoSessionFaker();
        var sessions = faker.GenerateLazy(6_000_000);
        foreach (var chunk in sessions.Chunk(300_000))
        {
            mongo.Sessions.InsertMany(chunk);
        }

        var job = GetService<SpendMoneyBonusJob>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        job.ExecuteAsync(null, bonusProgram, Q.TimeExtMoth1.from.UtcDateTime).GetAwaiter().GetResult();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var res = postgres.Transactions.Count();
        res.Should().BeGreaterThan(0);
    }
}
