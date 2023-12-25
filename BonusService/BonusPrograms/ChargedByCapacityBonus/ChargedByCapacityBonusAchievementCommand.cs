using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using Mediator;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.ChargedByCapacityBonus;

public record ChargedByCapacityAchievementRequest(string PersonId, DateTimeExt TimeExt, int BankId) : IRequest<long>;
public sealed class ChargedByCapacityBonusAchievementCommand : IRequestHandler<SpendMoneyBonusAchievementRequest, long>
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
                && x.chargeEndTime>= request.TimeExt.from.UtcDateTime && x.chargeEndTime< request.TimeExt.to.UtcDateTime)
            .Sum(x=> x.operation!.calculatedPayment ?? 0);
        return ValueTask.FromResult(payment);
    }
}
