using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace BonusService.Common;

public static class HangfireServicesExt
{
    private static string GetHangfireConnectionString(this IConfiguration configuration) => configuration.GetConnectionString("Hangfire") ?? throw new ArgumentException("ConnectionStrings.Hangfire not exist in configuration");

    public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetHangfireConnectionString();
        var delays = new [] { 1, 5, 10 };
        services.AddDbContext<HangfireDbContext>(opt => opt.UseNpgsql(conStr));
        services.AddHangfire((provider, config) =>
        {
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HangfireDbContext>();
            db.Database.EnsureCreated();

            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSerializerSettings(new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 10
                })
                .UseFilter(new AutomaticRetryAttribute { Attempts = delays.Length, DelaysInSeconds = delays })
                .UsePostgreSqlStorage((opt) =>
                {
                    opt.UseNpgsqlConnection(conStr, connection =>
                    {

                    });
                });
        });

        services.AddHangfireServer();

        return services;
    }

    public class HangfireDbContext : DbContext
    {
        public HangfireDbContext(){ }

        public HangfireDbContext(DbContextOptions<HangfireDbContext> options): base(options)
        {

        }
    }

    // This class injects the default DI container into hangfire jobs
    public class HangfireActivator : JobActivator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public HangfireActivator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new AspNetCoreJobActivatorScope(_serviceScopeFactory.CreateScope());
        }
    }

    class AspNetCoreJobActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope _scope;
        public AspNetCoreJobActivatorScope(IServiceScope scope)
        {
            _scope = scope;
        }

        public override object Resolve(Type type)
        {
            return _scope.ServiceProvider.GetRequiredService(type);
        }

        public override void DisposeScope()
        {
            _scope.Dispose();
        }
    }
}
