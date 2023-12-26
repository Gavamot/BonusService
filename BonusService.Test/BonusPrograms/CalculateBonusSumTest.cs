using BonusService.BonusPrograms;
using BonusService.Common.Postgres.Entity;
using BonusService.Test.Common;
namespace BonusService.Test.BonusPrograms;

public class CalculateBonusSumTest
{

    public BonusProgram bp => BonusProgramSeed.Get();
    private long GetCondition(int level) => bp.ProgramLevels.First(x => x.Level == level).Condition;
    [Fact]
    public void EmptyLevels_ReturnZeroAndNull()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(100, 100, new BonusProgram());
        res.level.Should().BeNull();
        res.sum.Should().Be(0);
    }

    [Fact]
    public void Level1ByLowBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(1), 100, bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(1);
        res.level.AwardPercent.Should().Be(0);
        res.sum.Should().Be(0);
    }

    [Fact]
    public void Level1ByUpperBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(2) -1, 100, bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(1);
        res.level.AwardPercent.Should().Be(0);
        res.sum.Should().Be(0);
    }

    [Fact]
    public void Level2ByLowBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(2), 100 , bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(2);
        res.level.AwardPercent.Should().Be(1);
        res.sum.Should().Be(1);
    }

    [Fact]
    public void Level2ByUpperBorder_ReturnZero()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(3) - 1, 100, bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(2);
        res.level.AwardPercent.Should().Be(1);
        res.sum.Should().Be(1);
    }

    [Fact]
    public void Level3ByLowBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(3), 100 , bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(3);
        res.level.AwardPercent.Should().Be(5);
        res.sum.Should().Be(5);
    }

    [Fact]
    public void Level3ByUpperBorder_ReturnZero()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(4) - 1, 1000, bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(3);
        res.level.AwardPercent.Should().Be(5);
        res.sum.Should().Be(50);
    }

    [Fact]
    public void Level4ByLowBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(4), 1000 , bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(4);
        res.level.AwardPercent.Should().Be(10);
        res.sum.Should().Be(100);
    }

    [Fact]
    public void Level4MoreWhenBorder()
    {
        var res = AccumulateByIntervalBonusJob.CalculateBonusSum(GetCondition(4) + 143555, 1000, bp);
        res.level.Should().NotBeNull();
        res.level!.Level.Should().Be(4);
        res.level.AwardPercent.Should().Be(10);
        res.sum.Should().Be(100);
    }
}
