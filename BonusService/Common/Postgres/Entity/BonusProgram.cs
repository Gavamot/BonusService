#nullable enable

using BonusService.Common;
#pragma warning disable CS8618
namespace BonusService.Postgres;

public enum FrequencyTypes
{
    /// <summary>
    /// С 1 числа месяца - поледниего числа месяца (включительно)
    /// </summary>
    Month,
    /// <summary>
    /// С Понедельника по Воскресенье (включительно)
    /// </summary>
    Week,
    /// <summary>
    /// Каждый календарный день
    /// </summary>
    Day,
}

public enum BonusProgramType
{
    /// <summary>
    /// заряжено Х кВт
    /// </summary>
    ChargedByCapacity,
    /// <summary>
    /// потрачено Х денег
    /// </summary>
    ChargedByStations,
    /// <summary>
    /// зарядка на Х разных станциях
    /// </summary>
    SpendMoney,
    /// <summary>
    /// день рождения
    /// </summary>
    Birthday
}

public class BonusProgram : ICatalogEntity, IDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public BonusProgramType BonusProgramType { get; set; }
    public string Description { get; set; } = "";
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset? DateStop { get; set; }
    public int BankId { get; set; }

    public string ExecutionCron { get; set; }
    public FrequencyTypes FrequencyType { get; set; }
    public int  FrequencyValue { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public List<BonusProgramLevel> ProgramLevels { get; set; }
    public List<BonusProgramHistory> BonusProgramHistory { get; set; }
    public string CreateMark() => $"{Id}_{Name}";
    public DateTimeInterval CreateDateTimeInterval(IDateTimeService dateTimeService)
    {
        if (FrequencyValue == 1)
        {
            var now = dateTimeService.GetNowUtc();
            switch (FrequencyType)
            {
                case FrequencyTypes.Day : return new DateTimeInterval(
                    new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset),
                    new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset) + TimeSpan.FromDays(1));
                case FrequencyTypes.Week : throw new NotImplementedException("Необходжимо найти либу или написать свой алгоритм для недельнольного предстваления из даты");
                case FrequencyTypes.Month : return dateTimeService.GetCurrentMonth();
                default: throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
