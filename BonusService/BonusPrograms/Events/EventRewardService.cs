using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
namespace BonusService.BonusPrograms.Events;

public interface IEventRewardService
{
    Task AccrueEventReward(EventTypes type, string personId, Guid? correlationId);
}

public class EventRewardService(BonusDbContext db, IDateTimeService dateTimeService, ILogger<EventRewardService> logger, string correlationId) : IEventRewardService
{
    public async Task AccrueEventReward(EventTypes type, string personId, Guid? correlationId)
    {
        EventReward? eventReward = await db.EventRewards.FirstOrDefaultAsync(x => x.Type == type);
        var now = dateTimeService.GetNowUtc();
        if (eventReward == null ||
            eventReward.Reward == 0 ||
            (eventReward as IHaveActivePeriod).IsActive(now) == false)
        {
            return;
        }

        await db.Transactions.AddAsync(new Transaction()
        {
            PersonId = personId,
            Type = TransactionType.Event,
            TransactionId = $"{type}_{personId}_{dateTimeService.GetNowUtc().ToUnixTimeMilliseconds()}",
            Description = $"{type} - Начисленно по событию { (correlationId == null ? "" : "correlationId = " + correlationId )}",
            BankId = 1,
            LastUpdated = now,
            BonusSum = eventReward.Reward
        });
        await db.SaveChangesAsync();
    }
}
