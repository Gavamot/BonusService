using BonusApi;
using BonusService.Test.Common;
namespace BonusService.Test;

public class UnitTest1 : BonusTestApi
{
    public UnitTest1(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    private readonly AccrualManualDto dto = new()
    {
        PersonId = Guid.Parse("6B29FC40-CA47-1067-B31D-00DD010662DA"),
        BankId = 1,
        UserId = Guid.Parse("6B29FC40-CA47-1067-B31D-00DD010662DA"),
        TransactionId = Guid.Parse("6B29FC40-CA47-1067-B31D-00DD010662DA"),
        Sum = 100,
        Description = "На чай"
    };

    [Fact]
    public async Task AccrualManual_Put100Bonus_BalanceIs100()
    {
        await api.ApiAccrualManualAsync(dto);
    }

    [Fact]
    public async Task AccrualManual_Negative()
    {
        await api.ApiAccrualManualAsync(dto);
    }
}
