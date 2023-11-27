using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Test.Common;
using FluentAssertions;
using Hangfire;
using BonusProgram = BonusService.Postgres.BonusProgram;
namespace BonusService.Test;

public static class BackgroundJobClientExt
{
    public static void WaitForEndOfJob(this IBackgroundJobClientV2 client, string jobId)
    {
        while (client.IsJobNotEnded(jobId))
        {
            Thread.Sleep(1);
        }
    }
}

public class MonthlySumBonusJobTest : BonusTestApi
{
    private BonusProgram bonusProgram = new BonusProgramRep().Get();
    private string jobId => $"bonusProgram_{bonusProgram.Id}";
    public MonthlySumBonusJobTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public void EmptySessions_EmptyResultHistoryAdded()
    {
        var jobId = jobClient.Enqueue<MonthlySumBonusJob>(x=> x.ExecuteAsync(bonusProgram));
        jobClient.WaitForEndOfJob(jobId);
        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        bonusProgramHistory.BonusProgramId.Should().Be(bonusProgram.Id);
        bonusProgramHistory.BankId.Should().Be(bonusProgram.BankId);
        bonusProgramHistory.ClientBalancesCount.Should().Be(0);
        bonusProgramHistory.TotalBonusSum.Should().Be(0);
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);

        postgres.Transactions.Any().Should().BeFalse();
    }

    [Fact]
    public void EmptyCalculationForPeriod_EmptyResultHistoryAdded()
    {
        var session = Q.CreateSession(new DateTime(1990, 1, 1));
        session.status = 44;
        var session2 = Q.CreateSession(new DateTime(1990, 1, 1));
        session2.tariff.BankId = Q.BankIdKaz;

        var session3 = Q.CreateSession(new DateTime(1990, 1, 1));
        session3.tariff.BankId = Q.BankIdKaz;
        session3.user.clientNodeId = Q.PersonId2String;

        var session4 = Q.CreateSession(new DateTime(1990, 1, 1));
        session3.user.chargingClientType = MongoChargingClientType.CompanyEntity;

        mongo.Sessions.InsertMany(new []
        {
            Q.CreateSession(new DateTime(1990, 1, 1)),
            Q.CreateSession(new DateTime(2090, 1, 1)),
            session,
            session2,
            session3,
            session4
        });
        var jobId = jobClient.Enqueue<MonthlySumBonusJob>(x=> x.ExecuteAsync(bonusProgram));
        jobClient.WaitForEndOfJob(jobId);
        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        bonusProgramHistory.BonusProgramId.Should().Be(bonusProgram.Id);
        bonusProgramHistory.BankId.Should().Be(bonusProgram.BankId);
        bonusProgramHistory.ClientBalancesCount.Should().Be(0);
        bonusProgramHistory.TotalBonusSum.Should().Be(0);
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);

        postgres.Transactions.Any().Should().BeFalse();
    }


}
