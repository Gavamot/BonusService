using BonusService.Balance.BalanceTransactions;
using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BonusProgramType = BonusApi.BonusProgramType;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace BonusService.Test;

public class BalanceGetTransactionsTest : BonusTestApi
{
    public BalanceGetTransactionsTest(FakeApplicationFactory<Program> server) : base(server)
    {
    }

    private Transaction GetTransactionPerson(string personId, DateTimeOffset date) => new()
    {
        BonusSum = Q.Sum1000,
        BankId = Q.BankIdRub,
        PersonId = personId,
        TransactionId = Guid.NewGuid().ToString("N"),
        Type = TransactionType.Auto,
        Description = "23432424",
        LastUpdated = date,
        EzsId = Guid.NewGuid(),
        BonusProgramId = 1,
        OwnerId = 3,
        BonusBase = 4440,
        UserName = "Vasia",
        BonusProgram = BonusProgramSeed.Get()
    };

    private Transaction GetTransactionPerson1(DateTimeOffset date) => GetTransactionPerson(Q.PersonId1, date);
    private Transaction GetTransactionPerson2(DateTimeOffset date) => GetTransactionPerson(Q.PersonId2, date);

    private void AddNoiseTransaction()
    {
        Bonus.Transactions.AddRange(GetTransactionPerson2(Q.IntervalMoth1Start),
            GetTransactionPerson2(Q.IntervalMoth1Start + TimeSpan.FromDays(1)));
        Bonus.SaveChanges();
    }

    [Fact]
    public async Task ZeroTransactions_ReturnCount0AndItems0()
    {
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 10, default, default);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task OnlyAnotherPeronTransaction_ReturnCount0AndItems0()
    {
        AddNoiseTransaction();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 10, default, default);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PersonHasOnlyTransactionsInAnotherCurrency_ReturnCount0AndItems0()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        t1.BankId = Q.BankIdKaz;
        var t2 = GetTransactionPerson1(Q.IntervalMoth1Start + TimeSpan.FromDays(1));
        t2.BankId = Q.BankIdKaz;
        Bonus.Transactions.AddRange(t1, t2);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 10, default, default);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PersonHasOnlyTransactionsWitchNotInInterval_ReturnCount0AndItems0()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start - TimeSpan.FromHours(1));
        var t2 = GetTransactionPerson1(Q.IntervalMoth1End + TimeSpan.FromHours(1));
        Bonus.Transactions.AddRange(t1, t2);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 10, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(0);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas2Transactions_ReturnCount2AndItems2()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        var t2 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var items = new [] { t1, t2 };
        Bonus.Transactions.AddRange(items);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 10, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(2);
        balance.Items.Count.Should().Be(2);
        var actual = items;//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t1, balance.Items.First());
        CheckTransaction(t2, balance.Items.Last());
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageLimitBy1ForksCorrect()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        var t2 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        Bonus.Transactions.AddRange(items);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        var actual = new [] { t1 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t1, balance.Items.First());

        balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 2, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t2 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t2, balance.Items.First());

        balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 3, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t3 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t3, balance.Items.First());
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageLimitBy2ForksCorrect()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        var t2 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        Bonus.Transactions.AddRange(items);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 2, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(2);
        var actual = new [] { t1, t2 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t1, balance.Items.First());
        CheckTransaction(t2, balance.Items.Last());

        balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 2, 2, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(1);
        actual = new [] { t3 };//.Select(x => new BalanceTransactionDtoMapper().ToDto(x)).ToArray();
        CheckTransaction(t3,balance.Items.First());

    }

    [Fact]
    public async Task GetTransactionsInIntervalHas3Transactions_PageOverCountReturnCount3Items0()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        var t2 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(1));
        var t3 = GetTransactionPerson1(Q.IntervalMoth1End - TimeSpan.FromHours(4));
        var items = new [] { t1, t2, t3 };
        Bonus.Transactions.AddRange(items);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 2, 3, Q.IntervalMoth1Start, Q.IntervalMoth1End);
        balance.Count.Should().Be(3);
        balance.Items.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTransactionsInIntervalHas1Transaction_CheckAllMappingFields()
    {
        var t1 = GetTransactionPerson1(Q.IntervalMoth1Start);
        var items = new [] { t1};
        Bonus.Transactions.AddRange(items);
        await Bonus.SaveChangesAsync();
        var balance =  await api.BalanceGetTransactionsAsync(Q.PersonId1, Q.BankIdRub, 1, 1, Q.IntervalMoth1Start, Q.IntervalMoth1End);
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
