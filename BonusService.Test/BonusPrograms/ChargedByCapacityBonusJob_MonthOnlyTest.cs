using BonusService.BonusPrograms.ChargedByCapacityBonus;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
namespace BonusService.Test.BonusPrograms;

#pragma warning disable CS8625, CS8602
public class ChargedByCapacityBonusJob_MonthOnlyTest : BonusTestApi
{
    private BonusProgram bonusProgram;
    private string jobId => $"bonusProgram_{bonusProgram.Id}";
    private const int BonusProgramId = 2;
    public ChargedByCapacityBonusJob_MonthOnlyTest(FakeApplicationFactory<Program> server) : base(server)
    {
        bonusProgram = BonusProgramSeed.Get();
        bonusProgram.Id = BonusProgramId;
        bonusProgram.BonusProgramType = BonusProgramType.ChargedByCapacity;
        postgres.BonusPrograms.Add(bonusProgram);
        postgres.SaveChanges();
    }

    private readonly DateTimeOffset bonusIntervalStart = new (2023, 11, 1, 0, 0, 0, new TimeSpan(0));
    private readonly DateTimeOffset bonusIntervalEnd = new (2023, 12, 1, 0, 0, 0, new TimeSpan(0));
    private readonly DateTimeOffset dateOfJobExecution = new(2023, 12, 2, 9, 0, 0, new TimeSpan(0));

    private void CheckEmptyHistory(BonusProgramHistory bonusProgramHistory)
    {
        bonusProgramHistory.ClientBalancesCount.Should().Be(0);
        bonusProgramHistory.TotalBonusSum.Should().Be(0);
        ValidateBonusProgramHistoryCommonFields(bonusProgramHistory);
    }

