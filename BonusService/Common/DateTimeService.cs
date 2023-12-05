using BonusService.Common.Postgres.Entity;
using SharpCompress.Common;
namespace BonusService.Common;

public class DateInterval
{
    public DateInterval(DateTimeOffset from, DateTimeOffset to)
    {
        if (from > to) throw new ArgumentException();
        this.from = from;
        this.to = to;
    }
    public readonly DateTimeOffset from;
    public readonly DateTimeOffset to;
    protected static DateInterval GetPrevMonthsInner(DateTimeOffset now, int frequencyValue = 1)
    {
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var @from =  startOfMonth.AddMonths(frequencyValue * -1);
        var to = startOfMonth;
        return new DateInterval(@from, to);
    }

    protected static DateInterval GetPrevDaysInner(DateTimeOffset now, int frequencyValue = 1)
    {
        var startOfDay = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var start = startOfDay - TimeSpan.FromDays(frequencyValue * -1);
        var end = startOfDay;
        return new DateInterval(start, end);
    }

    public static DateInterval GetCurrentDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return GetPrevDaysInner(now, frequencyValue);
            case FrequencyTypes.Week : throw new NotImplementedException("Необходжимо найти либу или написать свой алгоритм для недельнольного предстваления из даты");
            case FrequencyTypes.Month : return GetPrevMonthsInner(now, frequencyValue);
            default: throw new NotImplementedException();
        }
    }

    public static DateInterval GetPrevDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        if (frequencyType != FrequencyTypes.Month) throw new NotImplementedException();
        // TODO необходимо сделать правильное вычитание в зависимости от периода так как
        // для просмотра достижений нужны данные за текущий период
        // а для начислений зха прошедший
        return GetCurrentDateInterval(frequencyType, frequencyValue, now.AddMonths(-1));
    }
    public override string ToString() => $"{from.Day}.{from.Month}.{from.Year}-{to.Day}.{to.Month}.{to.Year}";
};

public interface IDateTimeService
{
    DateTimeOffset GetNowUtc();
}

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset GetNowUtc() => DateTimeOffset.UtcNow;
    private readonly static TimeSpan MoscowTimezone = new (3, 0, 0);
}
