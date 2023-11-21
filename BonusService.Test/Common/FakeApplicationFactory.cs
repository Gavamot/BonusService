using BonusService.Common;
using FakeItEasy;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BonusService.Test.Common;

public class FakeApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public readonly string DbName = $"bonus_{Guid.NewGuid():N}";

    public readonly IDateTimeService DateTimeService = A.Fake<IDateTimeService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(Program.AppTest, Program.AppTest);
        var config = new ConfigurationManager()
            .AddInMemoryCollection(new KeyValuePair<string, string?> []
            {
                new("ConnectionStrings:Hangfire", $"Host=localhost;Port=9999;Database=hangfire_{DbName};Username=postgres;Application Name=bonus-service"),
                new("ConnectionStrings:Postgres", $"Host=localhost;Port=9999;Database={DbName};Username=postgres;Application Name=bonus-service"),
                new("MongoConfig:ConnectionString", $"mongodb://localhost:9998?serverSelectionTimeoutMS=60000&connectTimeoutMS=7000&socketTimeoutMS=7000"),
                new("MongoConfig:QueriesFolder", "/PssPlatform/Queries"),
                new("MongoConfig:Database", $"{DbName}"),
            })
            .Build();
        builder
            .UseConfiguration(config)
            .UseTestServer()
            .UseEnvironment(Environments.Production)
            .UseConfiguration(config)
            .ConfigureServices(services =>
            {
                RemoveService<IBackgroundJobClient>(services);
                services.AddSingleton<IBackgroundJobClient, FakeBackgroundJobClient>();
                RemoveService<IDateTimeService>(services);
                services.AddSingleton<IDateTimeService>(x=> DateTimeService);
            });
    }

    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof (T));
        if(descriptor == null) return;
        services.Remove(descriptor);
    }
}
