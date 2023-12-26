using BonusService.Common;
using Mediator;
namespace BonusService.BonusPrograms.ChargedByCapacityBonus;

public record ChargedByCapacityAchievementRequest(string PersonId, Interval interval, int BankId) : IRequest<long>;
public sealed class ChargedByCapacityBonusAchievementCommand(
    MongoDbContext mongo) : IRequestHandler<ChargedByCapacityAchievementRequest, long>
{
    public ValueTask<long> Handle(ChargedByCapacityAchievementRequest request, CancellationToken ct)
    {
        var achievement= AccumulateByIntervalBonusJob.GetSessionsRequest(mongo, request.BankId, request.interval)
            .Where(x=> x.user!.clientNodeId ==  request.PersonId)
            .Sum(x=> x.operation!.calculatedConsume ?? 0);
        return ValueTask.FromResult(achievement);
    }
}
