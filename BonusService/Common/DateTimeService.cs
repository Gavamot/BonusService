using BonusService.Common.Postgres.Entity;
namespace BonusService.Common;

public record DateTimeInterval(DateTimeOffset from, DateTimeOffset to);

public interface IDateTimeService
{
    DateTimeOffset GetNowUtc();
    DateTimeInterval GetCurrentMonth();
    DateTimeOffset GetStartOfCurrentDay();
    DateTimeInterval GetDateTimeInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now = default);
}


public class DateTimeService : IDateTimeService
{
    public DateTimeOffset GetNowUtc() => DateTimeOffset.UtcNow;
    private readonly static TimeSpan MoscowTimezone = new (3, 0, 0);

    protected DateTimeInterval GetCurrentMonthInner(DateTimeOffset now, int frequencyValue = 1)
    {
        var @from =  new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var @to = @from.AddMonths(1 * frequencyValue);
        return new DateTimeInterval(@from, to);
    }

    public DateTimeInterval GetCurrentMonth()
    {
        var now = GetNowUtc();
        return GetCurrentMonthInner(now);
    }

    protected DateTimeOffset GetStartOfCurrentDayInner(DateTimeOffset now)
    {
        return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
    }
    public DateTimeOffset GetStartOfCurrentDay()
    {
        var now = GetNowUtc();
        return GetStartOfCurrentDayInner(now);
    }
    public DateTimeInterval GetDateTimeInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now = default)
    {
        if (now == default) now = DateTimeOffset.UtcNow;
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return new(GetStartOfCurrentDayInner(now), GetStartOfCurrentDayInner(now) + TimeSpan.FromDays(1 * frequencyValue));
            case FrequencyTypes.Week : throw new NotImplementedException("Необходжимо найти либу или написать свой алгоритм для недельнольного предстваления из даты");
            case FrequencyTypes.Month : return GetCurrentMonthInner(now, frequencyValue);
            default: throw new NotImplementedException();
        }

    }
}
