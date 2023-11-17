using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using Meziantou.Xunit;
namespace BonusService.Test;

public class GetBalanceAllTest : BonusTestApi
{
    public GetBalanceAllTest() : base(new FakeApplicationFactory<Program>())
    {
    }

    [Fact]
    public async Task EmptyBonuses_ReturnEmptyList()
    {
        var balance =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balance.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task OneTransactionOneBalance_ReturnListWithOneBalance()
    {
        await InitTransactionTran1Person1BankRub(Q.Sum1000);
        var balances =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balances.Items.Count.Should().Be(1);
        var balance = balances.Items.First();
        balance.Sum.Should().Be(Q.Sum1000);
        balance.BankId.Should().Be(Q.BankIdRub);
    }

    [Fact]
    public async Task ManyTransactionOneBalance_ReturnListWithOneBalanceWithSumOfTransactions()
    {
        await postgres.Transactions.AddRangeAsync(new Transaction()
        {
            BonusSum = Q.Sum1000,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.GetRandomTransactionId(),
            Type = TransactionType.Manual,
        },new Transaction()
        {
            BonusSum = Q.Sum500,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.GetRandomTransactionId(),
            Type = TransactionType.Manual,
        }
        ,new Transaction()
        {
            BonusSum = Q.Sum2000,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.GetRandomTransactionId(),
            Type = TransactionType.Manual,
        });
        await postgres.SaveChangesAsync();

        var balances =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balances.Items.Count.Should().Be(1);
        var balance = balances.Items.First();
        balance.Sum.Should().Be(Q.Sum500 + Q.Sum1000 + Q.Sum2000);
        balance.BankId.Should().Be(Q.BankIdRub);
    }

    [Fact]
    public async Task ManyTransactionManuBalancesOnlyPositive_ReturnListWithOneBalancesWithSumOfTheyTransactions()
    {
        await postgres.Transactions.AddRangeAsync(new Transaction()
            {
                BonusSum = Q.Sum1000,
                BankId = Q.BankIdRub,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            },new Transaction()
            {
                BonusSum = Q.Sum500,
                BankId = Q.BankIdRub,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            }
            ,new Transaction()
            {
                BonusSum = Q.Sum2000,
                BankId = Q.BankIdKaz,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            });
        await postgres.SaveChangesAsync();

        var balances =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balances.Items.Count.Should().Be(2);
        var balanceRus = balances.Items.FirstOrDefault(x=>x.BankId == Q.BankIdRub);
        balanceRus.Should().NotBeNull();
        balanceRus.Sum.Should().Be(Q.Sum500 + Q.Sum1000);

        var balanceKaz = balances.Items.FirstOrDefault(x=>x.BankId == Q.BankIdKaz);
        balanceKaz.Should().NotBeNull();
        balanceKaz.Sum.Should().Be(Q.Sum2000);
    }

    [Fact]
    public async Task ManyTransactionManuBalances_ReturnListWithOneBalancesWithSumOfTheyTransactions()
    {
        await postgres.Transactions.AddRangeAsync(new Transaction()
            {
                BonusSum = Q.Sum1000,
                BankId = Q.BankIdRub,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            },new Transaction()
            {
                BonusSum = Q.Sum500 *-1,
                BankId = Q.BankIdRub,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            }
            ,new Transaction()
            {
                BonusSum = Q.Sum2000,
                BankId = Q.BankIdKaz,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            },
            new Transaction()
            {
                BonusSum = Q.Sum1000*-1,
                BankId = Q.BankIdKaz,
                PersonId = Q.PersonId1,
                TransactionId = Q.GetRandomTransactionId(),
                Type = TransactionType.Manual,
            });
        await postgres.SaveChangesAsync();

        var balances =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balances.Items.Count.Should().Be(2);
        var balanceRus = balances.Items.FirstOrDefault(x=>x.BankId == Q.BankIdRub);
        balanceRus.Should().NotBeNull();
        balanceRus.Sum.Should().Be(Q.Sum1000 - Q.Sum500);

        var balanceKaz = balances.Items.FirstOrDefault(x=>x.BankId == Q.BankIdKaz);
        balanceKaz.Should().NotBeNull();
        balanceKaz.Sum.Should().Be(Q.Sum2000 - Q.Sum1000);
    }
}
