using BonusApi;
using BonusService.Bonuses;
using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;

public class BonusProgramAchievementTest : BonusTestApi
{
    public BonusProgramAchievementTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task ZeroAchievement_ReturnLevels0and1()
    {
        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        bonusPrograms.Items.Count.Should().Be(1);
        var item = bonusPrograms.Items.First();
        item.CurrentSum.Should().Be(0);

        var program = new BonusProgramRep().Get();
        var curLevel = program.ProgramLevels.First();
        var nextLevel = program.ProgramLevels.Skip(1).First();

        item.BonusProgramId.Should().Be(program.Id);
        item.BonusProgramName.Should().Be(program.Name);

        item.LevelCondition.Should().Be(curLevel.Condition);
        item.LevelName.Should().Be(curLevel.Name);
        item.Type.Should().Be((BonusProgramType)program.BonusProgramType);
        item.LevelAwardSum.Should().Be(curLevel.AwardSum);
        item.LevelAwardPercent.Should().Be(curLevel.AwardPercent);

        item.NextLevelCondition.Should().Be(nextLevel.Condition);
        item.NextLevelName.Should().Be(nextLevel.Name);
        item.NextLevelAwardPercent.Should().Be(nextLevel.AwardPercent);
        item.NextLevelAwardSum.Should().Be(nextLevel.AwardSum);
    }
}
