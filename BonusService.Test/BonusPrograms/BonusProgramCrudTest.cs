using BonusApi;
using BonusService.Common.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using BonusProgram = BonusService.Common.Postgres.Entity.BonusProgram;
namespace BonusService.Test;

public class BonusProgramCrudTest : BonusTestApi
{
    private BonusProgram bp;
    public BonusProgramCrudTest(FakeApplicationFactory<Program> server) : base(server)
    {
        bp = postgres.BonusPrograms.First(x => x.Id == Q.BonusProgramId1);
    }

    [Fact]
    public async Task GetById()
    {
        var item1= await api.BonusProgramGetByIdAsync(Q.BonusProgramId1);
        item1.Should().NotBeNull();
        item1.BankId.Should().Be(bp.BankId);
        item1.Name.Should().Be(bp.Name);
        item1.DateStart.Should().Be(bp.DateStart);
    }

    [Fact]
    public async Task GetAll()
    {
        var items= await api.BonusProgramGetAllAsync();
        items.Count.Should().Be(1);
        var item1 = items.First();
        item1.Should().NotBeNull();
        item1.BankId.Should().Be(bp.BankId);
        item1.Name.Should().Be(bp.Name);
        item1.DateStart.Should().Be(bp.DateStart);
    }

    [Fact]
    public async Task Add()
    {
        BonusApi.BonusProgram newBonusProgram1 = new()
        {
            Name = nameof(newBonusProgram1),
            DateStart = new DateTimeOffset(2001, 11, 1, 0, 1, 2, new TimeSpan(0)),
            DateStop = new DateTimeOffset(2002, 11, 2, 0, 1, 3, new TimeSpan(0)),
            Description = nameof(newBonusProgram1) + "desc",
            BankId = Q.BankIdRub,
            ExecutionCron = "* * * * * 1",
            FrequencyType = FrequencyTypes.Month,
            FrequencyValue = 10,
            BonusProgramType = BonusProgramType.SpendMoney,
            LastUpdated = DateTimeOffset.UtcNow,
            Id = 0,
        };

        BonusApi.BonusProgram newBonusProgram2 = new()
        {
            Name = nameof(newBonusProgram2),
            DateStart = new DateTimeOffset(2003, 11, 1, 0, 1, 2, new TimeSpan(0)),
            DateStop = new DateTimeOffset(2004, 11, 2, 0, 1, 3, new TimeSpan(0)),
            Description = nameof(newBonusProgram2) + "desc",
            BankId = Q.BankIdRub,
            ExecutionCron = "* * * 3 2 1",
            FrequencyType = FrequencyTypes.Day,
            FrequencyValue = 24,
            BonusProgramType = BonusProgramType.ChargedByCapacity,
            LastUpdated = DateTimeOffset.UtcNow,
            Id = 0,
        };


        var bp1 = await api.BonusProgramAddAsync(newBonusProgram1);
        var bp2 = await api.BonusProgramAddAsync(newBonusProgram2);

        // TODO Дописать Тесты !

        var items = postgres.BonusPrograms.ToArray();
        items.Length.Should().Be(3);
        var item1 = items.First(x => x.Name == bp1.Name);
        item1.Name.Should().Be(newBonusProgram1.Name);
        item1.DateStart.Should().Be(newBonusProgram1.DateStart);
        item1.DateStop.Should().Be(newBonusProgram1.DateStop);
        item1.ExecutionCron.Should().Be(newBonusProgram1.ExecutionCron);
        item1.BonusProgramType.Should().Be((BonusService.Common.Postgres.Entity.BonusProgramType)newBonusProgram1.BonusProgramType);
    }

    [Fact]
    public async Task Update()
    {
        var newDateStart = new DateTimeOffset(1970, 1, 1, 1, 1, 1, new TimeSpan(0));
        await api.BonusProgramUpdateAsync(new BonusProgramDto()
        {
            Id =  bp.Id,
            DateStart = newDateStart
        });


        using var serviceScope = CreateScope();
        var expected = serviceScope.GetRequiredService<PostgresDbContext>().BonusPrograms.First(x => x.Id == Q.BonusProgramId1);
        expected.DateStart.Should().Be(newDateStart);
        expected.DateStop.Should().Be(bp.DateStop);
        expected.Description.Should().Be(bp.Description);
        expected.Name.Should().Be(bp.Name);
        expected.BankId.Should().Be(bp.BankId);
        expected.FrequencyType.Should().Be(bp.FrequencyType);
        expected.ExecutionCron.Should().Be(bp.ExecutionCron);
        expected.BonusProgramType.Should().Be(bp.BonusProgramType);
    }

    [Fact]
    public async Task Delete()
    {
        postgres.BonusPrograms.First().IsDeleted.Should().Be(false);
        await api.BonusProgramDeleteByIdAsync(Q.BonusProgramId1);
        using var serviceScope = CreateScope();
        var db = serviceScope.GetRequiredService<PostgresDbContext>();
        var expected = db.BonusPrograms.First(x => x.Id == Q.BonusProgramId1);
        expected.IsDeleted.Should().Be(true);
        db.BonusPrograms.Count().Should().Be(1);
    }

    [Fact]
    public async Task NotShowDeletedInGetAll()
    {
        postgres.BonusPrograms.First().IsDeleted.Should().Be(false);
        await api.BonusProgramDeleteByIdAsync(Q.BonusProgramId1);
        var bonusPrograms = await api.BonusProgramGetAllAsync();
        bonusPrograms.Should().BeEmpty();
    }
}
