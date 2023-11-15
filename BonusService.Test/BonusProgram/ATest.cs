using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;

public class ATest : BonusTestApi, IDisposable
{
    public ATest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task GetBonusProgram()
    {
        var programs = await api.ApiBonusProgramAsync();
        programs.Should().NotBeNullOrEmpty();
    }

}
