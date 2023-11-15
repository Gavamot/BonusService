using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace BonusService.Common;

public static class HangfireServicesExt
{
    private static string GetHangfireConnectionString(this IConfiguration configuration) => configuration.GetConnectionString("Hangfire") ?? throw new ArgumentException("ConnectionStrings.Hangfire not exist in configuration");

    /*private static void CreateHangfireDbIfNotExist(string conStr)
    {
        using var db = new HangfireDbContext(conStr);
        db.Database.EnsureCreated();
    }*/
    public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetHangfireConnectionString();
        var delays = new [] { 1, 5, 10 };
        services.AddDbContext<HangfireDbContext>(opt => opt.UseNpgsql(conStr));
        services.AddHangfire((provider, config) =>
        {
            //CreateHangfireDbIfNotExist(conStr);
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                .UseActivator(new HangfireActivator(provider.GetRequiredService<IServiceScopeFactory>()))
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

    public class HangfireActivator : JobActivator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HangfireActivator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new AspNetCoreJobActivatorScope(_serviceScopeFactory.CreateScope(), context.BackgroundJob.Id);
        }
    }

    public class AspNetCoreJobActivatorScope : JobActivatorScope
    {
        private readonly string _jobId;
        private readonly IServiceScope _serviceScope;
        public AspNetCoreJobActivatorScope (IServiceScope serviceScope, string jobId)
        {
            this._jobId = jobId;
            _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
        }

        public override object Resolve(Type type)
        {
            var res = ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, type);
            return res;
        }

        public override void DisposeScope()
        {
            _serviceScope.Dispose();
        }
    }
}
