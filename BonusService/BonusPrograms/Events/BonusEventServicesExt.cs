using System.Reflection;
using MassTransit;
using UserProfileService;
using UserProfileService.Events;

namespace BonusService.BonusPrograms.Events;

public static class BonusEventServicesExt
{

    public static IServiceCollection AddBonusEventServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetAssembly(typeof(RegistrationEventConsumer));

        services.AddMussTransitServices(configuration, assembly,(cfg,ctx) =>
        {
            /*cfg.ReceiveEndpoint("bonus-new-user-registration", opt =>
            {
                opt.ConfigureConsumer<RegistrationEventConsumer>(ctx);
            });*/

            /*cfg.ReceiveEndpoint("bonus-user-data-committed", opt =>
            {
                opt.ConfigureConsumer<UserDataCommittedEventConsumer>(ctx);
            });*/
        });
        return services;
    }
}
