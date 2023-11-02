using BonusService.Common;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BonusService.Test;

public class FakeApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseTestServer()
            .UseEnvironment(Environments.Development)
            .ConfigureServices(services =>
            {
                ReplaceDbContext(services);
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
        RemoveService<DbContextOptions<MongoDbContext>>(services);
        RemoveService<MongoDbContext>(services);
    }
}
