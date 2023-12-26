using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using Mediator;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.ChargedByCapacityBonus;

public record ChargedByCapacityAchievementRequest(string PersonId, Interval interval, int BankId) : IRequest<long>;
public sealed class ChargedByCapacityBonusAchievementCommand : IRequestHandler<ChargedByCapacityAchievementRequest, long>
{
    private readonly MongoDbContext _mongo;
    private readonly IDateTimeService _dateTimeService;

    public ChargedByCapacityBonusAchievementCommand(
        MongoDbContext mongo,
        IDateTimeService dateTimeService)
    {
        _mongo = mongo;
        _dateTimeService = dateTimeService;
    }

    public ValueTask<long> Handle(ChargedByCapacityAchievementRequest request, CancellationToken ct)
    {
        var payment= AccumulateByIntervalBonusJob.GetSessionsRequest(_mongo, request.BankId, request.interval)
            .Where(x=> x.user!.clientNodeId ==  request.PersonId)
            .Sum(x=> x.operation!.calculatedPayment ?? 0);
        return ValueTask.FromResult(payment);
    }
}
