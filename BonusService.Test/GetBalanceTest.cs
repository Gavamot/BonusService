using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;

public class GetBalanceTest : BonusTestApi
{
    public GetBalanceTest(FakeApplicationFactory<Program> server) : base(server)
    {
    }

    [Fact]
    public async Task EmptyBonuses_ReturnEmptyList()
    {
        var balance =  await api.ApiBalanceGetAsync(Q.PersonId1, Q.BankIdRub);
        balance.Should().Be(0);
    }

    [Fact]
    public async Task OneTransactionOneBalance_ReturnListWithOneBalance()
    {
        await InitTransactionTran1Person1BankRub(Q.Sum1000);
        var balances =  await api.ApiBalanceGetAsync(Q.PersonId1, Q.BankIdRub);
        balances.Should().Be(Q.Sum1000);
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

        var balance =  await api.ApiBalanceGetAsync(Q.PersonId1, Q.BankIdRub);
        balance.Should().Be(Q.Sum500 + Q.Sum1000 + Q.Sum2000);
    }
}
