using BonusService.Common.Postgres.Entity;
namespace BonusService.Common;

public record DateInterval
{
    public readonly DateTimeOffset from;
    public readonly DateTimeOffset to;

    public DateInterval(DateTimeOffset from, DateTimeOffset to)
    {
        if (from > to) throw new ArgumentException();
        this.from = from;
        this.to = to;
    }

    protected static DateInterval GetFromNowToNextDays(DateTimeOffset now, int frequencyValue = 1)
    {
        var start = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var end = start + TimeSpan.FromDays(frequencyValue);
        return new DateInterval(start, end);
    }

    protected static DateInterval GetFromNowToNextMonths(DateTimeOffset now, int frequencyValue = 1)
    {
        var start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var end =  start.AddMonths(frequencyValue);
        return new DateInterval(start, end);
    }

    public static DateInterval GetFromNowToFutureDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: throw new NotImplementedException();//return GetFromNowToNextDays(now, frequencyValue);
            case FrequencyTypes.Week : throw new NotImplementedException("Необходжимо найти либу или написать свой алгоритм для недельнольного предстваления из даты");
            case FrequencyTypes.Month : return GetFromNowToNextMonths(now, frequencyValue);
            default: throw new NotImplementedException();
        }
    }

    protected static DateInterval GetFromPrevMonthsToNow(DateTimeOffset now, int frequencyValue = 1)
    {
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var @from =  startOfMonth.AddMonths(frequencyValue * -1);
        var to = startOfMonth;
        return new DateInterval(@from, to);
    }

    protected static DateInterval GetFromPrevDaysToNow(DateTimeOffset now, int frequencyValue = 1)
    {
        var startOfDay = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var start = startOfDay - TimeSpan.FromDays(frequencyValue * -1);
        var end = startOfDay;
        return new DateInterval(start, end);
    }


    public static DateInterval GetPrevToNowDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        if (frequencyType != FrequencyTypes.Month) throw new NotImplementedException();
        switch (frequencyType)
        {
            case FrequencyTypes.Day: throw new NotImplementedException(); //return GetFromPrevDaysToNow(now, frequencyValue);
            case FrequencyTypes.Week : throw new NotImplementedException("Необходжимо найти либу или написать свой алгоритм для недельнольного предстваления из даты");
            case FrequencyTypes.Month : return GetFromPrevMonthsToNow(now, frequencyValue);
            default: throw new NotImplementedException();
        }
    }
    public override string ToString() => $"{from.Day}.{from.Month}.{from.Year}-{to.Day}.{to.Month}.{to.Year}";
};
