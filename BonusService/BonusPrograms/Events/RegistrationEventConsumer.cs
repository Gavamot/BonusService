using BonusService.Common.Postgres.Entity;
using MassTransit;
using UserProfileService.Events;
namespace BonusService.BonusPrograms.Events;

public class RegistrationEventConsumer(IEventRewardService rewardService) :IConsumer<RegistrationEvent>
{
    public async Task Consume(ConsumeContext<RegistrationEvent> context)
    {
        await rewardService.AccrueEventReward(EventTypes.NewUserRegistration, context.Message.PersonId, context.CorrelationId);
    }
}
