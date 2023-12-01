using BonusService.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.SpendMoneyBonus;

public record SpendMoneyBonusAchievementRequest(string PersonId, DateTimeInterval Interval, int BankId) : IRequest<long>;
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
        var payment = _mongo.Sessions.AsQueryable().Where(x =>
                x.status == MongoSessionStatus.Paid
                && x.user != null
                && x.user.clientNodeId ==  request.PersonId
                && x.user.chargingClientType == MongoChargingClientType.IndividualEntity
                && x.tariff != null
                && x.tariff.BankId == request.BankId
                && x.operation != null
                && x.operation.calculatedPayment > 0
                && x.chargeEndTime>= request.Interval.from.UtcDateTime && x.chargeEndTime< request.Interval.to.UtcDateTime)
            .Sum(x=> x.operation!.calculatedPayment ?? 0);
        return ValueTask.FromResult(payment);
    }
}
