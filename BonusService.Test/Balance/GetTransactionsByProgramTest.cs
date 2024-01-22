using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace BonusService.Test;

public class GetTransactionsByProgramTest : BonusTestApi
{
    public GetTransactionsByProgramTest(FakeApplicationFactory<Program> server) : base(server)
    {
        var bp2 = BonusProgramSeed.Get();
        bp2.Id = Q.BonusProgramId2;

        postgres.BonusPrograms.Add(bp2);
        postgres.SaveChanges();
    }

    private Transaction GetTransaction(int bonusProgramId, DateTimeOffset date) => new()
    {
        BonusSum = Q.Sum1000,
        BankId = Q.BankIdRub,
        PersonId = Q.PersonId1,
        TransactionId = Guid.NewGuid().ToString("N"),
        Type = TransactionType.Auto,
        Description = "23432424",
        LastUpdated = date,
        EzsId = Guid.NewGuid(),
        BonusProgramId = bonusProgramId,
        OwnerId = 3,
        BonusBase = 4440,
        UserName = "Vasia"
    };

    private Transaction GetTransaction1(DateTimeOffset date) => GetTransaction(Q.BonusProgramId1, date);
    private Transaction GetTransaction2(DateTimeOffset date) => GetTransaction(Q.BonusProgramId2, date);

    private void AddNoiseTransaction()
    {
        postgres.Transactions.AddRange(GetTransaction2(Q.IntervalMoth1Start),
            GetTransaction2(Q.IntervalMoth1Start + TimeSpan.FromDays(1)));
        postgres.SaveChanges();
    }

    [Fact]
    public async Task ZeroTransactions_ReturnCount0AndItems0()
    {
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 10, default, default);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task OnlyAnotherPeronTransaction_ReturnCount0AndItems0()
    {
        AddNoiseTransaction();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 10, default, default);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PersonHasTransactionsInAnotherCurrency_ReturnTransactions()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        t1.BankId = Q.BankIdKaz;
        var t2 = GetTransaction1(Q.IntervalMoth1Start + TimeSpan.FromDays(1));
        t2.BankId = Q.BankIdKaz;
        postgres.Transactions.AddRange(t1, t2);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 10, default, default);
        balance.Count.Should().Be(2);
    }

    [Fact]
    public async Task PersonHasOnlyTransactionsWitchNotInInterval_ReturnCount0AndItems0()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start - TimeSpan.FromHours(1));
        var t2 = GetTransaction1( Q.IntervalMoth1End + TimeSpan.FromHours(1));
        postgres.Transactions.AddRange(t1, t2);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 10, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas2Transactions_ReturnCount2AndItems2()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        var t2 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var items = new [] { t1, t2 };
        postgres.Transactions.AddRange(items);
        await postgres.SaveChangesAsync();
        var a = await postgres.Transactions.ToArrayAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 10, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(2);
        balance.Items.Count.Should().Be(2);
        CheckTransaction(t1, balance.Items.First());
        CheckTransaction(t2, balance.Items.Last());
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageLimitBy1ForksCorrect()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        var t2 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        postgres.Transactions.AddRange(items);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        var actual = new [] { t1 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t1, balance.Items.First());

        balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 2, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t2 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t2, balance.Items.First());

        balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 3, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t3 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t3, balance.Items.First());
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageLimitBy2ForksCorrect()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        var t2 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        postgres.Transactions.AddRange(items);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 2, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(2);
        var actual = new [] { t1, t2 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t1, balance.Items.First());
        CheckTransaction(t2, balance.Items.Last());

        balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 2, 2, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t3 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t3,balance.Items.First());

    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageOverCountReturnCount3Items0()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        var t2 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransaction1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        postgres.Transactions.AddRange(items);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 2, 3, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas1Transaction_CheckAllMappingFields()
    {
        var t1 = GetTransaction1(Q.IntervalMoth1Start);
        var items = new [] { t1};
        postgres.Transactions.AddRange(items);
        await postgres.SaveChangesAsync();
        var balance =  await api.BonusProgramGetTransactionsByProgramAsync(Q.BonusProgramId1, 1, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        var item = balance.Items!.First();
        CheckTransaction(t1, item);
    }

    private void CheckTransaction(BonusService.Common.Postgres.Entity.Transaction t1, BonusApi.Transaction item)
    {
        item.TransactionId.Should().Be(t1.TransactionId);
        item.Description.Should().Be(t1.Description);
        item.BonusSum.Should().Be(t1.BonusSum);
        item.LastUpdated.Should().Be(t1.LastUpdated);
        item.Type.Should().Be((BonusApi.TransactionType)t1.Type);
        item.EzsId.Should().Be(t1.EzsId);
        item.BonusProgramId.Should().Be(t1.BonusProgramId);
        item.OwnerId.Should().Be(t1.OwnerId);
        item.BonusBase.Should().Be(t1.BonusBase);
        item.UserName.Should().Be(t1.UserName);
    }
}
