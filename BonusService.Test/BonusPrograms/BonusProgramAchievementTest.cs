using BonusService.Common;
using BonusService.Test.Common;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BonusProgram = BonusService.Common.Postgres.Entity.BonusProgram;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
namespace BonusService.Test.BonusPrograms;

public class BonusProgramAchievementTest : BonusTestApi
{
    private BonusProgram bonus;
    public BonusProgramAchievementTest(FakeApplicationFactory<Program> server) : base(server)
    {
        bonus = postgres.BonusPrograms.Include(x => x.ProgramLevels)
            .Include(x=>x.BonusProgramHistory)
            .First();
    }

    [Fact]
    public async Task AnotherDateNotCounting_OnlySumCurrentMonth()
    {
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();
        await mongo.Sessions.InsertManyAsync(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1Start.AddHours(-1).UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1End.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.IntervalMoth1Start.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);

    }

    [Fact]
    public async Task GetLastLevelSumMoreWhenNeed_WorksCorrectly()
    {
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        var sum = curLevel.Condition + 1000;
        await mongo.Sessions.InsertManyAsync(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = sum},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(sum);
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonus.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonus.Id);
    }


    [Fact]
    public async Task GetLastLevelExecCondition_WorksCorrectly()
    {
        var curLevel = bonus.ProgramLevels.OrderBy(x=> x.Level).Last();

        await mongo.Sessions.InsertManyAsync(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            }
        });

        Fake.ClearRecordedCalls(this.server.DateTimeService);
        A.CallTo(() => this.server.DateTimeService.GetNowUtc()).
            Returns(Q.IntervalMoth1Start);

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition);
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonus.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonus.Id);
    }

    [Fact]
    public async Task GetMiddleLevel3Sessions_WorksCorrectly()
    {
        var programLevels = bonus.ProgramLevels.OrderBy(x=>x.Level).ToArray();
        var curLevel = programLevels[2];

        mongo.Sessions.InsertMany(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition/3 + 1},
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            },
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment =curLevel.Condition/3 +2 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            }, new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition/3 + 3 },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();

        res.CurrentSum.Should().Be(curLevel.Condition / 3 + curLevel.Condition / 3 + curLevel.Condition / 3 + 6);

        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonus.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonus.Id);
    }

    [Fact]
    public async Task GetMiddleLevelOneSession_WorksCorrectly()
    {
        var programLevels = bonus.ProgramLevels.OrderBy(x=>x.Level).ToArray();
        var curLevel = programLevels[2];
        await mongo.Sessions.InsertManyAsync(new []
        {
            new MongoSession()
            {
                operation = new MongoOperation() { calculatedPayment = curLevel.Condition },
                status = 7,
                tariff = new MongoTariff() { BankId = 1 },
                user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 0, clientNodeId = Q.PersonId1 },
                chargeEndTime = Q.TimeExtMoth1.from.UtcDateTime,
            }
        });

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        var res = bonusPrograms.Items.First();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonus.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonus.Id);
    }

    [Fact]
    public async Task MongoSessionHasEmptyFields_WorksCorrectly()
    {
        await mongo.Sessions.InsertManyAsync(new []
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

        var bonusPrograms = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        bonusPrograms.Should().NotBeNull();
    }

[Fact]
    public async Task ZeroAchievement_ReturnLevels0and1()
    {
        await mongo.Sessions.InsertOneAsync(new MongoSession()
        {
            operation = new MongoOperation() { calculatedPayment = 100 },
            status = 6,
            tariff = new MongoTariff() { BankId = 1 },
            user = new MongoUser() { clientLogin = "Vasia", chargingClientType = 1, clientNodeId = Q.PersonId1 },
            chargeEndTime = new DateTime(2000, 1, 1, 1, 1, 1),
        });

        var items = await api.BonusProgramGetPersonAchievementAsync(Q.PersonId1);
        items.Items?.Count.Should().Be(1);
        var item = items.Items.First();
        item.CurrentSum.Should().Be(0);


        var res = items.Items.First();
        res.BonusProgram.Should().NotBeNull();
        res.BonusProgram.ProgramLevels.Count.Should().Be(bonus.ProgramLevels.Count);
        res.BonusProgram.Id.Should().Be(bonus.Id);
    }
}
