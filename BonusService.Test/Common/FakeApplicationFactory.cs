using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BonusService.Test.Common;

public class FakeApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(Program.AppTest, Program.AppTest);
        var config = new ConfigurationManager()
            .AddJsonFile("appsettings.json")
            .Build();
        builder
            .UseTestServer()
            .UseEnvironment(Environments.Production)
            .UseConfiguration(config)
            .ConfigureServices(services =>
            {
                /*RemoveService<IBackgroundJobClient>(services);
                services.AddSingleton<IBackgroundJobClient, FakeBackgroundJobClient>();*/
            });
    }

    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof (T));
        if(descriptor == null) return;
        services.Remove(descriptor);
    }
}
