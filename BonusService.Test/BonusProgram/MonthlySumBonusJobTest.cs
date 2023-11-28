using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Postgres;
using BonusService.Test.Common;
using FakeItEasy;
using FluentAssertions;
using Hangfire;
using BonusProgram = BonusService.Postgres.BonusProgram;
using BonusProgramHistory = BonusService.Postgres.BonusProgramHistory;
namespace BonusService.Test;

public static class BackgroundJobClientExt
{
    public static void WaitForEndOfJob(this IBackgroundJobClientV2 client, string jobId)
    {
        Thread.Sleep(50);
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
    public MonthlySumBonusJobTest(FakeApplicationFactory<Program> server) : base(new FakeApplicationFactory<Program>())
    {

    }

    private void CheckEmptyHistory(BonusProgramHistory bonusProgramHistory)
    {
        bonusProgramHistory.BonusProgramId.Should().Be(bonusProgram.Id);
        bonusProgramHistory.BankId.Should().Be(bonusProgram.BankId);
        bonusProgramHistory.ClientBalancesCount.Should().Be(0);
        bonusProgramHistory.TotalBonusSum.Should().Be(0);
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);
    }
    private void CheckEmptyHistory(IEnumerable<BonusProgramHistory> bonusProgramHistories)
    {
        foreach (var bonusProgramHistory in bonusProgramHistories)
        {
            CheckEmptyHistory(bonusProgramHistory);
        }
    }
    private void AddUnmatchedSessions()
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

        var session5 = Q.CreateSession(Q.IntervalMoth1.to.UtcDateTime);

