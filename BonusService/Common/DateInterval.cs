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

    public static DateInterval GetFromNowToFutureDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return now.GetFromNowToNextDays(frequencyValue);
            case FrequencyTypes.Week : return now.GetFromNowToNextWeeks(frequencyValue);
            case FrequencyTypes.Month : return now.GetFromNowToNextMonths(frequencyValue);
            default: throw new NotImplementedException();
        }
    }


    public static DateInterval GetPrevToNowDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return now.GetFromPrevDaysToNow(frequencyValue);
            case FrequencyTypes.Week : return now.GetFromPrevToNowWeeks(frequencyValue);
            case FrequencyTypes.Month : return now.GetFromNowToNextMonths(frequencyValue);
            default: throw new NotImplementedException();
        }
    }
    public override string ToString() => $"{from.Day}.{from.Month}.{from.Year}-{to.Day}.{to.Month}.{to.Year}";
}

public static class DateTimeExtensions
{
    public static DateTimeOffset StartOfDay(this DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Offset);
    }

    public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    public static DateTimeOffset StartOfMonth(this DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
    }

    public static DateInterval GetFromNowToNextDays(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start = now.StartOfDay();
        var end = start.AddDays(frequencyValue);
        return new DateInterval(start, end);
    }

    public  static DateInterval GetFromNowToNextWeeks(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start =  now.StartOfWeek();
        var end = start.AddDays(frequencyValue * 7);
        return new DateInterval(start, end);
    }

    public  static DateInterval GetFromNowToNextMonths(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start = now.StartOfMonth();
        var end =  start.AddMonths(frequencyValue);
        return new DateInterval(start, end);
    }

    public static DateInterval GetFromPrevDaysToNow(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfDay();
        var start = end.AddDays(frequencyValue * -1);
        return new DateInterval(start, end);
    }

    public static DateInterval GetFromPrevToNowWeeks(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfWeek();
        var start = end.AddDays(frequencyValue * -7);
        return new DateInterval(start, end);
    }

    public static DateInterval GetFromPrevToNowMonths(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfMonth();
        var start = end.AddMonths(frequencyValue * -1);
        return new DateInterval(start, end);
    }
}
