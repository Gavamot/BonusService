using BonusApi;
using BonusService.Common;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IAsyncDisposable
{
    public readonly string DbName = $"bonus_{Guid.NewGuid():N}";
    protected readonly BonusClient api;
    protected readonly FakeApplicationFactory<Program> Server;
    protected IServiceScope CreateScope() => Server.Services.CreateScope();

    public BonusTestApi(FakeApplicationFactory<Program> server)
    {
        Server = server;
        InitDatabases(server).GetAwaiter().GetResult();
        var httpClient = server.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        api = new BonusClient(httpClient);
    }

    protected async Task InitDatabases(FakeApplicationFactory<Program> server)
    {
        var postgres = InfraHelper.RunPostgresContainer();
        var mongo = InfraHelper.RunMongo(DbName).ContinueWith((t) =>
        {
            InfraHelper.CreateMongoDatabase(DbName);
        });
        await Task.WhenAll(postgres, mongo);
    }

    public async ValueTask DisposeAsync()
    {
        using var scope = Server.Services.CreateScope();

        var postgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        var postgresDelete = postgres.Database.EnsureDeletedAsync();

        var hangfire = scope.ServiceProvider.GetRequiredService<HangfireServicesExt.HangfireDbContext>();
        var hangfireDelete = hangfire.Database.EnsureDeletedAsync();

        var mongoDelete = InfraHelper.DropMongoDatabase(DbName);
        var tasks = new [] { postgresDelete, hangfireDelete, mongoDelete };
        await Task.WhenAll(tasks);
    }
}
