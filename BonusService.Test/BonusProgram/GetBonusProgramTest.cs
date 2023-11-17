using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;


public class BonusProgramTest : BonusTestApi
{
    public BonusProgramTest(FakeApplicationFactory<Program> server) : base(server)
    {
    }

    [Fact]
    public async Task GetBonusProgram()
    {
        var programs = await api.ApiBonusProgramGetAllAsync();
        programs.Should().NotBeNullOrEmpty();
    }

}
