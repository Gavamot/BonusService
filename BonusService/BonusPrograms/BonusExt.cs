using BonusService.Balance.OwnerByPayCrud;
using BonusService.BonusPrograms.BonusProgramCrud;
using BonusService.BonusPrograms.BonusProgramLevelsCrud;
using BonusService.BonusPrograms.SpendMoneyBonus;
namespace BonusService.BonusPrograms;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OwnerByPayRep>();
        services.AddScoped<BonusProgramRep>();
        services.AddScoped<BonusProgramLevelRep>();

        services.AddSingleton<IBonusProgramsRunner, BonusProgramsRunner>();
        services.AddScoped<SpendMoneyBonusJob>();
        return services;
    }
}
