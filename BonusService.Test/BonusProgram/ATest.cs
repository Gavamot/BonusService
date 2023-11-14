using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
namespace BonusService.Test;


public class ATest : BonusTestApi, IClassFixture<PostgresDbTestContainer>
{
    private readonly PostgresDbTestContainer postgresDbContainer;
    public ATest(FakeApplicationFactory<Program> server, PostgresDbTestContainer postgresDbContainer) : base(server)
    {
        this.postgresDbContainer = postgresDbContainer;

    }

    [Fact]
    public async Task GetBonusProgram()
    {
        var programs = await api.ApiBonusProgramAsync();
        programs.Should().NotBeNullOrEmpty();
    }

}
