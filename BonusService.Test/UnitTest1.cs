
using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;

public class BonusProgramTest : BonusTestApi
{
    public BonusProgramTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    /*
    private readonly AccrualManualDto dto = new()
    {
        PersonId = Guid.Parse("6B29FC40-CA47-1067-B31D-00DD010662DA"),
        BankId = 1,
        UserId = Guid.Parse("6B29FC40-CA47-1067-B31D-00DD010662DA"),
        TransactionId = "manual_6B29FC40-CA47-1067-B31D-00DD010662DA",
        Sum = 100,
        Description = "На чай"
    };
    */

    [Fact]
    public async Task GetBonusProgram()
    {
        var programs = await api.ApiBonusProgramAsync();
        programs.Should().NotBeNullOrEmpty();
    }

}
