using BonusApi;
using BonusService.Common;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IDisposable
{
    protected readonly BonusClient api;
    protected readonly FakeApplicationFactory<Program> Server;
    protected IServiceScope CreateScope() => Server.Services.CreateScope();

    public BonusTestApi(FakeApplicationFactory<Program> server)
    {
        Server = server;
        InitPostgres(server);
        var httpClient = server.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        api = new BonusClient(httpClient);
    }

    protected void InitPostgres(FakeApplicationFactory<Program> server)
    {
        InfraHelper.RunPostgresContainer();

        using var scope = server.Services.CreateScope();
        var postgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        postgres.Database.Migrate();

        InfraHelper.RunMongo();
        InfraHelper.CreateMongoDatabase(Server.DbName);
    }

    public void Dispose()
    {
        using var scope = Server.Services.CreateScope();
        var postgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        postgres.Database.EnsureDeleted();
        InfraHelper.DropMongoDatabase(Server.DbName);

        var hangfire = scope.ServiceProvider.GetRequiredService<HangfireServicesExt.HangfireDbContext>();
        hangfire.Database.EnsureDeleted();
    }
}
