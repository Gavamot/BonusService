using System.Text.RegularExpressions;
using BonusService.Test.Common;
using FluentAssertions;
using Testcontainers.PostgreSql;
namespace BonusService.Test;

public class PostgresDbTestContainer: IAsyncLifetime
{
    static PostgresDbTestContainer()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithName("test_postgres")
            .WithDatabase("test")
            .WithUsername("postgres")
            .WithPassword("123")
            .WithPortBinding(8888, 5432)
            .Build();

        PostgreSqlContainer.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
    private readonly static PostgreSqlContainer PostgreSqlContainer;

    private readonly string dbName = Guid.NewGuid().ToString();
    public string ConStr { get; private set; }
    public async Task InitializeAsync()
    {
        var result = await PostgreSqlContainer.ExecAsync(new List<string>(){"createdb","--host=localhost","--port=5432","--user=postgres", dbName});
        if (result.ExitCode != 0) throw new Exception("CreatePostgresDb error. ExecAsync return ExitCode != 0");
        var seekDbName = Regex.Match(PostgreSqlContainer.GetConnectionString(), "(?<=Database=).*?(?=;)");
        ConStr = PostgreSqlContainer.GetConnectionString().Replace($"Database={seekDbName}", $"Database={dbName}");
    }
    public async Task DisposeAsync()
    {
        var result = await PostgreSqlContainer.ExecAsync(new List<string>(){"dropdb","--host=localhost","--port=5432","--user=postgres", dbName});
        if (result.ExitCode != 0) throw new Exception("CreatePostgresDb error. ExecAsync return ExitCode != 0");
    }
}

public class BonusProgramTest : BonusTestApi, IClassFixture<PostgresDbTestContainer>
{
    private readonly PostgresDbTestContainer postgresDbContainer;
    public BonusProgramTest(FakeApplicationFactory<Program> server, PostgresDbTestContainer postgresDbContainer) : base(server)
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
