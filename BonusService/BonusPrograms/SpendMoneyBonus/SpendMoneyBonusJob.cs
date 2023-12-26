using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
namespace BonusService.BonusPrograms.SpendMoneyBonus;

/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class SpendMoneyBonusJob(MongoDbContext mongo, PostgresDbContext postgres, IDateTimeService dateTimeService, ILogger<SpendMoneyBonusJob> logger)
    : AccumulateByIntervalBonusJob(mongo, postgres, dateTimeService, logger)
{
    protected override BonusProgramType BonusProgramType => BonusProgramType.SpendMoney;
    protected override Func<MongoSession, long> GetConditionField => (x) => x.operation?.calculatedPayment ?? 0;
}
