using BonusService.BonusPrograms.SpendMoneyBonus;
namespace BonusService.BonusPrograms;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBonusProgramsRunner, BonusProgramsRunner>();
        services.AddScoped<SpendMoneyBonusJob>();
        return services;
    }
}
