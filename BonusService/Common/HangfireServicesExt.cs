using System.Collections.Immutable;
using BonusService.Common.Postgres;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BonusService.Common;

public static class HangfireHistoryStatus
{
    public const string Deleted = nameof(Deleted);
    public const string Enqueued = nameof(Enqueued);
    public const string Awaiting = nameof(Awaiting);
    public const string Processing = nameof(Processing);
    public const string Scheduled = nameof(Scheduled);
    public const string Succeeded = nameof(Succeeded);
    public const string Failed = nameof(Failed);

    public static readonly ImmutableHashSet<string> EndJobStatuses = ImmutableHashSet.Create(new [] { Deleted, Succeeded, Failed });
    public static bool IsJobEnded(this IBackgroundJobClientV2 client, string jobId) => client.Storage.GetMonitoringApi().JobDetails(jobId).History.Any(x=> EndJobStatuses.Contains(x.StateName));
    public static bool IsJobNotEnded(this IBackgroundJobClientV2 client, string jobId) => client.IsJobEnded(jobId) == false;

}

public class HangfireDbContext : DbContext
{
    public HangfireDbContext(){ }

    public HangfireDbContext(DbContextOptions<HangfireDbContext> options): base(options)
    {

    }
}

public static class HangfireServicesExt
{
    private static string GetHangfireConnectionString(this IConfiguration configuration) => configuration.GetConnectionString("Hangfire") ?? throw new ArgumentException("ConnectionStrings.Hangfire not exist in configuration");

    public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetHangfireConnectionString();
        //var delays = new [] { 1, 5, 10 };

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
                .UsePostgreSqlStorage((opt) =>
                {
                    opt.UseNpgsqlConnection(conStr, connection =>
                    {

                    });
                });
            if(Program.IsAppTest()) return;
            config.UseConsole();
        });

        services.AddHangfireServer(opt =>
        {
            //opt.CancellationCheckInterval = TimeSpan.FromSeconds(1);
            opt.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            opt.WorkerCount = 2;
            //opt.StopTimeout = TimeSpan.FromSeconds(30);
            //opt.ShutdownTimeout = TimeSpan.FromSeconds(30.0);
            //opt.ServerTimeout = TimeSpan.FromMinutes(10);
        });
        return services;
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
        var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<HangfireDbContext>();
        db.Database.Migrate();

        var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(scopeFactory));

        if (Program.IsAppTest()) return;

        var options = new DashboardOptions();
        if (Program.IsLocal() == false)
        {
            options.Authorization = new []
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
            };
        }
        //app.UseHangfireServer();
        app.UseHangfireDashboard("/hangfire", options);
    }
}
