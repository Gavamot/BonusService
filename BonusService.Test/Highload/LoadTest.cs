using AutoBogus;
using BonusApi;
using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Test.Common;
using FluentAssertions;
using BonusProgram = BonusService.Postgres.BonusProgram;
namespace BonusService.Test;

public sealed class MongoSessionFaker : AutoFaker<MongoSession>
{
    public static DateTime from = BonusTestApi.Q.IntervalMoth1.from.UtcDateTime - TimeSpan.FromDays(100);
    public static DateTime to = BonusTestApi.Q.IntervalMoth1.from.UtcDateTime + TimeSpan.FromDays(100);
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



public class LoadTest : BonusTestApi
{
    public LoadTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    private BonusProgram bonusProgram = new BonusProgramRep().Get();

    [Fact]
    public void AddManySessions()
    {
        var faker = new MongoSessionFaker();
        var sessions = faker.GenerateLazy(6_000_000);
        foreach (var chunk in sessions.Chunk(300_000))
        {
            mongo.Sessions.InsertMany(chunk);
        }

        var job = GetService<MonthlySumBonusJob>();
        job.ExecuteAsync(bonusProgram).GetAwaiter().GetResult();

        var res = postgres.Transactions.Count();
        res.Should().BeGreaterThan(0);
    }
}
