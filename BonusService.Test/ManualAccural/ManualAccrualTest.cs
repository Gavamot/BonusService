using BonusApi;
using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Test;

//[DisableParallelization]
public class ManualAccrualTest : BonusTestApi
{
    public ManualAccrualTest(FakeApplicationFactory<Program> server) : base(new FakeApplicationFactory<Program>())
    {
        /*using var scope = Server.Services.CreateScope();
        var postgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        postgres.Transactions.BulkDelete(postgres.Transactions.ToArray());
        A.CallTo(() => Server.DateTimeService.GetNowUtc()).ReturnsNextFromSequence(BonusTestApi.Q.DateTimeSequence);*/
    }

    [Fact]
    public async Task AccrualBonusOnce_OneBonusAddedToDatabase()
    {
        long sum = 1000L;
        var request = new AccrualManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = sum,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            UserId = Q.UserId1
        };
        await api.ApiAccrualManualAsync(request);
        using var scope = CreateScope();
        var postgres = scope.GetRequiredService<PostgresDbContext>();
        var transaction = await postgres.Transactions.SingleAsync();
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserId.Should().Be(Q.UserId1);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.First());
    }

    [Fact]
    public async Task AccrualTwoDifferentBonusWithSameTransactionIdTwice_OnlyFirstWasAddedToDatabase()
    {
        long sum = 1000L;
        var request = new AccrualManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = sum,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            UserId = Q.UserId1
        };
        await api.ApiAccrualManualAsync(request);

        request = new AccrualManualRequestDto()
        {
            Description = Q.Description2,
            BonusSum = sum + 1,
            BankId = Q.BankIdKaz,
            PersonId = Q.PersonId2,
            TransactionId = Q.TransactionId1,
            UserId = Q.UserId2
        };

        await api.ApiAccrualManualAsync(request);

        using var scope = CreateScope();
        var postgres = scope.GetRequiredService<PostgresDbContext>();
        var transaction = await postgres.Transactions.SingleAsync();
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserId.Should().Be(Q.UserId1);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.First());
    }

    [Fact]
    public async Task AccrualTwoDifferentBonus_TwoAddedToDatabase()
    {
        long sum = 1000L;
        var request = new AccrualManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = sum,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            UserId = Q.UserId1
        };

        await api.ApiAccrualManualAsync(request);

        long sum2 = 1111L;
        var request2 = new AccrualManualRequestDto()
        {
            Description = Q.Description2,
            BonusSum = sum2,
            BankId = Q.BankIdKaz,
            PersonId = Q.PersonId2,
            TransactionId = Q.TransactionId2,
            UserId = Q.UserId2
        };

        await api.ApiAccrualManualAsync(request2);

        using var scope = CreateScope();
        var postgres = scope.GetRequiredService<PostgresDbContext>();

        var transactions = await postgres.Transactions.ToArrayAsync();
        transactions.Should().HaveCount(2);

        var transaction = transactions.Single(x => x.TransactionId == Q.TransactionId1);
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserId.Should().Be(Q.UserId1);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.First());


        transaction = transactions.Single(x => x.TransactionId == Q.TransactionId2);
        transaction.TransactionId.Should().Be(Q.TransactionId2);
        transaction.Description.Should().Be(Q.Description2);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdKaz);
        transaction.BonusSum.Should().Be(sum2);
        transaction.UserId.Should().Be(Q.UserId2);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.Skip(1).First());
    }
}
