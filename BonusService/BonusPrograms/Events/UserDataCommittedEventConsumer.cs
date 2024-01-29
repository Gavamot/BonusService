using BonusService.Common.Postgres.Entity;
using MassTransit;
using UserProfileService.Events;
namespace BonusService.BonusPrograms.Events;

class UserDataCommittedEventConsumer(IEventRewardService rewardService) :IConsumer<UserDataCommittedEvent>
{
    public Task Consume(ConsumeContext<UserDataCommittedEvent> context)
    {
        return rewardService.AccrueEventReward(EventTypes.UserDataCommitted, context.Message.PersonId, context.CorrelationId ?? Guid.NewGuid());
    }
}
