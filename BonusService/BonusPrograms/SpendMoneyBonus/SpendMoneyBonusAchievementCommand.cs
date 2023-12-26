using BonusService.Common;
using Mediator;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.SpendMoneyBonus;

public record SpendMoneyBonusAchievementRequest(string PersonId, Interval interval, int BankId) : IRequest<long>;
public sealed class SpendMoneyBonusAchievementCommand(
    MongoDbContext mongo) : IRequestHandler<SpendMoneyBonusAchievementRequest, long>
{
    public ValueTask<long> Handle(SpendMoneyBonusAchievementRequest request, CancellationToken ct)
    {
        var achievement = AccumulateByIntervalBonusJob.GetSessionsRequest(mongo, request.BankId, request.interval)
            .Where(x=> x.user!.clientNodeId ==  request.PersonId)
            .Sum(x=> x.operation!.calculatedPayment ?? 0);
        return ValueTask.FromResult(achievement);
    }
}
