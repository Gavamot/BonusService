using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire.Console;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.ChargedByCapacityBonus;

/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-411
/// </summary>
public class ChargedByCapacityBonusJob(MongoDbContext mongo, PostgresDbContext postgres, IDateTimeService dateTimeService, ILogger<ChargedByCapacityBonusJob> logger)
    : AccumulateByIntervalBonusJob(mongo, postgres, dateTimeService, logger)
{
    protected override BonusProgramType BonusProgramType => BonusProgramType.ChargedByCapacity;
    protected override Func<MongoSession, long> GetConditionField => (x) => x.operation?.calculatedConsume ?? 0;
}
