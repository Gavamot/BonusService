using BonusApi;
using BonusService.Test.Common;
namespace BonusService.Test;

public class UnitTest1 : BonusTestApi
{
    public UnitTest1(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    //readonly AccrualManualDto dto = new AccrualManualDto()
    /*[Fact]
    public Task AccrualManual_Put100Bonus_BalanceIs100()
    {
        api.ApiAccrualManualAsync(new AccrualManualDto(){});
    }*/
}
