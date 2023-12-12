using BonusApi;
using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using TransactionType = BonusService.Common.Postgres.Entity.TransactionType;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace BonusService.Test;

public class ManualAccrualTest : BonusTestApi
{
    public ManualAccrualTest(FakeApplicationFactory<Program> server) : base(server)
    {

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
        };
        await api.BalanceAccrualManualAsync(request);
        var transaction = await postgres.Transactions.SingleAsync();
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserName.Should().Be(Q.UserName);
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
        };
        await api.BalanceAccrualManualAsync(request);

        request = new AccrualManualRequestDto()
        {
            Description = Q.Description2,
            BonusSum = sum + 1,
            BankId = Q.BankIdKaz,
            PersonId = Q.PersonId2,
            TransactionId = Q.TransactionId1,
        };

        await api.BalanceAccrualManualAsync(request);

        var transaction = await postgres.Transactions.SingleAsync();
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserName.Should().Be(Q.UserName);
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
        };

        await api.BalanceAccrualManualAsync(request);

        long sum2 = 1111L;
        var request2 = new AccrualManualRequestDto()
        {
            Description = Q.Description2,
            BonusSum = sum2,
            BankId = Q.BankIdKaz,
            PersonId = Q.PersonId2,
            TransactionId = Q.TransactionId2,
        };

        await api.BalanceAccrualManualAsync(request2);

        var transactions = await postgres.Transactions.ToArrayAsync();
        transactions.Should().HaveCount(2);

        var transaction = transactions.Single(x => x.TransactionId == Q.TransactionId1);
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserName.Should().Be(Q.UserName);
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
        transaction.UserName.Should().Be(Q.UserName);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.Skip(1).First());
    }

    [Fact]
    public async Task WrongParameters_TrowsException()
    {
        async Task AccrualManualAsyncTrows(AccrualManualRequestDto request)
        {
            Func<Task> t = async () => await api.BalanceAccrualManualAsync(request);
            await t.Should().ThrowAsync<Exception>();
        }

        var request = new AccrualManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = 1000L,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
        };
        request = request.ToJsonClone();
        request.Description = "";
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.Description = null;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.Description = " "; // Tab
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.Description = "  "; // Spaces
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.BonusSum = 0;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.BonusSum = -1;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.BankId = 0;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.BankId = -1;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.PersonId = default;
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.TransactionId = "";
        await AccrualManualAsyncTrows(request);

        request = request.ToJsonClone();
        request.TransactionId = null;
        await AccrualManualAsyncTrows(request);
    }
}