        mongo.Sessions.InsertMany(new []
        {
            Q.CreateSession(new DateTime(1990, 1, 1)),
            Q.CreateSession(new DateTime(2090, 1, 1)),
            session,
            session2,
            session3,
            session4,
            session5
        });
    }

    [Fact]
    public async Task EmptySessions_EmptyResultHistoryAdded()
    {
        var job = GetService<MonthlySumBonusJob>();
        await job.ExecuteAsync(bonusProgram);
        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task EmptyCalculationForPeriod_EmptyResultHistoryAdded()
    {
        AddUnmatchedSessions();
        var job = GetService<MonthlySumBonusJob>();
        await job.ExecuteAsync(bonusProgram);
        postgres.Transactions.Any().Should().BeFalse();

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task EmptyCalculationForPeriodTwiceExecution_EmptyResult2HistoryAdded()
    {
        A.CallTo(()=> this.server.DateTimeService.GetCurrentMonth()).ReturnsNextFromSequence(new []
        {
            Q.IntervalMoth1,
            Q.IntervalMoth1,
        });
        AddUnmatchedSessions();
        var job = GetService<MonthlySumBonusJob>();
        await job.ExecuteAsync(bonusProgram);

        var job2 = GetService<MonthlySumBonusJob>();
        await job2.ExecuteAsync(bonusProgram);

        postgres.Transactions.Any().Should().BeFalse();

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(2);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task OnePerson2PayInDifferentTimeByDifferentSum_CalculatedCorrectly()
    {
        AddUnmatchedSessions();

        MongoSession user1RusAccountSession1 = Q.CreateSession(Q.IntervalMoth1.from.UtcDateTime);
        MongoSession user1RusAccountSession2 = Q.CreateSession((Q.IntervalMoth1.from + TimeSpan.FromDays(27)).UtcDateTime);
        user1RusAccountSession2.operation.calculatedPayment = Q.Sum2000;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1,
            user1RusAccountSession2
        });

        var job = GetService<MonthlySumBonusJob>();
        await job.ExecuteAsync(bonusProgram);

        // 2 PaymentCalc
        postgres.Transactions.Count().Should().Be(1);
        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        tranUser1Rus.Type.Should().Be(TransactionType.Auto);
        tranUser1Rus.Description.Should().NotBeEmpty();
        tranUser1Rus.BonusProgramId.Should().Be(1);
        long bonusBaseSum = user1RusAccountSession1.operation.calculatedPayment!.Value + user1RusAccountSession2.operation.calculatedPayment.Value;
        tranUser1Rus.BonusBase.Should().Be(bonusBaseSum);
        long bonusSumByLevel2 = bonusBaseSum * 5 / 100;
        tranUser1Rus.BonusSum.Should().Be(bonusSumByLevel2);
        tranUser1Rus.UserName.Should().Be(user1RusAccountSession1.user.clientLogin);
        tranUser1Rus.OwnerId.Should().BeNull();
        tranUser1Rus.EzsId.Should().BeNull();
        tranUser1Rus.TransactionId.Should().NotBeEmpty();
        tranUser1Rus.LastUpdated.Should().Be(Q.DateTimeSequence.First());

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);
        bonusProgramHistory.BankId.Should().Be(1);
        bonusProgramHistory.BonusProgramId.Should().Be(1);
        bonusProgramHistory.ClientBalancesCount.Should().Be(1);
        bonusProgramHistory.TotalBonusSum.Should().Be(bonusSumByLevel2);
    }


    [Fact]
    public async Task SessionFromDifferentBunk_OnlyBonusProgramBankCalculated()
    {
        AddUnmatchedSessions();

        MongoSession user1RusAccountSession1 = Q.CreateSession(Q.IntervalMoth1.from.UtcDateTime);
        MongoSession user1RusAccountSession2 = Q.CreateSession(Q.IntervalMoth1.from.UtcDateTime);
        user1RusAccountSession2.tariff.BankId = Q.BankIdKaz;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1,
            user1RusAccountSession2
        });

        var job = GetService<MonthlySumBonusJob>();
        await job.ExecuteAsync(bonusProgram);

        postgres.Transactions.Count().Should().Be(1);

        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        tranUser1Rus.Type.Should().Be(TransactionType.Auto);
        tranUser1Rus.Description.Should().NotBeEmpty();
        tranUser1Rus.BonusProgramId.Should().Be(1);
        tranUser1Rus.BonusBase.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser1Rus.BonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value  * 5 / 100);
        tranUser1Rus.UserName.Should().Be(user1RusAccountSession1.user.clientLogin);
        tranUser1Rus.OwnerId.Should().BeNull();
        tranUser1Rus.EzsId.Should().BeNull();
        tranUser1Rus.TransactionId.Should().NotBeEmpty();
        tranUser1Rus.LastUpdated.Should().Be(Q.DateTimeSequence.First());


        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);
        bonusProgramHistory.BankId.Should().Be(1);
        bonusProgramHistory.BonusProgramId.Should().Be(1);
        bonusProgramHistory.ClientBalancesCount.Should().Be(1);
        bonusProgramHistory.TotalBonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value * 5 / 100);
    }

    [Fact]
    public void RealTest_TwoPersonsPayed_CalculatedCorrectly()
    {
        AddUnmatchedSessions();

        MongoSession user1RusAccountSession1 = Q.CreateSession(Q.IntervalMoth1.from.UtcDateTime);
        MongoSession user2RusAccountSession1 = Q.CreateSession(Q.IntervalMoth1.from.UtcDateTime);
        user2RusAccountSession1.user.clientLogin = Q.ClientLogin2;
        user2RusAccountSession1.user.clientNodeId = Q.PersonId2String;
        user2RusAccountSession1.operation.calculatedPayment = Q.SumLevel1;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1,
            user2RusAccountSession1
        });

        var jobId = jobClient.Enqueue<MonthlySumBonusJob>(x=> x.ExecuteAsync(bonusProgram));
        jobClient.WaitForEndOfJob(jobId);

        postgres.Transactions.Count().Should().Be(2);

        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        tranUser1Rus.Type.Should().Be(TransactionType.Auto);
        tranUser1Rus.Description.Should().NotBeEmpty();
        tranUser1Rus.BonusProgramId.Should().Be(1);
        tranUser1Rus.BonusBase.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser1Rus.BonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value  * 5 / 100);
        tranUser1Rus.UserName.Should().Be(user1RusAccountSession1.user.clientLogin);
        tranUser1Rus.OwnerId.Should().BeNull();
        tranUser1Rus.EzsId.Should().BeNull();
        tranUser1Rus.TransactionId.Should().NotBeEmpty();
        tranUser1Rus.LastUpdated.Should().Be(Q.DateTimeSequence.First());

        var tranUser2Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId2 && x.BankId == Q.BankIdRub);
        tranUser2Rus.Type.Should().Be(TransactionType.Auto);
        tranUser2Rus.Description.Should().NotBeEmpty();
        tranUser2Rus.BonusProgramId.Should().Be(1);
        tranUser2Rus.BonusBase.Should().Be(user2RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser2Rus.BonusSum.Should().Be(user2RusAccountSession1.operation.calculatedPayment!.Value  * 1 / 100);
        tranUser2Rus.UserName.Should().Be(user2RusAccountSession1.user.clientLogin);
        tranUser2Rus.OwnerId.Should().BeNull();
        tranUser2Rus.EzsId.Should().BeNull();
        tranUser2Rus.TransactionId.Should().NotBeEmpty();
        tranUser2Rus.LastUpdated.Should().Be(Q.DateTimeSequence.First());

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        bonusProgramHistory.ExecTimeStart.Should().Be(Q.IntervalMoth1.from);
        bonusProgramHistory.ExecTimeEnd.Should().Be(Q.IntervalMoth1.to);
        bonusProgramHistory.BankId.Should().Be(Q.BankIdRub);
        bonusProgramHistory.BonusProgramId.Should().Be(1);
        bonusProgramHistory.ClientBalancesCount.Should().Be(2);
        bonusProgramHistory.TotalBonusSum.Should().Be(
            (user1RusAccountSession1.operation.calculatedPayment!.Value * 5 / 100) +
            (user2RusAccountSession1.operation.calculatedPayment!.Value * 1 / 100));

    }
}
