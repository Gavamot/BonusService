using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;

public class GetBalanceTest : BonusTestApi
{
    public GetBalanceTest() : base(new FakeApplicationFactory<Program>())
    {
    }

    [Fact]
    public async Task EmptyBonuses_ReturnEmptyList()
    {
        var balance =  await api.ApiBalanceGetAsync(Q.PersonId1, Q.BankIdRub);
        balance.Should().Be(0);
    }
}
