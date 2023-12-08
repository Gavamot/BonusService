using BonusApi;
using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace BonusService.Test;

public class ManualPayTest : BonusTestApi
{
    public ManualPayTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task PayIfZeroBalance_HasNoRecordsInDatabase()
    {
        long sum = 1000L;
        var request = new PayManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = sum,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
        };

        var res = await api.BalancePayManualAsync(request);
        res.Should().Be(0L);

        var isTransactionExist = await postgres.Transactions.AnyAsync();
        isTransactionExist.Should().BeFalse();
    }

    private async Task<long> InitPayIfBalanceLessThenTransaction(long balance, long pay)
    {
        await InitTransactionTran1Person1BankRub(balance);

        var request = new PayManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = pay,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId2,
        };

        return await api.BalancePayManualAsync(request);
    }

    [Fact]
    public async Task PayIfBalanceLessThenTransaction_AccrualTransactionDoesNotChanged()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum500, Q.Sum1000);

        var transaction = await postgres.Transactions.SingleAsync(x => x.TransactionId == Q.TransactionId1);
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(Q.Sum500);
        transaction.UserName.Should().Be(Q.UserName);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(default);
    }

    [Fact]
    public async Task PayIfBalanceLessThenTransaction_InDatabaseOnly2Transactions()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum500, Q.Sum1000);
        var transactionCount = await postgres.Transactions.CountAsync();
        transactionCount.Should().Be(2);
    }

    [Fact]
    public async Task PayIfBalanceLessThenTransaction_PayOnlyBalanceInDataBaseCorrectTransaction()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum500, Q.Sum1000);
        var transaction = await postgres.Transactions.SingleAsync(x => x.TransactionId == Q.TransactionId2);
        transaction.TransactionId.Should().Be(Q.TransactionId2);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(Q.Sum500 * -1);
        transaction.UserName.Should().Be(Q.UserName);
        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.First());
    }

    [Fact]
    public async Task PayIfBalanceLessThenTransaction_InDataBaseTotalTransactionsSumIsZero()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum500, Q.Sum1000);
        var balance = await postgres.Transactions.SumAsync(x => x.BonusSum);
        balance.Should().Be(0);
    }

    [Fact]
    public async Task PayIfSumOfBalanceTransactionsIsZero_DoNotWriteTransactionInDbAndReturnZero()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum2000, Q.Sum2000);

        var request = new PayManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = 1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId3,
        };

        var res =  await api.BalancePayManualAsync(request);
        res.Should().Be(0);
        var balance = await postgres.Transactions.FirstOrDefaultAsync(x => x.TransactionId == request.TransactionId);
        balance.Should().BeNull();
    }

    [Fact]
    public async Task PayIfBalanceMoreThenTransaction_PayTransactionSumAndTransactionIsAddedToDataBase()
    {
        await InitPayIfBalanceLessThenTransaction(Q.Sum2000, Q.Sum1000);
        var request = new PayManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = Q.Sum1000,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId3,
        };

        var res =  await api.BalancePayManualAsync(request);
        res.Should().Be(Q.Sum1000);
        var transaction = await postgres.Transactions.FirstOrDefaultAsync(x => x.TransactionId == request.TransactionId);
        transaction.Should().NotBeNull();
        transaction.BonusSum.Should().Be(Q.Sum1000 *-1);
        var balance= await postgres.Transactions.SumAsync(x => x.BonusSum);
        balance.Should().Be(0);
    }

    [Fact]
    public async Task WrongParameters_TrowsException()
    {
        async Task BalancePayManualAsyncTrows(PayManualRequestDto request)
        {
            Func<Task> t = async () => await api.BalancePayManualAsync(request);
            await t.Should().ThrowAsync<Exception>();
        }

        var requestOriginal = new PayManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = 1000L,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
        };
        var request = requestOriginal.ToJsonClone();
        request.Description = "";
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = null;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = " "; // Tab
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = "  "; // Spaces
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BonusSum = 0;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BonusSum = -1;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BankId = 0;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BankId = -1;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.PersonId = default;
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.TransactionId = "";
        await BalancePayManualAsyncTrows(request);

        request = requestOriginal.ToJsonClone();
        request.TransactionId = null;
        await BalancePayManualAsyncTrows(request);
    }
}