    private void AddNoiseSession()
    {
        var session = Q.CreateSession(new DateTime(1990, 1, 1));
        session.status = 44;

        var session2 = Q.CreateSession(new DateTime(1990, 1, 1));
        session2.tariff.BankId = Q.BankIdKaz;

        var session3 = Q.CreateSession(new DateTime(1990, 1, 1));
        session3.tariff.BankId = Q.BankIdKaz;
        session3.user.clientNodeId = Q.PersonId2;

        var session4 = Q.CreateSession(new DateTime(1990, 1, 1));
        session3.user.chargingClientType = MongoChargingClientType.CompanyEntity;

        var session5 = Q.CreateSession(Q.TimeExtMoth1.to.UtcDateTime);

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

    private void ValidateTransactionCommonFields(Transaction transaction, string clientLogin = Q.ClientLogin)
    {
        transaction.Type.Should().Be(TransactionType.Auto);
        transaction.Description.Should().NotBeEmpty();
        transaction.BonusProgramId.Should().Be(BonusProgramId);
        transaction.OwnerId.Should().BeNull();
        transaction.EzsId.Should().BeNull();
        transaction.TransactionId.Should().NotBeEmpty();
        transaction.UserName.Should().Be(clientLogin);
        transaction.LastUpdated.Should().NotBe(default);
    }

    private void ValidateBonusProgramHistoryCommonFields(BonusProgramHistory history)
    {
        history.ExecTimeStart.Should().Be(bonusIntervalStart);
        history.ExecTimeEnd.Should().Be(bonusIntervalEnd);
        history.BankId.Should().Be(1);
        history.BonusProgramId.Should().Be(BonusProgramId);
        history.LastUpdated.Should().NotBe(default);
    }

    private void CheckEmptyHistory(IEnumerable<BonusProgramHistory> bonusProgramHistories)
    {
        foreach (var bonusProgramHistory in bonusProgramHistories)
        {
            CheckEmptyHistory(bonusProgramHistory);
        }
    }

    [Fact]
    public async Task EmptySessions_EmptyResultHistoryAdded()
    {
        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);
        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task EmptyCalculationForPeriod_EmptyResultHistoryAdded()
    {
        AddNoiseSession();
        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);
        postgres.Transactions.Any().Should().BeFalse();

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task EmptyCalculationForPeriodTwiceExecution_EmptyResult2HistoryAdded()
    {
        AddNoiseSession();
        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        var job2 = GetService<ChargedByCapacityBonusJob>();
        await job2.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        postgres.Transactions.Any().Should().BeFalse();

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(2);
        CheckEmptyHistory(bonusProgramHistories);
    }

    [Fact]
    public async Task OnePerson2PayInDifferentTimeByDifferentSum_CalculatedCorrectly()
    {
        //AddUnmatchedSessions();

        MongoSession user1RusAccountSession1 = Q.CreateSession(bonusIntervalStart);
        user1RusAccountSession1.operation.calculatedConsume = Q.SumLevel3;
        MongoSession user1RusAccountSession2 = Q.CreateSession(bonusIntervalStart + TimeSpan.FromDays(27));
        user1RusAccountSession2.operation.calculatedConsume = Q.Sum2000;
        await mongo.Sessions.InsertManyAsync(new []
        {
            user1RusAccountSession1,
            user1RusAccountSession2
        });

        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        using var scope = CreateScope();
        var newPostgres = scope.GetRequiredService<PostgresDbContext>();

        // 2 PaymentCalc
        newPostgres.Transactions.Count().Should().Be(1);

        var tranUser1Rus = newPostgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        long bonusBaseSum = user1RusAccountSession1.operation.calculatedPayment!.Value + user1RusAccountSession2.operation.calculatedPayment.Value;
        tranUser1Rus.BonusBase.Should().Be(bonusBaseSum);
        long bonusSumByLevel2 = bonusBaseSum * 5 / 100;
        tranUser1Rus.BonusSum.Should().Be(bonusSumByLevel2);
        tranUser1Rus.UserName.Should().Be(user1RusAccountSession1.user.clientLogin);
        ValidateTransactionCommonFields(tranUser1Rus);

        var histories = newPostgres.BonusProgramHistory.ToArray();
        histories.Length.Should().Be(1);
        var history = histories.First();
        history.ClientBalancesCount.Should().Be(1);
        history.TotalBonusSum.Should().Be(bonusSumByLevel2);
        ValidateBonusProgramHistoryCommonFields(history);
    }
    [Fact]
    public async Task SessionFromDifferentBunk_OnlyBonusProgramBankCalculated()
    {
        AddNoiseSession();

        MongoSession user1RusAccountSession1 = Q.CreateSession(bonusIntervalStart, Q.SumLevel3);
        user1RusAccountSession1.operation.calculatedConsume = Q.SumLevel2;
        MongoSession user1RusAccountSession2 = Q.CreateSession(bonusIntervalStart ,Q.SumLevel3);
        user1RusAccountSession2.operation.calculatedConsume = Q.SumLevel2;
        user1RusAccountSession2.tariff.BankId = Q.BankIdKaz;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1,
            user1RusAccountSession2
        });

        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        postgres.Transactions.Count().Should().Be(1);

        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        tranUser1Rus.BonusBase.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser1Rus.BonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value  * 1 / 100);
        ValidateTransactionCommonFields(tranUser1Rus);


        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        ValidateBonusProgramHistoryCommonFields(bonusProgramHistory);
        bonusProgramHistory.ClientBalancesCount.Should().Be(1);
        bonusProgramHistory.TotalBonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value * 1 / 100);
    }

    [Fact]
    public async Task SessionsFrom1Person_FirstLevelCalculated()
    {
        AddNoiseSession();

        MongoSession user1RusAccountSession1 = Q.CreateSession(bonusIntervalStart, Q.SumLevel2);
        user1RusAccountSession1.operation.calculatedConsume = Q.SumLevel2;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1
        });

        var job = GetService<ChargedByCapacityBonusJob>();

        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        postgres.Transactions.Count().Should().Be(1);

        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);
        tranUser1Rus.BonusBase.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser1Rus.BonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value  * 1 / 100);
        ValidateTransactionCommonFields(tranUser1Rus);


        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        ValidateBonusProgramHistoryCommonFields(bonusProgramHistory);
        bonusProgramHistory.ClientBalancesCount.Should().Be(1);
        bonusProgramHistory.TotalBonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value * 1 / 100);
    }

    [Fact]
    public async Task TwoPersonsPayed_CalculatedCorrectly()
    {
        AddNoiseSession();

        MongoSession user1RusAccountSession1 = Q.CreateSession(bonusIntervalStart);
        MongoSession user2RusAccountSession1 = Q.CreateSession(bonusIntervalStart);
        user2RusAccountSession1.user.clientLogin = Q.ClientLogin2;
        user2RusAccountSession1.user.clientNodeId = Q.PersonId2;
        user2RusAccountSession1.operation.calculatedPayment = Q.SumLevel2;
        user2RusAccountSession1.operation.calculatedConsume = Q.SumLevel2;
        mongo.Sessions.InsertMany(new []
        {
            user1RusAccountSession1,
            user2RusAccountSession1
        });

        var job = GetService<ChargedByCapacityBonusJob>();
        await job.ExecuteAsync(null, bonusProgram, dateOfJobExecution);

        postgres.Transactions.Count().Should().Be(2);

        var tranUser1Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub);

        tranUser1Rus.BonusBase.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser1Rus.BonusSum.Should().Be(user1RusAccountSession1.operation.calculatedPayment!.Value  * 5 / 100);
        ValidateTransactionCommonFields(tranUser1Rus);

        var tranUser2Rus = postgres.Transactions.First(x => x.PersonId == Q.PersonId2 && x.BankId == Q.BankIdRub);

        tranUser2Rus.BonusBase.Should().Be(user2RusAccountSession1.operation.calculatedPayment!.Value);
        tranUser2Rus.BonusSum.Should().Be(user2RusAccountSession1.operation.calculatedPayment!.Value  * 1 / 100);
        ValidateTransactionCommonFields(tranUser2Rus, user2RusAccountSession1.user.clientLogin);

        var bonusProgramHistories = postgres.BonusProgramHistory.ToArray();
        bonusProgramHistories.Length.Should().Be(1);
        var bonusProgramHistory = bonusProgramHistories.First();
        ValidateBonusProgramHistoryCommonFields(bonusProgramHistory);
        bonusProgramHistory.BonusProgramId.Should().Be(BonusProgramId);
        bonusProgramHistory.ClientBalancesCount.Should().Be(2);
        bonusProgramHistory.TotalBonusSum.Should().Be(
            (user1RusAccountSession1.operation.calculatedPayment!.Value * 5 / 100) +
            (user2RusAccountSession1.operation.calculatedPayment!.Value * 1 / 100));
    }
}
