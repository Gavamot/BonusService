using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
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
            var a=  db.Database.GetConnectionString();
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

    public static void UseHangfire(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[]
            {
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    SslRedirect = false,
                    RequireSsl = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "adminka",
                            PasswordClear = "d6c61b11b2188340f59c9c1b4ada9345684434ef660c013f0432423423553454e9fb600aa1e09fc3453453469ea2376e747c0c5def0242a56c120002b29eef9bad59434dbc52b8f9f767c341"
                        }
                    }
                })
            }
        });

        var options = new BackgroundJobServerOptions { WorkerCount = 2 };
        app.UseHangfireServer(options);
    }
}
