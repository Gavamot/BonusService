namespace BonusService.Bonuses;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<RunBonusProgramsBackgroundService>();
        services.AddScoped<MonthlySumBonusJob>();
        services.AddScoped<IBonusService, BonusService>();
        services.AddScoped<IBonusProgramRep, BonusProgramRep>();
        services.AddScoped<MonthlySumBonusJob>();
        return services;
    }
}
