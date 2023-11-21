using BonusApi;
using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Test.Common;
using FluentAssertions;
namespace BonusService.Test;


public class BonusProgramAchievementTest : BonusTestApi
{
    public BonusProgramAchievementTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task AnotherDateNotCounting_OnlySumCurrentMonth()
    {
        var bonus = new BonusProgramRep().Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime - TimeSpan.FromHours(1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.to.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);
        res.LevelName.Should().Be(curLevel.Name);
        res.LevelCondition.Should().Be(curLevel.Condition);
        res.LevelAwardPercent.Should().Be(curLevel.AwardPercent);
        res.LevelAwardSum.Should().Be(curLevel.AwardSum);

        res.NextLevelName.Should().BeNull();
        res.NextLevelCondition.Should().BeNull();
        res.NextLevelAwardPercent.Should().BeNull();
        res.NextLevelAwardSum.Should().BeNull();
    }

    [Fact]
    public async Task GetLastLevelSumMoreWhenNeed_WorksCorrectly()
    {
        var bonus = new BonusProgramRep().Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        var sum = curLevel.Condition + 1000;
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = sum},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(sum);
        res.LevelName.Should().Be(curLevel.Name);
        res.LevelCondition.Should().Be(curLevel.Condition);
        res.LevelAwardPercent.Should().Be(curLevel.AwardPercent);
        res.LevelAwardSum.Should().Be(curLevel.AwardSum);

        res.NextLevelName.Should().BeNull();
        res.NextLevelCondition.Should().BeNull();
        res.NextLevelAwardPercent.Should().BeNull();
        res.NextLevelAwardSum.Should().BeNull();
    }


    [Fact]
    public async Task GetLastLevelExecCondition_WorksCorrectly()
    {
        var bonus = new BonusProgramRep().Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);
        res.LevelName.Should().Be(curLevel.Name);
        res.LevelCondition.Should().Be(curLevel.Condition);
        res.LevelAwardPercent.Should().Be(curLevel.AwardPercent);
        res.LevelAwardSum.Should().Be(curLevel.AwardSum);

        res.NextLevelName.Should().BeNull();
        res.NextLevelCondition.Should().BeNull();
        res.NextLevelAwardPercent.Should().BeNull();
        res.NextLevelAwardSum.Should().BeNull();
    }

    [Fact]
    public async Task GetMiddleLevel3Sessions_WorksCorrectly()
    {
        var bonus = new BonusProgramRep().Get();
        var programLevels = bonus.ProgramLevels.OrderBy(x=>x.Level).ToArray();
        var curLevel = programLevels[2];
        var nextLevel = programLevels[3];

        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition/3 + 1},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment =curLevel.Condition/3 +2 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }, new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition/3 + 3 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition / 3 + curLevel.Condition / 3 + curLevel.Condition / 3 + 6);
        res.LevelName.Should().Be(curLevel.Name);
        res.LevelCondition.Should().Be(curLevel.Condition);
        res.LevelAwardPercent.Should().Be(curLevel.AwardPercent);
        res.LevelAwardSum.Should().Be(curLevel.AwardSum);

        res.NextLevelName.Should().Be(nextLevel.Name);
        res.NextLevelCondition.Should().Be(nextLevel.Condition);
        res.NextLevelAwardPercent.Should().Be(nextLevel.AwardPercent);
        res.NextLevelAwardSum.Should().Be(nextLevel.AwardSum);
    }

    [Fact]
    public async Task GetMiddleLevelOneSession_WorksCorrectly()
    {
        var bonus = new BonusProgramRep().Get();
        var programLevels = bonus.ProgramLevels.OrderBy(x=>x.Level).ToArray();
        var curLevel = programLevels[2];
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();
        res.LevelName.Should().Be(curLevel.Name);
        res.LevelCondition.Should().Be(curLevel.Condition);
        res.LevelAwardPercent.Should().Be(curLevel.AwardPercent);
        res.LevelAwardSum.Should().Be(curLevel.AwardSum);
    }

    [Fact]
    public async Task MongoSessionHasEmptyFields_WorksCorrectly()
    {
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = null },
                status = 6,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = null,
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = new MongoTariff() { BankId = null },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = null,
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = null,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = null, chargingClientType = null, clientNodeId = null },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = new MongoTariff() { BankId = 1 },
                user = null,
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            }
        });

        var bonusPrograms = await api.ApiBonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        bonusPrograms.Should().NotBeNull();
    }

[Fact]
    public async Task ZeroAchievement_ReturnLevels0and1()
    {
        mongo.Sessions.InsertOne(new MongoSession()
        {
            operation = new MongoOperation() { calculatedPayment = 100 },
            status = 6,
            tariff = new MongoTariff() { BankId = 1 },
            user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
            chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
        });

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
