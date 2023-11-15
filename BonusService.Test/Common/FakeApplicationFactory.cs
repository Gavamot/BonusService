using BonusService.Common;
using BonusService.Postgres;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BonusService.Test.Common;

public class FakeApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public readonly string DbName = $"bonus_{Guid.NewGuid():N}";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(Program.AppTest, Program.AppTest);
        builder
            .UseTestServer()
            .UseEnvironment(Environments.Development)
            .ConfigureServices(services =>
            {
                InfraHelper.RunPostgresContainer();
                //ReplaceDbContext(services);
                RemoveService<IBackgroundJobClient>(services);
                services.AddSingleton<IBackgroundJobClient, FakeBackgroundJobClient>();
            });
    }

    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof (T));
        if(descriptor == null) return;
        services.Remove(descriptor);
    }
    private void ReplaceDbContext(IServiceCollection services)
    {
        RemoveService<DbContextOptions<PostgresDbContext>>(services);
        RemoveService<PostgresDbContext>(services);

        services.AddDbContext<PostgresDbContext>(opt =>
        {
            opt.UseNpgsql($"Host=localhost;Port={InfraHelper.PostgresContainerPort};Database={DbName};Username=postgres");
        });

        RemoveService<DbContextOptions<MongoDbContext>>(services);
        RemoveService<MongoDbContext>(services);
        services.AddMongoDB<MongoDbContext>($"mongodb://localhost:{InfraHelper.MongoContainerPort}?serverSelectionTimeoutMS=60000&connectTimeoutMS=7000&socketTimeoutMS=7000",DbName);
    }
}
