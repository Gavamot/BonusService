using BonusService.Common.Postgres.Entity;
namespace BonusService.Common;

public record DateTimeExt
{
    public readonly DateTimeOffset from;
    public readonly DateTimeOffset to;

    public DateTimeExt(DateTimeOffset from, DateTimeOffset to)
    {
        if (from > to) throw new ArgumentException();
        this.from = from;
        this.to = to;
    }

    public static DateTimeExt GetFromNowToFutureDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return now.GetFromNowToNextDays(frequencyValue);
            case FrequencyTypes.Week : return now.GetFromNowToNextWeeks(frequencyValue);
            case FrequencyTypes.Month : return now.GetFromNowToNextMonths(frequencyValue);
            default: throw new NotImplementedException();
        }
    }


    public static DateTimeExt GetPrevToNowDateInterval(FrequencyTypes frequencyType, int frequencyValue, DateTimeOffset now)
    {
        switch (frequencyType)
        {
            case FrequencyTypes.Day: return now.GetFromPrevDaysToNow(frequencyValue);
            case FrequencyTypes.Week : return now.GetFromPrevToNowWeeks(frequencyValue);
            case FrequencyTypes.Month : return now.GetFromPrevToNowMonths(frequencyValue);
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
        return new DateTimeOffset(dt.AddDays(-1 * diff).Date, TimeSpan.Zero);
    }

    public static DateTimeOffset StartOfMonth(this DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
    }

    public static DateTimeExt GetFromNowToNextDays(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start = now.StartOfDay();
        var end = start.AddDays(frequencyValue);
        return new DateTimeExt(start, end);
    }

    public  static DateTimeExt GetFromNowToNextWeeks(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start =  now.StartOfWeek();
        var end = start.AddDays(frequencyValue * 7);
        return new DateTimeExt(start, end);
    }

    public  static DateTimeExt GetFromNowToNextMonths(this DateTimeOffset now, int frequencyValue = 1)
    {
        var start = now.StartOfMonth();
        var end =  start.AddMonths(frequencyValue);
        return new DateTimeExt(start, end);
    }

    public static DateTimeExt GetFromPrevDaysToNow(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfDay();
        var start = end.AddDays(frequencyValue * -1);
        return new DateTimeExt(start, end);
    }

    public static DateTimeExt GetFromPrevToNowWeeks(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfWeek();
        var start = end.AddDays(frequencyValue * -7);
        return new DateTimeExt(start, end);
    }

    public static DateTimeExt GetFromPrevToNowMonths(this DateTimeOffset now, int frequencyValue = 1)
    {
        var end = now.StartOfMonth();
        var start = end.AddMonths(frequencyValue * -1);
        return new DateTimeExt(start, end);
    }
}
