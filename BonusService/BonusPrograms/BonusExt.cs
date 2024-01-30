using BonusService.Balance.OwnerByPayCrud;
using BonusService.BonusPrograms.BonusProgramCrud;
using BonusService.BonusPrograms.BonusProgramLevelsCrud;
using BonusService.BonusPrograms.ChargedByCapacityBonus;
using BonusService.BonusPrograms.ChargedByStationsBonus;
using BonusService.BonusPrograms.Events;
using BonusService.BonusPrograms.SpendMoneyBonus;
namespace BonusService.BonusPrograms;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OwnerByPayRep>();
        services.AddScoped<BonusProgramRep>();
        services.AddScoped<BonusProgramLevelRep>();

        services.AddScoped<IBonusProgramsRunner, BonusProgramsRunner>();
        services.AddScoped<SpendMoneyBonusJob>();
        services.AddScoped<ChargedByCapacityBonusJob>();
        services.AddScoped<ChargedByStationsBonusJob>();

        services.AddBonusEventServices(configuration);
        return services;
    }
}
