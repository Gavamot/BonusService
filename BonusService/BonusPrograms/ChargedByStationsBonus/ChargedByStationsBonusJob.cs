using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
namespace BonusService.BonusPrograms.ChargedByStationsBonus;

/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class ChargedByStationsBonusJob(MongoDbContext mongo, BonusDbContext bonus, IDateTimeService dateTimeService, ILogger<ChargedByStationsBonusJob> logger)
    : AccumulateByIntervalBonusJob(mongo, bonus, dateTimeService, logger)
{
    protected override BonusProgramType BonusProgramType => BonusProgramType.ChargedByStations;
    private readonly HashSet<string> Stations = new();
    protected override Func<MongoSession, long> GetConditionField => (x) => Stations.Add(x.operation?.cpName ?? "") ? 1 : 0;
}
