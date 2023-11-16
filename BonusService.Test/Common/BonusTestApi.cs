using BonusApi;
using BonusService.Common;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
namespace BonusService.Test.Common;

public class BonusTestApi : IClassFixture<FakeApplicationFactory<Program>>, IAsyncDisposable
{
    public static class Q
    {
        public const string Description1 = "Описанье 1";
        public readonly static Guid UserId1 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        public const string TransactionId1 = "3fa85f64-5717-4562-b3fc-2c963f66af11";
        public readonly static Guid PersonId1 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        public readonly static Guid PersonId2 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa7");
        public const int BankIdRub = 1;
        public const  int BankIdKaz = 7;
    }

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
        var mongo = InfraHelper.RunMongo(Server.DbName).ContinueWith((t) =>
        {
            InfraHelper.CreateMongoDatabase(Server.DbName);
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

        var mongoDelete = InfraHelper.DropMongoDatabase(Server.DbName);
        var tasks = new [] { postgresDelete, hangfireDelete, mongoDelete };
        await Task.WhenAll(tasks);
    }
}
