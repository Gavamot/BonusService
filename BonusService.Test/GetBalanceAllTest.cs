using BonusService.Test.Common;
using FluentAssertions;
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
    public async Task Sum1000OneOneBalance_ReturnListWithOneBalanceSum1000()
    {
        await InitialBalanceTran1Person1BankRub(Q.Sum1000);
        var balances =  await api.ApiBalanceGetAllAsync(Q.PersonId1);
        balances.Items.Count.Should().Be(1);
        var balance = balances.Items.First();
        balance.Sum.Should().Be(Q.Sum1000);
        balance.BankId.Should().Be(Q.BankIdRub);
    }
}
