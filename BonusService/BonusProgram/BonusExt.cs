namespace BonusService.Bonuses;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBonusProgramsRunner, BonusProgramsRunner>();
        services.AddScoped<MonthlySumBonusJob>();
        services.AddScoped<IBonusProgramRep, BonusProgramRep>();
        return services;
    }
}
