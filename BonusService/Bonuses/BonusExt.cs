namespace BonusService.Bonuses;

public static class BonusExt
{
    public static IServiceCollection AddBonusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<MonthlySumBonusJob>();
        return services;
    }
}
