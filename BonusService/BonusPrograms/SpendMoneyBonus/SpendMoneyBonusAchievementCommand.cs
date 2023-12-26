using BonusService.Common;
using Mediator;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.SpendMoneyBonus;

public record SpendMoneyBonusAchievementRequest(string PersonId, Interval interval, int BankId) : IRequest<long>;
public sealed class SpendMoneyBonusAchievementCommand : IRequestHandler<SpendMoneyBonusAchievementRequest, long>
{
    private readonly MongoDbContext _mongo;
    private readonly IDateTimeService _dateTimeService;

    public SpendMoneyBonusAchievementCommand(
        MongoDbContext mongo,
        IDateTimeService dateTimeService)
    {
        _mongo = mongo;
        _dateTimeService = dateTimeService;
    }

    public ValueTask<long> Handle(SpendMoneyBonusAchievementRequest request, CancellationToken ct)
    {
        var payment = AccumulateByIntervalBonusJob.GetSessionsRequest(_mongo, request.BankId, request.interval)
            .Where(x=> x.user!.clientNodeId ==  request.PersonId)
            .Sum(x=> x.operation!.calculatedPayment ?? 0);
        return ValueTask.FromResult(payment);
    }
}
