using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test.BonusPrograms;

public class BonusProgramTest : BonusTestApi
{
    public BonusProgramTest(FakeApplicationFactory<Program> server) : base(server)
    {
    }

    [Fact]
    public async Task GetAllBonusPrograms_GetNotEmptyArray()
    {
        var programs = await api.BonusProgramGetAllAsync();
        programs.Should().NotBeNullOrEmpty();
    }
}
