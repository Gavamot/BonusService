using BonusService.Common;
using Mediator;
namespace BonusService.BonusPrograms.ChargedByStationsBonus;

public record ChargedByStationsBonusAchievementRequest(string PersonId, Interval interval, int BankId) : IRequest<int>;
public sealed class ChargedByStationsBonusAchievementCommand(
    MongoDbContext mongo) : IRequestHandler<ChargedByStationsBonusAchievementRequest, int>
{
    public ValueTask<int> Handle(ChargedByStationsBonusAchievementRequest request, CancellationToken ct)
    {
        var achievement = AccumulateByIntervalBonusJob.GetSessionsRequest(mongo, request.BankId, request.interval)
            .Where(x=> x.user!.clientNodeId ==  request.PersonId && x.operation!.cpName != null)
            .DistinctBy(x=> x.operation!.cpName ?? "")
            .Count();
        return ValueTask.FromResult(achievement);
    }
}
