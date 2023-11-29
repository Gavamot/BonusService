using BonusService.BonusPrograms;
using BonusService.Common;
using BonusService.Test.Common;
using FluentAssertions;
#pragma warning disable CS8604 // Possible null reference argument.
namespace BonusService.Test.BonusPrograms;

public class BonusProgramAchievementTest : BonusTestApi
{
    public BonusProgramAchievementTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task AnotherDateNotCounting_OnlySumCurrentMonth()
    {
        var bonus = BonusProgramSeed.Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1Start.AddHours(-1).UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1End.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1Start.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);

    }

    [Fact]
    public async Task GetLastLevelSumMoreWhenNeed_WorksCorrectly()
    {
        var bonus = BonusProgramSeed.Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        var sum = curLevel.Condition + 1000;
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = sum},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(sum);
        var bonusProgram = BonusProgramSeed.Get();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonusProgram.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonusProgram.Id);
    }


    [Fact]
    public async Task GetLastLevelExecCondition_WorksCorrectly()
    {
        var bonus = BonusProgramSeed.Get();
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);
        var bonusProgram = BonusProgramSeed.Get();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonusProgram.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonusProgram.Id);
    }

    [Fact]
    public async Task GetMiddleLevel3Sessions_WorksCorrectly()
    {
        var bonus = BonusProgramSeed.Get();
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
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment =curLevel.Condition/3 +2 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }, new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition/3 + 3 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition / 3 + curLevel.Condition / 3 + curLevel.Condition / 3 + 6);
        var bonusProgram = BonusProgramSeed.Get();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonusProgram.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonusProgram.Id);
    }

    [Fact]
    public async Task GetMiddleLevelOneSession_WorksCorrectly()
    {
        var bonus = BonusProgramSeed.Get();
        var programLevels = bonus.ProgramLevels.OrderBy(x=>x.Level).ToArray();
        var curLevel = programLevels[2];
        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1String },
                chargeEndTime = Q.IntervalMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();
        var bonusProgram = BonusProgramSeed.Get();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonusProgram.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonusProgram.Id);
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
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = null,
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = new MongoTariff() { BankId = null },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = 6,
                tariff = null,
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
                chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = 100 },
                status = null,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
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

        var bonusPrograms = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
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
            user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1String },
            chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
        });

        var items = await api.BonusProgramAchievementGetPersonAchievementAsync(Q.PersonId1);
        items.Items?.Count.Should().Be(1);
        var item = items.Items.First();
        item.CurrentSum.Should().Be(0);

        var bonusProgram = BonusProgramSeed.Get();

        var res = items.Items.First();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonusProgram.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonusProgram.Id);
    }
}
