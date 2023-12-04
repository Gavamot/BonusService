using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using BonusApi;
using BonusService.Common.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using FSharp.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BonusProgram = BonusService.Common.Postgres.Entity.BonusProgram;
using OwnerMaxBonusPay = BonusService.Common.Postgres.Entity.OwnerMaxBonusPay;
namespace BonusService.Test;

public class BonusProgramLevelCrudTest : BonusTestApi
{
    private BonusProgram bp;
    public BonusProgramLevelCrudTest(FakeApplicationFactory<Program> server) : base(server)
    {
        bp = postgres.BonusPrograms.Include(x=>x.ProgramLevels).First(x => x.Id == 1);
    }

    private void AssertAreEquals(BonusService.Common.Postgres.Entity.BonusProgramLevel actual, BonusApi.BonusProgramLevel expected)
    {
        expected.Should().NotBeNull();
        expected.Name.Should().Be(actual.Name);
        expected.Condition.Should().Be(actual.Condition);
        expected.AwardPercent.Should().Be(actual.AwardPercent);
        expected.LastUpdated.Should().Be(actual.LastUpdated);
        expected.AwardSum.Should().Be(actual.AwardSum);
        expected.BonusProgramId.Should().Be(actual.BonusProgramId);
    }

    [Fact]
    public async Task GetById()
    {
        var actual = bp.ProgramLevels.First();
        var expected= await api.BonusLevelsGetByIdAsync(actual.Id);
        AssertAreEquals(actual, expected);
    }

    /*[Fact]
    public async Task GetAll()
    {
        var actual = bp.ProgramLevels.OrderBy(x => x.Id).ToArray();
        var expectedCollection= await api.BonusLevelsGetAllAsync();
        var expected = expectedCollection.OrderBy(x => x.Id).ToArray();
        for (int i = 0; i < actual.Count(); i++)
        {
            AssertAreEquals(actual[i], expected[i]);
        }
    }*/

    [Fact]
    public async Task GetAllByBonusProgram()
    {
        var actual = bp.ProgramLevels.OrderBy(x => x.Id).ToArray();
        var expectedCollection= await api.BonusLevelsGetAllAsync();
        var expected = expectedCollection.OrderBy(x => x.Id).ToArray();
        for (int i = 0; i < actual.Count(); i++)
        {
            AssertAreEquals(actual[i], expected[i]);
        }
    }

    [Fact]
    public async Task Add()
    {
        int ProgramLevelsCount = bp.ProgramLevels.Count;
        BonusApi.BonusProgramLevel newItem1 = new()
        {
            Name = nameof(newItem1),
            Condition = 13,
            AwardPercent = 3,
            AwardSum = 44,
            BonusProgramId = bp.Id,
            Level = 5,
        };
        var bp1 = await api.BonusLevelsAddAsync(newItem1);

        // TODO Дописать Тесты !

        using var scope = CreateScope();
        var items = base.scope.ServiceProvider.GetRequiredService<PostgresDbContext>().BonusProgramsLevels.ToArray();
        items.Length.Should().Be(ProgramLevelsCount + 1);
        var dbItem = items.First(x => x.Id == bp1.Id);
        dbItem.Name.Should().Be(newItem1.Name);
        dbItem.Level.Should().Be(newItem1.Level);
        dbItem.Condition.Should().Be(newItem1.Condition);
        dbItem.AwardPercent.Should().Be(newItem1.AwardPercent);
        dbItem.BonusProgramId.Should().Be(newItem1.BonusProgramId);
        dbItem.AwardSum.Should().Be(newItem1.AwardSum);
    }

    [Fact]
    public async Task Update()
    {
        var oldLevel = bp.ProgramLevels.First();
        var newBonusLevel = new BonusProgramLevelDto()
        {
            Id = oldLevel.Id,
            Level = 100,
            AwardSum = 9999
        };
        await api.BonusLevelsUpdateAsync(newBonusLevel);

        using var scope = CreateScope();
        var expected = scope.GetRequiredService<PostgresDbContext>().BonusProgramsLevels.First(x => x.Id == 1);
        expected.Level.Should().Be(newBonusLevel.Level);
        expected.AwardSum.Should().Be(newBonusLevel.AwardSum);
        expected.Condition.Should().Be(oldLevel.Condition);
        expected.Name.Should().Be(oldLevel.Name);
        expected.AwardPercent.Should().Be(oldLevel.AwardPercent);
        expected.BonusProgramId.Should().Be(oldLevel.BonusProgramId);
    }

    [Fact]
    public async Task Delete()
    {
        int ProgramLevelsCount = bp.ProgramLevels.Count;
        var f = bp.ProgramLevels.ToArray().First();
        await api.BonusLevelsDeleteByIdAsync(f.Id);

        using var scope = CreateScope();
        var newPostgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        newPostgres.BonusProgramsLevels.Count().Should().Be(ProgramLevelsCount - 1);
        newPostgres.BonusProgramsLevels.Any(x=>x.Id == f.Id).Should().BeFalse();
    }
}
